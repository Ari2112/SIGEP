using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Settlement;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class SettlementService : ISettlementService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public SettlementService(
        ApplicationDbContext context,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _context = context;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<SettlementDto>> GetAllAsync()
    {
        var settlements = await _context.Settlements
            .Include(s => s.Employee)
                .ThenInclude(e => e!.Position)
            .Include(s => s.TerminationType)
            .Include(s => s.CalculatedBy)
            .Include(s => s.ApprovedBy)
            .Include(s => s.Deductions)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return settlements.Select(MapToDto);
    }

    public async Task<SettlementDto?> GetByIdAsync(int id)
    {
        var settlement = await _context.Settlements
            .Include(s => s.Employee)
                .ThenInclude(e => e!.Position)
            .Include(s => s.TerminationType)
            .Include(s => s.CalculatedBy)
            .Include(s => s.ApprovedBy)
            .Include(s => s.Deductions)
            .FirstOrDefaultAsync(s => s.Id == id);

        return settlement == null ? null : MapToDto(settlement);
    }

    public async Task<SettlementDto?> GetByEmployeeAsync(int employeeId)
    {
        var settlement = await _context.Settlements
            .Include(s => s.Employee)
                .ThenInclude(e => e!.Position)
            .Include(s => s.TerminationType)
            .Include(s => s.CalculatedBy)
            .Include(s => s.ApprovedBy)
            .Include(s => s.Deductions)
            .Where(s => s.EmployeeId == employeeId && s.Status != SettlementStatus.Anulada)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        return settlement == null ? null : MapToDto(settlement);
    }

    public async Task<SettlementDto> CalculateAsync(CalculateSettlementDto dto, int userId)
    {
        var employee = await _context.Employees
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId)
            ?? throw new ArgumentException("Empleado no encontrado");

        var terminationType = await _context.TerminationTypes
            .FirstOrDefaultAsync(t => t.Id == dto.TerminationType);

        if (terminationType == null)
            throw new ArgumentException("Tipo de terminación no encontrado");

        var existingSettlement = await _context.Settlements
            .FirstOrDefaultAsync(s =>
                s.EmployeeId == dto.EmployeeId &&
                s.Status != SettlementStatus.Anulada);

        if (existingSettlement != null)
            throw new InvalidOperationException("Ya existe una liquidación activa para este empleado");

        var hireDate = employee.HireDate;
        var terminationDate = dto.TerminationDate;

        if (terminationDate < hireDate)
            throw new InvalidOperationException("La fecha de terminación no puede ser anterior a la fecha de ingreso");

        var totalMonths = (terminationDate.Year - hireDate.Year) * 12 + terminationDate.Month - hireDate.Month;
        var workedYears = totalMonths / 12;
        var workedMonths = totalMonths % 12;
        var workedDays = (terminationDate - hireDate.AddMonths(totalMonths)).Days;

        var currentYear = terminationDate.Year;

        var vacBalance = await _context.VacationBalances
            .FirstOrDefaultAsync(v => v.EmployeeId == dto.EmployeeId && v.Year == currentYear);

        decimal pendingVacDays = vacBalance?.AvailableDays ?? 0;

        decimal lastSalary = employee.BaseSalary;
        decimal dailySalary = lastSalary / 30;

        decimal vacationAmount = pendingVacDays * dailySalary;

        int monthsThisYear = terminationDate.Month;
        decimal proportionalBonus = (lastSalary / 12) * monthsThisYear;

        decimal severance = 0;

        if (terminationType.HasSeverance)
        {
            severance = lastSalary * workedYears;

            if (workedYears == 0 && totalMonths >= 3)
                severance = lastSalary * 0.5m;
        }

        decimal totalDeductions = dto.AdditionalDeductions.Sum(d => d.Amount);
        decimal grossTotal = vacationAmount + proportionalBonus + severance;
        decimal netTotal = grossTotal - totalDeductions;

        var settlement = new Settlement
        {
            EmployeeId = dto.EmployeeId,
            TerminationTypeId = terminationType.Id,
            HireDate = hireDate,
            TerminationDate = terminationDate,
            LastSalary = lastSalary,
            AverageSalary = lastSalary,
            WorkedYears = workedYears,
            WorkedMonths = workedMonths,
            WorkedDays = workedDays,
            PendingVacationDays = pendingVacDays,
            VacationAmount = vacationAmount,
            ProportionalBonus = proportionalBonus,
            SeveranceAmount = severance,
            OtherBenefits = 0,
            TotalDeductions = totalDeductions,
            GrossTotal = grossTotal,
            NetTotal = netTotal,
            Status = SettlementStatus.Calculada,
            CalculatedById = userId,
            CalculatedAt = DateTime.UtcNow,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var deduction in dto.AdditionalDeductions)
        {
            settlement.Deductions.Add(new SettlementDeduction
            {
                Description = deduction.Description,
                Amount = deduction.Amount,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId,
            "CALCULATE",
            "LIQUIDACIONES",
            "Settlement",
            settlement.Id,
            description: $"Liquidación calculada para {employee.FullName}: Total neto {netTotal:C}"
        );

        return (await GetByIdAsync(settlement.Id))!;
    }

    public async Task<SettlementDto> ApproveAsync(int id, int userId, string? notes = null)
    {
        var settlement = await _context.Settlements.FindAsync(id)
            ?? throw new ArgumentException("Liquidación no encontrada");

        if (settlement.Status != SettlementStatus.Calculada)
            throw new InvalidOperationException("Solo se puede aprobar una liquidación calculada");

        settlement.Status = SettlementStatus.Aprobada;
        settlement.ApprovedById = userId;
        settlement.ApprovedAt = DateTime.UtcNow;
        settlement.Notes = notes ?? settlement.Notes;
        settlement.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId,
            "APPROVE",
            "LIQUIDACIONES",
            "Settlement",
            id,
            description: "Liquidación aprobada"
        );

        return (await GetByIdAsync(id))!;
    }

    public async Task<SettlementDto> MarkAsPaidAsync(int id, int userId)
    {
        var settlement = await _context.Settlements.FindAsync(id)
            ?? throw new ArgumentException("Liquidación no encontrada");

        if (settlement.Status != SettlementStatus.Aprobada)
            throw new InvalidOperationException("Solo se puede marcar como pagada una liquidación aprobada");

        settlement.Status = SettlementStatus.Pagada;
        settlement.UpdatedAt = DateTime.UtcNow;

        var employee = await _context.Employees.FindAsync(settlement.EmployeeId);

        if (employee != null)
        {
            var liquidatedStatus = await GetEmployeeStatusForSettlementAsync();

            employee.EmployeeStatusId = liquidatedStatus.Id;
            employee.TerminationDate = settlement.TerminationDate;
            employee.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId,
            "PAY",
            "LIQUIDACIONES",
            "Settlement",
            id,
            description: "Liquidación marcada como pagada"
        );

        return (await GetByIdAsync(id))!;
    }

    private async Task<EmployeeStatus> GetEmployeeStatusForSettlementAsync()
    {
        var liquidatedStatus = await _context.EmployeeStatuses
            .FirstOrDefaultAsync(s => s.Name == "Liquidado");

        if (liquidatedStatus != null)
            return liquidatedStatus;

        var inactiveStatus = await _context.EmployeeStatuses
            .FirstOrDefaultAsync(s => s.Name == "Inactivo");

        if (inactiveStatus != null)
            return inactiveStatus;

        throw new InvalidOperationException("No existe estado de empleado Liquidado o Inactivo");
    }

    private static SettlementDto MapToDto(Settlement s)
    {
        return new SettlementDto
        {
            Id = s.Id,
            EmployeeId = s.EmployeeId,
            EmployeeName = s.Employee?.FullName ?? string.Empty,
            TerminationType = s.TerminationType?.Name ?? string.Empty,
            HireDate = s.HireDate,
            TerminationDate = s.TerminationDate,
            LastSalary = s.LastSalary,
            AverageSalary = s.AverageSalary,
            WorkedYears = s.WorkedYears,
            WorkedMonths = s.WorkedMonths,
            WorkedDays = s.WorkedDays,
            PendingVacationDays = s.PendingVacationDays,
            VacationAmount = s.VacationAmount,
            ProportionalBonus = s.ProportionalBonus,
            SeveranceAmount = s.SeveranceAmount,
            OtherBenefits = s.OtherBenefits,
            TotalDeductions = s.TotalDeductions,
            GrossTotal = s.GrossTotal,
            NetTotal = s.NetTotal,
            Status = s.Status.ToString(),
            CalculatedByName = s.CalculatedBy?.Username ?? string.Empty,
            CalculatedAt = s.CalculatedAt,
            ApprovedByName = s.ApprovedBy?.Username,
            ApprovedAt = s.ApprovedAt,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt,
            Deductions = s.Deductions.Select(d => new SettlementDeductionDto
            {
                Id = d.Id,
                Description = d.Description,
                Amount = d.Amount
            }).ToList()
        };
    }
}
