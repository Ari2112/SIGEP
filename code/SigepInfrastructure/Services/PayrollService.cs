using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Payroll;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class PayrollService : IPayrollService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public PayrollService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<IEnumerable<PayrollDto>> GetAllAsync()
    {
        var payrolls = await _context.Payrolls
            .Include(p => p.ProcessedBy)
            .Include(p => p.ApprovedBy)
            .Include(p => p.PayrollStatus)
            .Include(p => p.PayrollPeriodType)
            .OrderByDescending(p => p.PeriodYear)
            .ThenByDescending(p => p.PeriodMonth)
            .ThenByDescending(p => p.PayrollPeriodTypeId)
            .ToListAsync();

        return payrolls.Select(p => MapToDto(p, false));
    }

    public async Task<PayrollDto?> GetByIdAsync(int id)
    {
        var payroll = await _context.Payrolls
            .Include(p => p.ProcessedBy)
            .Include(p => p.ApprovedBy)
            .Include(p => p.PayrollStatus)
            .Include(p => p.PayrollPeriodType)
            .Include(p => p.Details)
                .ThenInclude(d => d.Employee)
                    .ThenInclude(e => e!.Position)
            .Include(p => p.Details)
                .ThenInclude(d => d.Deductions)
                    .ThenInclude(dd => dd.DeductionType)
            .Include(p => p.Details)
                .ThenInclude(d => d.Benefits)
                    .ThenInclude(b => b.BenefitType)
            .FirstOrDefaultAsync(p => p.Id == id);

        return payroll == null ? null : MapToDto(payroll, true);
    }

    public async Task<PayrollDto> GenerateAsync(CreatePayrollDto dto, int userId)
    {
        var annulledStatus = await GetPayrollStatusAsync("Anulada");
        var draftStatus = await GetPayrollStatusAsync("Borrador");
        var processedStatus = await GetPayrollStatusAsync("Procesada");

        var periodType = await _context.PayrollPeriodTypes
            .FirstOrDefaultAsync(pt => pt.Id == dto.PeriodType);

        if (periodType == null)
            throw new ArgumentException("Tipo de período de planilla no válido");

        var existing = await _context.Payrolls
            .FirstOrDefaultAsync(p =>
                p.PeriodYear == dto.PeriodYear &&
                p.PeriodMonth == dto.PeriodMonth &&
                p.PayrollPeriodTypeId == dto.PeriodType);

        if (existing != null && existing.PayrollStatusId != annulledStatus.Id)
        {
            throw new InvalidOperationException(
                $"Ya existe una planilla para el período {dto.PeriodYear}/{dto.PeriodMonth} tipo {periodType.Name}");
        }

        var (startDate, endDate) = GetPeriodDates(dto.PeriodYear, dto.PeriodMonth, periodType.Name);

        var payroll = new Payroll
        {
            PeriodYear = dto.PeriodYear,
            PeriodMonth = dto.PeriodMonth,
            PayrollPeriodTypeId = periodType.Id,
            PeriodStartDate = startDate,
            PeriodEndDate = endDate,
            PayrollStatusId = draftStatus.Id,
            ProcessedById = userId,
            ProcessedAt = DateTime.UtcNow,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payrolls.Add(payroll);
        await _context.SaveChangesAsync();

        var activeStatus = await _context.EmployeeStatuses
            .FirstAsync(es => es.Name == "Activo");

        var employees = await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.Schedule)
            .Where(e => e.EmployeeStatusId == activeStatus.Id && e.HireDate <= endDate)
            .ToListAsync();

        var deductionTypes = await _context.DeductionTypes
            .Where(d => d.IsActive)
            .ToListAsync();

        var benefitTypes = await _context.BenefitTypes
            .Where(b => b.IsActive)
            .ToListAsync();

        int workDays = CountWorkDays(startDate, endDate);

        decimal totalGross = 0;
        decimal totalDeductions = 0;
        decimal totalBenefits = 0;
        decimal totalNet = 0;

        foreach (var emp in employees)
        {
            // Calcular salario base del período (mensual o quincenal)
            decimal fullPeriodSalary = IsMonthlyPeriod(periodType.Name)
                ? emp.BaseSalary
                : emp.BaseSalary / 2;

            // Calcular proporcional si el empleado ingresó durante este período
            decimal periodSalary;
            if (emp.HireDate > startDate && emp.HireDate <= endDate)
            {
                // Días trabajados en el período
                int totalPeriodDays = IsMonthlyPeriod(periodType.Name) ? 30 : 15;
                int daysWorked = (endDate - emp.HireDate.Date).Days + 1;
                daysWorked = Math.Min(daysWorked, totalPeriodDays);
                periodSalary = Math.Round(fullPeriodSalary * daysWorked / totalPeriodDays, 2);
            }
            else
            {
                periodSalary = fullPeriodSalary;
            }

            var overtimeRecords = await _context.OvertimeRecords
                .Where(o =>
                    o.EmployeeId == emp.Id &&
                    o.Date >= startDate &&
                    o.Date <= endDate &&
                    o.Status == OvertimeStatus.Aprobada)
                .ToListAsync();

            decimal overtimeHours = overtimeRecords.Sum(o => o.TotalHours);
            decimal overtimeAmount = overtimeRecords.Sum(o => o.TotalAmount);

            // Permisos sin goce aprobados en el período — se descuentan del salario
            var approvedRequestStatus = await _context.RequestStatuses
                .FirstOrDefaultAsync(s => s.Name == "Aprobada");

            decimal unpaidPermissionDays = 0;
            if (approvedRequestStatus != null)
            {
                var unpaidPermissions = await _context.PermissionRequests
                    .Include(pr => pr.PermissionType)
                    .Where(pr =>
                        pr.EmployeeId == emp.Id &&
                        pr.RequestStatusId == approvedRequestStatus.Id &&
                        pr.StartDate >= startDate &&
                        pr.StartDate <= endDate &&
                        pr.PermissionType != null &&
                        !pr.PermissionType.IsPaid)
                    .ToListAsync();

                unpaidPermissionDays = unpaidPermissions.Sum(pr => pr.DurationDays);
            }

            decimal dailySalary = periodSalary / (IsMonthlyPeriod(periodType.Name) ? 30m : 15m);
            decimal unpaidPermissionDeduction = Math.Round(dailySalary * unpaidPermissionDays, 2);

            decimal grossSalary = periodSalary + overtimeAmount - unpaidPermissionDeduction;

            var detail = new PayrollDetail
            {
                PayrollId = payroll.Id,
                EmployeeId = emp.Id,
                BaseSalary = periodSalary,
                WorkedDays = workDays,
                OvertimeHours = overtimeHours,
                OvertimeAmount = overtimeAmount,
                GrossSalary = grossSalary,
                CreatedAt = DateTime.UtcNow
            };

            decimal detailDeductions = 0;
            decimal detailBenefits = 0;
// Salario mensual equivalente para calcular renta correctamente
// periodType es una entidad con campo Name, no un enum
bool isMensual = IsMonthlyPeriod(periodType.Name);
decimal monthlyEquivalent = isMensual
    ? grossSalary
    : grossSalary * 2;

foreach (var dedType in deductionTypes)
{
    decimal amount;

    if (dedType.Name.Contains("Renta") || dedType.Name.Contains("renta"))
    {
        // Impuesto sobre la renta con tramos progresivos CR
        decimal monthlyTax = CalculateIncomeTax(monthlyEquivalent);
        // Si es quincenal, cobrar la mitad del impuesto mensual
        amount = isMensual
            ? monthlyTax
            : Math.Round(monthlyTax / 2, 2);
    }
    else
    {
        amount = dedType.IsPercentage
            ? Math.Round(grossSalary * dedType.DefaultValue, 2)
            : dedType.DefaultValue;
    }
                var deduction = new PayrollDeduction
                {
                    DeductionTypeId = dedType.Id,
                    Amount = amount,
                    IsPercentage = dedType.IsPercentage,
                    PercentageValue = dedType.IsPercentage ? dedType.DefaultValue : null,
                    CreatedAt = DateTime.UtcNow
                };

                detail.Deductions.Add(deduction);
                detailDeductions += amount;
            }

            foreach (var benType in benefitTypes)
            {
                decimal amount = benType.IsPercentage
                    ? Math.Round(grossSalary * benType.DefaultValue, 2)
                    : benType.DefaultValue;

                var benefit = new PayrollBenefit
                {
                    BenefitTypeId = benType.Id,
                    Amount = amount,
                    IsPercentage = benType.IsPercentage,
                    PercentageValue = benType.IsPercentage ? benType.DefaultValue : null,
                    CreatedAt = DateTime.UtcNow
                };

                detail.Benefits.Add(benefit);
                detailBenefits += amount;
            }

            detail.TotalDeductions = detailDeductions;
detail.TotalBenefits = detailBenefits;
// Las cargas patronales (BenefitTypes) son costo del patrono
// NO se suman al salario neto del empleado
detail.NetSalary = grossSalary - detailDeductions;

            _context.PayrollDetails.Add(detail);
            await _context.SaveChangesAsync();

            totalGross += grossSalary;
            totalDeductions += detailDeductions;
            totalBenefits += detailBenefits;
            totalNet += detail.NetSalary;

            foreach (var ot in overtimeRecords)
            {
                ot.Status = OvertimeStatus.Pagada;
                ot.PayrollDetailId = detail.Id;
                ot.UpdatedAt = DateTime.UtcNow;
            }
        }

        payroll.TotalGrossSalary = totalGross;
        payroll.TotalDeductions = totalDeductions;
        payroll.TotalBenefits = totalBenefits;
        payroll.TotalNetSalary = totalNet;
        payroll.TotalEmployees = employees.Count;
        payroll.PayrollStatusId = processedStatus.Id;
        payroll.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId,
            "GENERATE",
            "PLANILLA",
            "Payroll",
            payroll.Id,
            description: $"Planilla generada: {dto.PeriodYear}/{dto.PeriodMonth} - {employees.Count} empleados - Total neto: {totalNet:C}"
        );

        return (await GetByIdAsync(payroll.Id))!;
    }

    public async Task<PayrollDto> ApproveAsync(int id, int userId, string? notes = null)
    {
        var payroll = await _context.Payrolls.FindAsync(id)
            ?? throw new ArgumentException("Planilla no encontrada");

        var processedStatus = await GetPayrollStatusAsync("Procesada");
        var approvedStatus = await GetPayrollStatusAsync("Aprobada");

        if (payroll.PayrollStatusId != processedStatus.Id)
            throw new InvalidOperationException("Solo se puede aprobar una planilla procesada");

        payroll.PayrollStatusId = approvedStatus.Id;
        payroll.ApprovedById = userId;
        payroll.ApprovedAt = DateTime.UtcNow;
        payroll.Notes = notes ?? payroll.Notes;
        payroll.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId,
            "APPROVE",
            "PLANILLA",
            "Payroll",
            id,
            description: $"Planilla aprobada: {payroll.PeriodYear}/{payroll.PeriodMonth}"
        );

        return (await GetByIdAsync(id))!;
    }

    public async Task<PayrollDto> AnnulAsync(int id, int userId, string? notes = null)
    {
        var payroll = await _context.Payrolls.FindAsync(id)
            ?? throw new ArgumentException("Planilla no encontrada");

        var annulledStatus = await GetPayrollStatusAsync("Anulada");

        if (payroll.PayrollStatusId == annulledStatus.Id)
            throw new InvalidOperationException("La planilla ya está anulada");

        payroll.PayrollStatusId = annulledStatus.Id;
        payroll.Notes = notes;
        payroll.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId,
            "ANNUL",
            "PLANILLA",
            "Payroll",
            id,
            description: $"Planilla anulada: {payroll.PeriodYear}/{payroll.PeriodMonth}"
        );

        return (await GetByIdAsync(id))!;
    }

    public async Task<IEnumerable<DeductionTypeDto>> GetDeductionTypesAsync()
    {
        return await _context.DeductionTypes
            .Where(d => d.IsActive)
            .Select(d => new DeductionTypeDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                IsPercentage = d.IsPercentage,
                DefaultValue = d.DefaultValue,
                IsActive = d.IsActive
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<BenefitTypeDto>> GetBenefitTypesAsync()
    {
        return await _context.BenefitTypes
            .Where(b => b.IsActive)
            .Select(b => new BenefitTypeDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                IsPercentage = b.IsPercentage,
                DefaultValue = b.DefaultValue,
                IsActive = b.IsActive
            })
            .ToListAsync();
    }

    private async Task<PayrollStatus> GetPayrollStatusAsync(string name)
    {
        var status = await _context.PayrollStatuses
            .FirstOrDefaultAsync(s => s.Name == name);

        if (status == null)
            throw new InvalidOperationException($"No existe el estado de planilla: {name}");

        return status;
    }

    private static (DateTime start, DateTime end) GetPeriodDates(int year, int month, string periodTypeName)
    {
        if (periodTypeName.Contains("Primera", StringComparison.OrdinalIgnoreCase))
        {
            return (
                new DateTime(year, month, 1),
                new DateTime(year, month, 15)
            );
        }

        if (periodTypeName.Contains("Segunda", StringComparison.OrdinalIgnoreCase))
        {
            return (
                new DateTime(year, month, 16),
                new DateTime(year, month, DateTime.DaysInMonth(year, month))
            );
        }

        return (
            new DateTime(year, month, 1),
            new DateTime(year, month, DateTime.DaysInMonth(year, month))
        );
    }

    private static bool IsMonthlyPeriod(string periodTypeName)
    {
        return periodTypeName.Contains("Mensual", StringComparison.OrdinalIgnoreCase);
    }

    private static int CountWorkDays(DateTime start, DateTime end)
    {
        int count = 0;

        for (var d = start; d <= end; d = d.AddDays(1))
        {
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                count++;
        }

        return count;
    }

    private static PayrollDto MapToDto(Payroll p, bool includeDetails)
    {
        var dto = new PayrollDto
        {
            Id = p.Id,
            PeriodYear = p.PeriodYear,
            PeriodMonth = p.PeriodMonth,
            PeriodType = p.PayrollPeriodType?.Name ?? string.Empty,
            PeriodStartDate = p.PeriodStartDate,
            PeriodEndDate = p.PeriodEndDate,
            Status = p.PayrollStatus?.Name ?? string.Empty,
            TotalGrossSalary = p.TotalGrossSalary,
            TotalDeductions = p.TotalDeductions,
            TotalBenefits = p.TotalBenefits,
            TotalNetSalary = p.TotalNetSalary,
            TotalEmployees = p.TotalEmployees,
            ProcessedByName = p.ProcessedBy?.Username,
            ProcessedAt = p.ProcessedAt,
            ApprovedByName = p.ApprovedBy?.Username,
            ApprovedAt = p.ApprovedAt,
            Notes = p.Notes,
            CreatedAt = p.CreatedAt
        };

        if (includeDetails)
        {
            dto.Details = p.Details.Select(d => new PayrollDetailDto
            {
                Id = d.Id,
                PayrollId = d.PayrollId,
                EmployeeId = d.EmployeeId,
                EmployeeName = d.Employee?.FullName ?? string.Empty,
                PositionName = d.Employee?.Position?.Name,
                BaseSalary = d.BaseSalary,
                WorkedDays = d.WorkedDays,
                OvertimeHours = d.OvertimeHours,
                OvertimeAmount = d.OvertimeAmount,
                GrossSalary = d.GrossSalary,
                TotalDeductions = d.TotalDeductions,
                TotalBenefits = d.TotalBenefits,
                NetSalary = d.NetSalary,
                Notes = d.Notes,
                Deductions = d.Deductions.Select(dd => new DeductionItemDto
                {
                    Id = dd.Id,
                    DeductionTypeName = dd.DeductionType?.Name ?? string.Empty,
                    Amount = dd.Amount,
                    IsPercentage = dd.IsPercentage,
                    PercentageValue = dd.PercentageValue
                }).ToList(),
                Benefits = d.Benefits.Select(b => new BenefitItemDto
                {
                    Id = b.Id,
                    BenefitTypeName = b.BenefitType?.Name ?? string.Empty,
                    Amount = b.Amount,
                    IsPercentage = b.IsPercentage,
                    PercentageValue = b.PercentageValue
                }).ToList()
            }).ToList();
        }

        return dto;
    }
    /// <summary>
/// Calcula el impuesto sobre la renta según tramos progresivos CR.
/// Tramos aproximados
/// Hasta ₡941.000: exento
/// ₡941.001 - ₡1.381.000: 10%
/// ₡1.381.001 - ₡2.423.000: 15%
/// ₡2.423.001 - ₡4.845.000: 20%
/// Más de ₡4.845.000: 25%
/// </summary>
private decimal CalculateIncomeTax(decimal monthlyGross)
{
    decimal tax = 0;

    if (monthlyGross <= 941000m)
        tax = 0;
    else if (monthlyGross <= 1381000m)
        tax = (monthlyGross - 941000m) * 0.10m;
    else if (monthlyGross <= 2423000m)
        tax = (440000m * 0.10m) + ((monthlyGross - 1381000m) * 0.15m);
    else if (monthlyGross <= 4845000m)
        tax = (440000m * 0.10m) + (1042000m * 0.15m) + ((monthlyGross - 2423000m) * 0.20m);
    else
        tax = (440000m * 0.10m) + (1042000m * 0.15m) + (2422000m * 0.20m) + ((monthlyGross - 4845000m) * 0.25m);

    return Math.Round(tax, 2);
}
}