using SigepApplication.DTOs.Settlement;

namespace SigepApplication.Interfaces;

public interface ISettlementService
{
    Task<IEnumerable<SettlementDto>> GetAllAsync();
    Task<SettlementDto?> GetByIdAsync(int id);
    Task<SettlementDto?> GetByEmployeeAsync(int employeeId);
    Task<SettlementDto> CalculateAsync(CalculateSettlementDto dto, int userId);
    Task<SettlementDto> ApproveAsync(int id, int userId, string? notes = null);
    Task<SettlementDto> MarkAsPaidAsync(int id, int userId);
}
