using SigepApplication.DTOs.Payroll;

namespace SigepApplication.Interfaces;

public interface IPayrollService
{
    Task<IEnumerable<PayrollDto>> GetAllAsync();
    Task<PayrollDto?> GetByIdAsync(int id);
    Task<PayrollDto> GenerateAsync(CreatePayrollDto dto, int userId);
    Task<PayrollDto> ApproveAsync(int id, int userId, string? notes = null);
    Task<PayrollDto> AnnulAsync(int id, int userId, string? notes = null);
    Task<IEnumerable<DeductionTypeDto>> GetDeductionTypesAsync();
    Task<IEnumerable<BenefitTypeDto>> GetBenefitTypesAsync();
}
