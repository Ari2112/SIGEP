using SigepApplication.DTOs.AnnualBonus;

namespace SigepApplication.Interfaces;

public interface IAnnualBonusService
{
    Task<IEnumerable<AnnualBonusDto>> GetAllAsync();
    Task<AnnualBonusDto?> GetByIdAsync(int id);
    Task<AnnualBonusDto?> GetByYearAsync(int year);
    Task<AnnualBonusDto> CalculateAsync(CalculateAnnualBonusDto dto, int userId);
    Task<AnnualBonusDto> ApproveAsync(int id, int userId, string? notes = null);
    Task<AnnualBonusDto> RecalculateAsync(int id, int userId);
}
