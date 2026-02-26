using SigepApplication.DTOs.Employees;

namespace SigepApplication.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync();
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, int createdByUserId);
    Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto, int updatedByUserId);
    Task DeactivateAsync(int id, int updatedByUserId);

    // Puestos
    Task<IEnumerable<PositionDto>> GetAllPositionsAsync();
    Task<PositionDto> CreatePositionAsync(CreatePositionDto dto);

    // Horarios
    Task<IEnumerable<ScheduleDto>> GetAllSchedulesAsync();
    Task<ScheduleDto> CreateScheduleAsync(CreateScheduleDto dto);
}
