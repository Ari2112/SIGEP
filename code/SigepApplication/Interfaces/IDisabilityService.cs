using SigepApplication.DTOs.Disability;

namespace SigepApplication.Interfaces;

public interface IDisabilityService
{
    Task<IEnumerable<DisabilityRequestDto>> GetAllAsync(DisabilityFilterDto? filter = null);
    Task<IEnumerable<DisabilityRequestDto>> GetByEmployeeAsync(int employeeId);
    Task<DisabilityRequestDto?> GetByIdAsync(int id);
    Task<DisabilityRequestDto> CreateAsync(int employeeId, CreateDisabilityDto dto);
    Task<DisabilityRequestDto> ReviewAsync(int id, int reviewerUserId, bool approve, string? comments = null);
    Task<IEnumerable<DisabilityTypeDto>> GetTypesAsync();
}
