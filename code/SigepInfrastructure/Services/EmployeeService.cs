using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Employees;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepDomain.Enums;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public EmployeeService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        return await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.Schedule)
            .OrderBy(e => e.LastName)
            .Select(e => MapToDto(e))
            .ToListAsync();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var e = await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.Schedule)
            .FirstOrDefaultAsync(e => e.Id == id);

        return e == null ? null : MapToDto(e);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, int createdByUserId)
    {
        // Validar cédula única
        if (await _context.Employees.AnyAsync(e => e.IdentificationNumber == dto.IdentificationNumber))
            throw new InvalidOperationException("Ya existe un empleado con ese número de identificación");

        if (await _context.Employees.AnyAsync(e => e.Email == dto.Email))
            throw new InvalidOperationException("Ya existe un empleado con ese correo electrónico");

        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IdentificationNumber = dto.IdentificationNumber,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            BirthDate = dto.BirthDate,
            HireDate = dto.HireDate,
            BaseSalary = dto.BaseSalary,
            PositionId = dto.PositionId,
            ScheduleId = dto.ScheduleId,
            SupervisorId = dto.SupervisorId,
            VacationDaysPerYear = dto.VacationDaysPerYear,
            Status = EmployeeStatus.Activo,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Crear usuario si se proveen credenciales
        if (!string.IsNullOrWhiteSpace(dto.Username) && !string.IsNullOrWhiteSpace(dto.Password))
        {
            var role = dto.UserRole?.ToLower() switch
            {
                "admin" => UserRole.Admin,
                "rrhh" => UserRole.RRHH,
                "jefatura" => UserRole.Jefatura,
                _ => UserRole.Empleado
            };

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = role,
                EmployeeId = employee.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        await _auditService.LogAsync(createdByUserId, "CREATE", "EMPLEADOS", "Employee", employee.Id,
            description: $"Nuevo empleado creado: {employee.FirstName} {employee.LastName}");

        return await GetByIdAsync(employee.Id) ?? throw new Exception("Error al crear empleado");
    }

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto, int updatedByUserId)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            throw new ArgumentException("Empleado no encontrado");

        if (await _context.Employees.AnyAsync(e => e.Email == dto.Email && e.Id != id))
            throw new InvalidOperationException("Ya existe otro empleado con ese correo electrónico");

        var oldValues = new { employee.FirstName, employee.LastName, employee.BaseSalary, employee.Status };

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Phone = dto.Phone;
        employee.Address = dto.Address;
        employee.BirthDate = dto.BirthDate;
        employee.BaseSalary = dto.BaseSalary;
        employee.PositionId = dto.PositionId;
        employee.ScheduleId = dto.ScheduleId;
        employee.SupervisorId = dto.SupervisorId;
        employee.VacationDaysPerYear = dto.VacationDaysPerYear;
        employee.Status = Enum.Parse<EmployeeStatus>(dto.Status);
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(updatedByUserId, "UPDATE", "EMPLEADOS", "Employee", id,
            oldValues: oldValues,
            newValues: new { employee.FirstName, employee.LastName, employee.BaseSalary, employee.Status },
            description: $"Empleado actualizado: {employee.FirstName} {employee.LastName}");

        return await GetByIdAsync(id) ?? throw new Exception("Error al actualizar empleado");
    }

    public async Task DeactivateAsync(int id, int updatedByUserId)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            throw new ArgumentException("Empleado no encontrado");

        employee.Status = EmployeeStatus.Inactivo;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(updatedByUserId, "DEACTIVATE", "EMPLEADOS", "Employee", id,
            description: $"Empleado desactivado: {employee.FirstName} {employee.LastName}");
    }

    // === PUESTOS ===

    public async Task<IEnumerable<PositionDto>> GetAllPositionsAsync()
    {
        return await _context.Positions
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new PositionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                BaseSalary = p.BaseSalary,
                IsActive = p.IsActive
            })
            .ToListAsync();
    }

    public async Task<PositionDto> CreatePositionAsync(CreatePositionDto dto)
    {
        var position = new Position
        {
            Name = dto.Name,
            Description = dto.Description,
            BaseSalary = dto.BaseSalary,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Positions.Add(position);
        await _context.SaveChangesAsync();

        return new PositionDto
        {
            Id = position.Id,
            Name = position.Name,
            Description = position.Description,
            BaseSalary = position.BaseSalary,
            IsActive = position.IsActive
        };
    }

    // === HORARIOS ===

    public async Task<IEnumerable<ScheduleDto>> GetAllSchedulesAsync()
    {
        return await _context.Schedules
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new ScheduleDto
            {
                Id = s.Id,
                Name = s.Name,
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                EndTime = s.EndTime.ToString(@"hh\:mm"),
                WorkHoursPerDay = s.WorkHoursPerDay,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<ScheduleDto> CreateScheduleAsync(CreateScheduleDto dto)
    {
        var schedule = new Schedule
        {
            Name = dto.Name,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            WorkHoursPerDay = dto.WorkHoursPerDay,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        return new ScheduleDto
        {
            Id = schedule.Id,
            Name = schedule.Name,
            StartTime = schedule.StartTime.ToString(@"hh\:mm"),
            EndTime = schedule.EndTime.ToString(@"hh\:mm"),
            WorkHoursPerDay = schedule.WorkHoursPerDay,
            IsActive = schedule.IsActive
        };
    }

    private static EmployeeDto MapToDto(Employee e)
    {
        return new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            IdentificationNumber = e.IdentificationNumber,
            Email = e.Email,
            Phone = e.Phone,
            Address = e.Address,
            BirthDate = e.BirthDate,
            HireDate = e.HireDate,
            BaseSalary = e.BaseSalary,
            Status = e.Status.ToString(),
            PositionId = e.PositionId,
            PositionName = e.Position?.Name,
            ScheduleId = e.ScheduleId,
            ScheduleName = e.Schedule?.Name,
            VacationDaysPerYear = e.VacationDaysPerYear,
            CreatedAt = e.CreatedAt
        };
    }
}
