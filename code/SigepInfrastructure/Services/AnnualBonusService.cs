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

        // Período legal del aguinaldo según Art. 229 Código de Trabajo CR:
        // Del 1 de diciembre del año anterior al 30 de noviembre del año en curso
        var periodStart = new DateTime(dto.Year - 1, 12, 1);
        var periodEnd   = new DateTime(dto.Year, 11, 30);

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
            var (workedMonths, proportionalAmount) = CalculateProportionalBonus(emp, periodStart, periodEnd);

            decimal averageSalary = emp.BaseSalary;

            var detail = new AnnualBonusDetail
            {
                AnnualBonusId      = bonus.Id,
                EmployeeId         = emp.Id,
                WorkedMonths       = workedMonths,
                AverageSalary      = averageSalary,
                ProportionalAmount = proportionalAmount,
                Deductions         = 0,
                NetAmount          = proportionalAmount,
                CreatedAt          = DateTime.UtcNow
            };

            _context.AnnualBonusDetails.Add(detail);
            totalAmount += detail.NetAmount;
        }

        bonus.TotalAmount     = totalAmount;
        bonus.TotalEmployees  = employees.Count;

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

        bonus.Status        = AnnualBonusStatus.Aprobado;
        bonus.ApprovedById  = userId;
        bonus.ApprovedAt    = DateTime.UtcNow;
        bonus.Notes         = notes ?? bonus.Notes;
        bonus.UpdatedAt     = DateTime.UtcNow;

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

        // Período legal: 1 dic año anterior → 30 nov año en curso
        var periodStart = new DateTime(bonus.Year - 1, 12, 1);
        var periodEnd   = new DateTime(bonus.Year, 11, 30);

        decimal totalAmount = 0;

        foreach (var emp in employees)
        {
            var (workedMonths, proportionalAmount) = CalculateProportionalBonus(emp, periodStart, periodEnd);

            decimal averageSalary = emp.BaseSalary;

            var detail = new AnnualBonusDetail
            {
                AnnualBonusId      = bonus.Id,
                EmployeeId         = emp.Id,
                WorkedMonths       = workedMonths,
                AverageSalary      = averageSalary,
                ProportionalAmount = proportionalAmount,
                Deductions         = 0,
                NetAmount          = proportionalAmount,
                CreatedAt          = DateTime.UtcNow
            };

            _context.AnnualBonusDetails.Add(detail);
            totalAmount += proportionalAmount;
        }

        bonus.TotalAmount    = totalAmount;
        bonus.TotalEmployees = employees.Count;
        bonus.Status         = AnnualBonusStatus.Calculado;
        bonus.UpdatedAt      = DateTime.UtcNow;

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

    // === Aguinaldo proporcional - Ley No. 2412 ===
    //
    // La ley dice: aguinaldo = (suma de los salarios devengados entre el 1 de diciembre
    // del año anterior y el 30 de noviembre del año en curso) ÷ 12.
    //
    // Con un salario mensual fijo eso equivale a:  salario × meses trabajados ÷ 12.
    // Por eso, quien trabajó el período completo recibe EXACTAMENTE un salario mensual,
    // y quien trabajó 6 meses recibe medio salario.
    //
    // Se cuentan meses completos desde el ingreso (o desde el 1 de diciembre, lo que sea
    // más reciente) y los días sueltos que sobran se cuentan como fracción de mes (/30).
    private static (int WorkedMonths, decimal ProportionalAmount) CalculateProportionalBonus(
        Employee emp, DateTime periodStart, DateTime periodEnd)
    {
        // El devengo arranca en el ingreso o el 1 de diciembre, lo que sea más reciente.
        DateTime accrualStart = emp.HireDate > periodStart ? emp.HireDate : periodStart;

        // Si entró después de que cerró el período, no le corresponde nada de este año.
        if (accrualStart > periodEnd)
            return (0, 0m);

        // Meses calendario completos entre el inicio del devengo y el fin del período.
        int fullMonths = ((periodEnd.Year - accrualStart.Year) * 12) + periodEnd.Month - accrualStart.Month;
        if (accrualStart.AddMonths(fullMonths) > periodEnd)
            fullMonths--;
        if (fullMonths < 0) fullMonths = 0;

        // Días que sobran después del último mes completo, como fracción de mes.
        DateTime lastFullMonth = accrualStart.AddMonths(fullMonths);
        int leftoverDays = (periodEnd - lastFullMonth).Days + 1;
        if (leftoverDays < 0) leftoverDays = 0;
        if (leftoverDays > 30) leftoverDays = 30;

        decimal monthsAccrued = fullMonths + (leftoverDays / 30m);
        if (monthsAccrued > 12m) monthsAccrued = 12m;
        if (monthsAccrued < 0m) monthsAccrued = 0m;

        decimal salary = emp.BaseSalary;
        decimal proportionalAmount = Math.Round(salary * monthsAccrued / 12m, 2);

        // Solo para mostrar en pantalla/reportes; el monto NO depende de este valor.
        int workedMonths = (int)Math.Floor(monthsAccrued);
        if (workedMonths > 12) workedMonths = 12;

        return (workedMonths, proportionalAmount);
    }

    private static AnnualBonusDto MapToDto(AnnualBonus ab, bool includeDetails)
    {
        var dto = new AnnualBonusDto
        {
            Id                = ab.Id,
            Year              = ab.Year,
            PeriodStartDate   = ab.PeriodStartDate,
            PeriodEndDate     = ab.PeriodEndDate,
            Status            = ab.Status.ToString(),
            TotalAmount       = ab.TotalAmount,
            TotalEmployees    = ab.TotalEmployees,
            CalculatedByName  = ab.CalculatedBy?.Username ?? string.Empty,
            CalculatedAt      = ab.CalculatedAt,
            ApprovedByName    = ab.ApprovedBy?.Username,
            ApprovedAt        = ab.ApprovedAt,
            Notes             = ab.Notes,
            CreatedAt         = ab.CreatedAt
        };

        if (includeDetails)
        {
            dto.Details = ab.Details.Select(d => new AnnualBonusDetailDto
            {
                Id                 = d.Id,
                EmployeeId         = d.EmployeeId,
                EmployeeName       = d.Employee?.FullName ?? string.Empty,
                PositionName       = d.Employee?.Position?.Name,
                WorkedMonths       = d.WorkedMonths,
                AverageSalary      = d.AverageSalary,
                ProportionalAmount = d.ProportionalAmount,
                Deductions         = d.Deductions,
                NetAmount          = d.NetAmount,
                Notes              = d.Notes
            }).ToList();
        }

        return dto;
    }
}