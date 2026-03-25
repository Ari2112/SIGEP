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
            .OrderByDescending(p => p.PeriodYear)
            .ThenByDescending(p => p.PeriodMonth)
            .ThenByDescending(p => p.PeriodType)
            .ToListAsync();

        return payrolls.Select(p => MapToDto(p, false));
    }

    public async Task<PayrollDto?> GetByIdAsync(int id)
    {
        var payroll = await _context.Payrolls
            .Include(p => p.ProcessedBy)
            .Include(p => p.ApprovedBy)
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
        // Verificar si ya existe una planilla para ese período
        var existing = await _context.Payrolls
            .FirstOrDefaultAsync(p => p.PeriodYear == dto.PeriodYear
                                   && p.PeriodMonth == dto.PeriodMonth
                                   && (int)p.PeriodType == dto.PeriodType);

        if (existing != null && existing.Status != PayrollStatus.Anulada)
            throw new InvalidOperationException($"Ya existe una planilla para el período {dto.PeriodYear}/{dto.PeriodMonth} tipo {dto.PeriodType}");

        var periodType = (PayrollPeriodType)dto.PeriodType;
        var (startDate, endDate) = GetPeriodDates(dto.PeriodYear, dto.PeriodMonth, periodType);

        var payroll = new Payroll
        {
            PeriodYear = dto.PeriodYear,
            PeriodMonth = dto.PeriodMonth,
            PeriodType = periodType,
            PeriodStartDate = startDate,
            PeriodEndDate = endDate,
            Status = PayrollStatus.Procesando,
            ProcessedById = userId,
            ProcessedAt = DateTime.UtcNow,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payrolls.Add(payroll);
        await _context.SaveChangesAsync();

        // Obtener empleados activos
        var employees = await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.Schedule)
            .Where(e => e.Status == EmployeeStatus.Activo && e.HireDate <= endDate)
            .ToListAsync();

        // Obtener deducciones y beneficios configurados
        var deductionTypes = await _context.DeductionTypes.Where(d => d.IsActive).ToListAsync();
        var benefitTypes = await _context.BenefitTypes.Where(b => b.IsActive).ToListAsync();

        // Número de días laborales en el período
        int workDays = CountWorkDays(startDate, endDate);

        decimal totalGross = 0, totalDeductions = 0, totalBenefits = 0, totalNet = 0;

        foreach (var emp in employees)
        {
            // Calcular salario proporcional
            decimal dailySalary = emp.BaseSalary / 30;
            decimal periodSalary = periodType == PayrollPeriodType.Mensual
                ? emp.BaseSalary
                : emp.BaseSalary / 2;

            // Horas extra aprobadas en el período
            var overtimeRecords = await _context.OvertimeRecords
                .Where(o => o.EmployeeId == emp.Id
                         && o.Date >= DateOnly.FromDateTime(startDate)
                         && o.Date <= DateOnly.FromDateTime(endDate)
                         && o.Status == OvertimeStatus.Aprobada)
                .ToListAsync();

            decimal overtimeHours = overtimeRecords.Sum(o => o.TotalHours);
            decimal overtimeAmount = overtimeRecords.Sum(o => o.TotalAmount);

            decimal grossSalary = periodSalary + overtimeAmount;

            // Calcular deducciones
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

            foreach (var dedType in deductionTypes)
            {
                decimal amount = dedType.IsPercentage
                    ? Math.Round(grossSalary * dedType.DefaultValue, 2)
                    : dedType.DefaultValue;

                var ded = new PayrollDeduction
                {
                    DeductionTypeId = dedType.Id,
                    Amount = amount,
                    IsPercentage = dedType.IsPercentage,
                    PercentageValue = dedType.IsPercentage ? dedType.DefaultValue : null,
                    CreatedAt = DateTime.UtcNow
                };
                detail.Deductions.Add(ded);
                detailDeductions += amount;
            }

            foreach (var benType in benefitTypes)
            {
                decimal amount = benType.IsPercentage
                    ? Math.Round(grossSalary * benType.DefaultValue, 2)
                    : benType.DefaultValue;

                var ben = new PayrollBenefit
                {
                    BenefitTypeId = benType.Id,
                    Amount = amount,
                    IsPercentage = benType.IsPercentage,
                    PercentageValue = benType.IsPercentage ? benType.DefaultValue : null,
                    CreatedAt = DateTime.UtcNow
                };
                detail.Benefits.Add(ben);
                detailBenefits += amount;
            }

            detail.TotalDeductions = detailDeductions;
            detail.TotalBenefits = detailBenefits;
            detail.NetSalary = grossSalary - detailDeductions + detailBenefits;

            _context.PayrollDetails.Add(detail);

            totalGross += grossSalary;
            totalDeductions += detailDeductions;
            totalBenefits += detailBenefits;
            totalNet += detail.NetSalary;

            // Marcar horas extra como pagadas
            foreach (var ot in overtimeRecords)
            {
                ot.Status = OvertimeStatus.Pagada;
                ot.UpdatedAt = DateTime.UtcNow;
            }
        }

        payroll.TotalGrossSalary = totalGross;
        payroll.TotalDeductions = totalDeductions;
        payroll.TotalBenefits = totalBenefits;
        payroll.TotalNetSalary = totalNet;
        payroll.TotalEmployees = employees.Count;
        payroll.Status = PayrollStatus.Completada;
        payroll.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(userId, "GENERATE", "PLANILLA", "Payroll", payroll.Id,
            description: $"Planilla generada: {dto.PeriodYear}/{dto.PeriodMonth} - {employees.Count} empleados - Total neto: {totalNet:C}");

        return (await GetByIdAsync(payroll.Id))!;
    }

    public async Task<PayrollDto> ApproveAsync(int id, int userId, string? notes = null)
    {
        var payroll = await _context.Payrolls.FindAsync(id)
            ?? throw new ArgumentException("Planilla no encontrada");

        if (payroll.Status != PayrollStatus.Completada)
            throw new InvalidOperationException("Solo se puede aprobar una planilla completada");

        payroll.ApprovedById = userId;
        payroll.ApprovedAt = DateTime.UtcNow;
        payroll.Notes = notes ?? payroll.Notes;
        payroll.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync(userId, "APPROVE", "PLANILLA", "Payroll", id,
            description: $"Planilla aprobada: {payroll.PeriodYear}/{payroll.PeriodMonth}");

        return (await GetByIdAsync(id))!;
    }

    public async Task<PayrollDto> AnnulAsync(int id, int userId, string? notes = null)
    {
        var payroll = await _context.Payrolls.FindAsync(id)
            ?? throw new ArgumentException("Planilla no encontrada");

        if (payroll.Status == PayrollStatus.Anulada)
            throw new InvalidOperationException("La planilla ya está anulada");

        payroll.Status = PayrollStatus.Anulada;
        payroll.Notes = notes;
        payroll.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync(userId, "ANNUL", "PLANILLA", "Payroll", id,
            description: $"Planilla anulada: {payroll.PeriodYear}/{payroll.PeriodMonth}");

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

    private static (DateTime start, DateTime end) GetPeriodDates(int year, int month, PayrollPeriodType type)
    {
        return type switch
        {
            PayrollPeriodType.PrimeraQuincena => (new DateTime(year, month, 1), new DateTime(year, month, 15)),
            PayrollPeriodType.SegundaQuincena => (new DateTime(year, month, 16), new DateTime(year, month, DateTime.DaysInMonth(year, month))),
            _ => (new DateTime(year, month, 1), new DateTime(year, month, DateTime.DaysInMonth(year, month)))
        };
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
            PeriodType = p.PeriodType.ToString(),
            PeriodStartDate = p.PeriodStartDate,
            PeriodEndDate = p.PeriodEndDate,
            Status = p.Status.ToString(),
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
}
