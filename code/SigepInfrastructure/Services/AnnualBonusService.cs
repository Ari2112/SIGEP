using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.AnnualBonus;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class AnnualBonusService : IAnnualBonusService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public AnnualBonusService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<IEnumerable<AnnualBonusDto>> GetAllAsync()
    {
        var bonuses = await _context.AnnualBonuses
            .Include(ab => ab.CalculatedBy)
            .Include(ab => ab.ApprovedBy)
            .OrderByDescending(ab => ab.Year)
            .ToListAsync();

        return bonuses.Select(ab => MapToDto(ab, false));
    }

    public async Task<AnnualBonusDto?> GetByIdAsync(int id)
    {
        var bonus = await _context.AnnualBonuses
            .Include(ab => ab.CalculatedBy)
            .Include(ab => ab.ApprovedBy)
            .Include(ab => ab.Details)
                .ThenInclude(d => d.Employee)
                    .ThenInclude(e => e!.Position)
            .FirstOrDefaultAsync(ab => ab.Id == id);

        return bonus == null ? null : MapToDto(bonus, true);
    }

    public async Task<AnnualBonusDto?> GetByYearAsync(int year)
    {
        var bonus = await _context.AnnualBonuses
            .Include(ab => ab.CalculatedBy)
            .Include(ab => ab.ApprovedBy)
            .Include(ab => ab.Details)
                .ThenInclude(d => d.Employee)
                    .ThenInclude(e => e!.Position)
            .FirstOrDefaultAsync(ab => ab.Year == year);

        return bonus == null ? null : MapToDto(bonus, true);
    }

    public async Task<AnnualBonusDto> CalculateAsync(CalculateAnnualBonusDto dto, int userId)
    {
        var existing = await _context.AnnualBonuses
            .FirstOrDefaultAsync(ab => ab.Year == dto.Year && ab.Status != AnnualBonusStatus.Anulado);

        if (existing != null)
            throw new InvalidOperationException($"Ya existe un aguinaldo calculado para el año {dto.Year}");

        // Período legal del aguinaldo en Costa Rica: 1 de diciembre del año anterior
// al 30 de noviembre del año en curso (Código de Trabajo, Art. 229)
var periodStart = new DateTime(dto.Year - 1, 12, 1);
var periodEnd = new DateTime(dto.Year, 11, 30);

        var bonus = new AnnualBonus
        {
            Year = dto.Year,
            PeriodStartDate = periodStart,
            PeriodEndDate = periodEnd,
            Status = AnnualBonusStatus.Calculado,
            CalculatedById = userId,
            CalculatedAt = DateTime.UtcNow,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.AnnualBonuses.Add(bonus);
        await _context.SaveChangesAsync();

        var activeStatus = await _context.EmployeeStatuses
            .FirstAsync(es => es.Name == "Activo");

        var employees = await _context.Employees
            .Include(e => e.Position)
            .Where(e => e.EmployeeStatusId == activeStatus.Id && e.HireDate.Year <= dto.Year)
            .ToListAsync();

        decimal totalAmount = 0;

        foreach (var emp in employees)
{
    // Meses trabajados dentro del período legal (1 dic año anterior → 30 nov año en curso)
    DateTime empStart = emp.HireDate > periodStart ? emp.HireDate : periodStart;
    DateTime empEnd = periodEnd;

    int workedMonths = ((empEnd.Year - empStart.Year) * 12) + empEnd.Month - empStart.Month;
    if (empStart.Day > 1) workedMonths--;
    if (workedMonths < 0) workedMonths = 0;
    if (workedMonths > 12) workedMonths = 12;

    decimal averageSalary = emp.BaseSalary;
    decimal proportionalAmount = Math.Round((averageSalary / 12m) * workedMonths, 2);

            var detail = new AnnualBonusDetail
            {
                AnnualBonusId = bonus.Id,
                EmployeeId = emp.Id,
                WorkedMonths = workedMonths,
                AverageSalary = averageSalary,
                ProportionalAmount = Math.Round(proportionalAmount, 2),
                Deductions = 0,
                NetAmount = Math.Round(proportionalAmount, 2),
                CreatedAt = DateTime.UtcNow
            };

            _context.AnnualBonusDetails.Add(detail);
            totalAmount += detail.NetAmount;
        }

        bonus.TotalAmount = totalAmount;
        bonus.TotalEmployees = employees.Count;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId,
            "CALCULATE",
            "AGUINALDO",
            "AnnualBonus",
            bonus.Id,
            description: $"Aguinaldo {dto.Year} calculado: {employees.Count} empleados, Total: {totalAmount:C}"
        );

        return (await GetByIdAsync(bonus.Id))!;
    }

    public async Task<AnnualBonusDto> ApproveAsync(int id, int userId, string? notes = null)
    {
        var bonus = await _context.AnnualBonuses.FindAsync(id)
            ?? throw new ArgumentException("Aguinaldo no encontrado");

        if (bonus.Status != AnnualBonusStatus.Calculado)
            throw new InvalidOperationException("Solo se puede aprobar un aguinaldo calculado");

        bonus.Status = AnnualBonusStatus.Aprobado;
        bonus.ApprovedById = userId;
        bonus.ApprovedAt = DateTime.UtcNow;
        bonus.Notes = notes ?? bonus.Notes;
        bonus.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId,
            "APPROVE",
            "AGUINALDO",
            "AnnualBonus",
            id,
            description: $"Aguinaldo {bonus.Year} aprobado"
        );

        return (await GetByIdAsync(id))!;
    }

    public async Task<AnnualBonusDto> RecalculateAsync(int id, int userId)
    {
        var bonus = await _context.AnnualBonuses
            .Include(ab => ab.Details)
            .FirstOrDefaultAsync(ab => ab.Id == id)
            ?? throw new ArgumentException("Aguinaldo no encontrado");

        if (bonus.Status == AnnualBonusStatus.Pagado)
            throw new InvalidOperationException("No se puede recalcular un aguinaldo pagado");

        _context.AnnualBonusDetails.RemoveRange(bonus.Details);

        var activeStatus = await _context.EmployeeStatuses
            .FirstAsync(es => es.Name == "Activo");

        var employees = await _context.Employees
            .Include(e => e.Position)
            .Where(e => e.EmployeeStatusId == activeStatus.Id && e.HireDate.Year <= bonus.Year)
            .ToListAsync();

        decimal totalAmount = 0;

        foreach (var emp in employees)
        {
            int startMonth = emp.HireDate.Year == bonus.Year ? emp.HireDate.Month : 1;
            int workedMonths = 12 - startMonth + 1;

            decimal averageSalary = emp.BaseSalary;
            decimal proportionalAmount = Math.Round((averageSalary / 12) * workedMonths, 2);

            var detail = new AnnualBonusDetail
            {
                AnnualBonusId = bonus.Id,
                EmployeeId = emp.Id,
                WorkedMonths = workedMonths,
                AverageSalary = averageSalary,
                ProportionalAmount = proportionalAmount,
                Deductions = 0,
                NetAmount = proportionalAmount,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnnualBonusDetails.Add(detail);
            totalAmount += proportionalAmount;
        }

        bonus.TotalAmount = totalAmount;
        bonus.TotalEmployees = employees.Count;
        bonus.Status = AnnualBonusStatus.Calculado;
        bonus.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId,
            "RECALCULATE",
            "AGUINALDO",
            "AnnualBonus",
            id,
            description: $"Aguinaldo {bonus.Year} recalculado"
        );

        return (await GetByIdAsync(id))!;
    }

    private static AnnualBonusDto MapToDto(AnnualBonus ab, bool includeDetails)
    {
        var dto = new AnnualBonusDto
        {
            Id = ab.Id,
            Year = ab.Year,
            PeriodStartDate = ab.PeriodStartDate,
            PeriodEndDate = ab.PeriodEndDate,
            Status = ab.Status.ToString(),
            TotalAmount = ab.TotalAmount,
            TotalEmployees = ab.TotalEmployees,
            CalculatedByName = ab.CalculatedBy?.Username ?? string.Empty,
            CalculatedAt = ab.CalculatedAt,
            ApprovedByName = ab.ApprovedBy?.Username,
            ApprovedAt = ab.ApprovedAt,
            Notes = ab.Notes,
            CreatedAt = ab.CreatedAt
        };

        if (includeDetails)
        {
            dto.Details = ab.Details.Select(d => new AnnualBonusDetailDto
            {
                Id = d.Id,
                EmployeeId = d.EmployeeId,
                EmployeeName = d.Employee?.FullName ?? string.Empty,
                PositionName = d.Employee?.Position?.Name,
                WorkedMonths = d.WorkedMonths,
                AverageSalary = d.AverageSalary,
                ProportionalAmount = d.ProportionalAmount,
                Deductions = d.Deductions,
                NetAmount = d.NetAmount,
                Notes = d.Notes
            }).ToList();
        }

        return dto;
    }
}