using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Employees;
using SigepApplication.Interfaces;
using SigepInfrastructure.Persistence;

namespace SigepApplication.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        return await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.Schedule)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                IdentificationNumber = e.IdentificationNumber,
                Email = e.Email,
                Phone = e.Phone,
                HireDate = e.HireDate,
                BaseSalary = e.BaseSalary,
                Status = e.Status.ToString(),
                PositionName = e.Position != null ? e.Position.Name : null,
                ScheduleName = e.Schedule != null ? e.Schedule.Name : null
            })
            .ToListAsync();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.Schedule)
            .Where(e => e.Id == id)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                IdentificationNumber = e.IdentificationNumber,
                Email = e.Email,
                Phone = e.Phone,
                HireDate = e.HireDate,
                BaseSalary = e.BaseSalary,
                Status = e.Status.ToString(),
                PositionName = e.Position != null ? e.Position.Name : null,
                ScheduleName = e.Schedule != null ? e.Schedule.Name : null
            })
            .FirstOrDefaultAsync();
    }
}
