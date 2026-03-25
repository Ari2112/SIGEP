using SigepApplication.DTOs.PerformanceEvaluation;

namespace SigepApplication.Interfaces;

public interface IPerformanceEvaluationService
{
    Task<IEnumerable<PerformanceEvaluationDto>> GetAllAsync(EvaluationFilterDto? filter = null);
    Task<IEnumerable<PerformanceEvaluationDto>> GetByEmployeeAsync(int employeeId);
    Task<PerformanceEvaluationDto?> GetByIdAsync(int id);
    Task<PerformanceEvaluationDto> CreateAsync(CreateEvaluationDto dto, int evaluatorUserId);
    Task<PerformanceEvaluationDto> UpdateAsync(int id, UpdateEvaluationDto dto, int evaluatorUserId);
    Task<PerformanceEvaluationDto> CompleteAsync(int id, int evaluatorUserId);
    Task<PerformanceEvaluationDto> AcknowledgeAsync(int id, int employeeUserId);
}
