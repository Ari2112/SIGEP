using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.PerformanceEvaluation;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepInfrastructure.Persistence;
using System.Linq;

namespace SigepInfrastructure.Services;

public class PerformanceEvaluationService : IPerformanceEvaluationService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public PerformanceEvaluationService(ApplicationDbContext context, IAuditService auditService, INotificationService notificationService)
    {
        _context = context;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<PerformanceEvaluationDto>> GetAllAsync(EvaluationFilterDto? filter = null)
    {
        var query = _context.PerformanceEvaluations
            .Include(pe => pe.Employee).ThenInclude(e => e!.Position)
            .Include(pe => pe.Evaluator)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.EmployeeId.HasValue)
                query = query.Where(pe => pe.EmployeeId == filter.EmployeeId.Value);
            if (filter.Year.HasValue)
                query = query.Where(pe => pe.EvaluationDate.Year == filter.Year.Value);
            if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<EvaluationStatus>(filter.Status, out var status))
                query = query.Where(pe => pe.Status == status);
        }

        var evals = await query.OrderByDescending(pe => pe.EvaluationDate).ToListAsync();
        return evals.Select(MapToDto);
    }

    public async Task<IEnumerable<PerformanceEvaluationDto>> GetByEmployeeAsync(int employeeId)
    {
        var evals = await _context.PerformanceEvaluations
            .Include(pe => pe.Employee).ThenInclude(e => e!.Position)
            .Include(pe => pe.Evaluator)
            .Where(pe => pe.EmployeeId == employeeId)
            .OrderByDescending(pe => pe.EvaluationDate)
            .ToListAsync();

        return evals.Select(MapToDto);
    }

    public async Task<PerformanceEvaluationDto?> GetByIdAsync(int id)
    {
        var eval = await _context.PerformanceEvaluations
            .Include(pe => pe.Employee).ThenInclude(e => e!.Position)
            .Include(pe => pe.Evaluator)
            .FirstOrDefaultAsync(pe => pe.Id == id);

        return eval == null ? null : MapToDto(eval);
    }

    public async Task<PerformanceEvaluationDto> CreateAsync(CreateEvaluationDto dto, int evaluatorUserId)
    {
        ValidateCriteria(dto.ScorePunctuality, dto.ScoreObedience, dto.ScoreQuality,
                         dto.ScoreResponsibility, dto.ScoreTeamwork, dto.ScoreCustomerService);

        var score = AverageScore(dto.ScorePunctuality, dto.ScoreObedience, dto.ScoreQuality,
                                 dto.ScoreResponsibility, dto.ScoreTeamwork, dto.ScoreCustomerService);

        var employee = await _context.Employees.FindAsync(dto.EmployeeId)
            ?? throw new ArgumentException("Empleado no encontrado");

        var eval = new PerformanceEvaluation
        {
            EmployeeId = dto.EmployeeId,
            EvaluatorId = evaluatorUserId,
            EvaluationDate = dto.EvaluationDate,
            PeriodStartDate = dto.PeriodStartDate,
            PeriodEndDate = dto.PeriodEndDate,
            Score = score,
            ScorePunctuality = dto.ScorePunctuality,
            ScoreObedience = dto.ScoreObedience,
            ScoreQuality = dto.ScoreQuality,
            ScoreResponsibility = dto.ScoreResponsibility,
            ScoreTeamwork = dto.ScoreTeamwork,
            ScoreCustomerService = dto.ScoreCustomerService,
            Comments = dto.Comments,
            Strengths = dto.Strengths,
            AreasToImprove = dto.AreasToImprove,
            Goals = dto.Goals,
            Status = EvaluationStatus.Completada,
            CreatedAt = DateTime.UtcNow
        };

        _context.PerformanceEvaluations.Add(eval);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(evaluatorUserId, "CREATE", "EVALUACION", "PerformanceEvaluation", eval.Id,
            description: $"Evaluación creada para {employee.FullName}: puntuación {score}/10");

        // Notificar al empleado
        var empUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == dto.EmployeeId);
        if (empUser != null)
        {
            await _notificationService.CreateNotificationAsync(
                empUser.Id,
                "Nueva evaluación de desempeño",
                $"Se ha registrado una evaluación de desempeño con puntuación {score}/10. Por favor revise y confirme.",
                "INFO", "EVALUACION", "PerformanceEvaluation", eval.Id);
        }

        return (await GetByIdAsync(eval.Id))!;
    }

    /// <summary>Valida que cada criterio esté entre 1 y 10.</summary>
    private static void ValidateCriteria(params int[] scores)
    {
        if (scores.Any(s => s < 1 || s > 10))
            throw new ArgumentException("Cada criterio debe tener una nota entre 1 y 10");
    }

    /// <summary>Promedio redondeado de los criterios (nota general).</summary>
    private static int AverageScore(params int[] scores) =>
        (int)Math.Round(scores.Average(), MidpointRounding.AwayFromZero);

    public async Task<PerformanceEvaluationDto> UpdateAsync(int id, UpdateEvaluationDto dto, int evaluatorUserId)
    {
        var eval = await _context.PerformanceEvaluations.FindAsync(id)
            ?? throw new ArgumentException("Evaluación no encontrada");

        if (eval.Status == EvaluationStatus.RevisadaPorEmpleado)
            throw new InvalidOperationException("No se puede modificar una evaluación ya revisada por el empleado");

        ValidateCriteria(dto.ScorePunctuality, dto.ScoreObedience, dto.ScoreQuality,
                         dto.ScoreResponsibility, dto.ScoreTeamwork, dto.ScoreCustomerService);

        eval.ScorePunctuality = dto.ScorePunctuality;
        eval.ScoreObedience = dto.ScoreObedience;
        eval.ScoreQuality = dto.ScoreQuality;
        eval.ScoreResponsibility = dto.ScoreResponsibility;
        eval.ScoreTeamwork = dto.ScoreTeamwork;
        eval.ScoreCustomerService = dto.ScoreCustomerService;
        eval.Score = AverageScore(dto.ScorePunctuality, dto.ScoreObedience, dto.ScoreQuality,
                                  dto.ScoreResponsibility, dto.ScoreTeamwork, dto.ScoreCustomerService);
        eval.Comments = dto.Comments;
        eval.Strengths = dto.Strengths;
        eval.AreasToImprove = dto.AreasToImprove;
        eval.Goals = dto.Goals;
        eval.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync(evaluatorUserId, "UPDATE", "EVALUACION", "PerformanceEvaluation", id,
            description: $"Evaluación actualizada: puntuación {eval.Score}/10");

        return (await GetByIdAsync(id))!;
    }

    public async Task<PerformanceEvaluationDto> CompleteAsync(int id, int evaluatorUserId)
    {
        var eval = await _context.PerformanceEvaluations.FindAsync(id)
            ?? throw new ArgumentException("Evaluación no encontrada");

        eval.Status = EvaluationStatus.Completada;
        eval.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<PerformanceEvaluationDto> AcknowledgeAsync(int id, int employeeUserId)
    {
        var eval = await _context.PerformanceEvaluations.FindAsync(id)
            ?? throw new ArgumentException("Evaluación no encontrada");

        var user = await _context.Users.FindAsync(employeeUserId);
        if (user?.EmployeeId != eval.EmployeeId)
            throw new UnauthorizedAccessException("No puede confirmar una evaluación de otro empleado");

        eval.Status = EvaluationStatus.RevisadaPorEmpleado;
        eval.EmployeeAcknowledged = true;
        eval.AcknowledgedAt = DateTime.UtcNow;
        eval.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    private static string GetScoreLabel(int score) => score switch
    {
        >= 9 => "Excelente",
        >= 7 => "Bueno",
        >= 5 => "Aceptable",
        _ => "Necesita mejorar"
    };

    private static PerformanceEvaluationDto MapToDto(PerformanceEvaluation pe) => new()
    {
        Id = pe.Id,
        EmployeeId = pe.EmployeeId,
        EmployeeName = pe.Employee?.FullName ?? string.Empty,
        PositionName = pe.Employee?.Position?.Name,
        EvaluatorId = pe.EvaluatorId,
        EvaluatorName = pe.Evaluator?.Username ?? string.Empty,
        EvaluationDate = pe.EvaluationDate,
        PeriodStartDate = pe.PeriodStartDate,
        PeriodEndDate = pe.PeriodEndDate,
        Score = pe.Score,
        ScoreLabel = GetScoreLabel(pe.Score),
        ScorePunctuality = pe.ScorePunctuality,
        ScoreObedience = pe.ScoreObedience,
        ScoreQuality = pe.ScoreQuality,
        ScoreResponsibility = pe.ScoreResponsibility,
        ScoreTeamwork = pe.ScoreTeamwork,
        ScoreCustomerService = pe.ScoreCustomerService,
        Comments = pe.Comments,
        Strengths = pe.Strengths,
        AreasToImprove = pe.AreasToImprove,
        Goals = pe.Goals,
        Status = pe.Status.ToString(),
        EmployeeAcknowledged = pe.EmployeeAcknowledged,
        AcknowledgedAt = pe.AcknowledgedAt,
        CreatedAt = pe.CreatedAt
    };
}