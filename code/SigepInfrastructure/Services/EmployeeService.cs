using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Employees;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
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
        var employees = await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.Schedule)
            .Include(e => e.EmployeeStatus)
            .Include(e => e.EmployeePhones)
            .Include(e => e.EmployeeAddresses)
            .OrderBy(e => e.LastName)
            .ToListAsync();
 
        return employees.Select(MapToDto);
    }
 
    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.Schedule)
            .Include(e => e.EmployeeStatus)
            .Include(e => e.EmployeePhones)
            .Include(e => e.EmployeeAddresses)
            .FirstOrDefaultAsync(e => e.Id == id);
 
        return employee == null ? null : MapToDto(employee);
    }
 
    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, int createdByUserId)
    {
        if (await _context.Employees.AnyAsync(e => e.IdentificationNumber == dto.IdentificationNumber))
            throw new InvalidOperationException("Ya existe un empleado con ese número de identificación");
 
        if (await _context.Employees.AnyAsync(e => e.Email == dto.Email))
            throw new InvalidOperationException("Ya existe un empleado con ese correo electrónico");
 
        var activeStatus = await GetEmployeeStatusAsync("Activo");
 
        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IdentificationNumber = dto.IdentificationNumber,
            Email = dto.Email,
            BirthDate = dto.BirthDate,
            HireDate = dto.HireDate,
            BaseSalary = dto.BaseSalary,
            PositionId = dto.PositionId,
            ScheduleId = dto.ScheduleId,
            SupervisorId = dto.SupervisorId,
            VacationDaysPerYear = dto.VacationDaysPerYear,
            EmployeeStatusId = activeStatus.Id,
            CreatedAt = DateTime.UtcNow
        };
 
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
 
        await SavePrimaryPhoneAsync(employee.Id, dto.Phone);
        await SavePrimaryAddressAsync(employee.Id, dto.Address);
        await _context.SaveChangesAsync();
 
        if (!string.IsNullOrWhiteSpace(dto.Username) && !string.IsNullOrWhiteSpace(dto.Password))
        {
            var roleName = NormalizeRoleName(dto.UserRole);
            var role = await GetUserRoleAsync(roleName);
 
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = role.Id,
                EmployeeId = employee.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
 
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
 
        await _auditService.LogAsync(
            createdByUserId,
            "CREATE",
            "EMPLEADOS",
            "Employee",
            employee.Id,
            description: $"Nuevo empleado creado: {employee.FirstName} {employee.LastName}"
        );
 
        return await GetByIdAsync(employee.Id) ?? throw new Exception("Error al crear empleado");
    }
 
    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto, int updatedByUserId)
    {
        var employee = await _context.Employees
            .Include(e => e.EmployeePhones)
            .Include(e => e.EmployeeAddresses)
            .FirstOrDefaultAsync(e => e.Id == id);
 
        if (employee == null)
            throw new ArgumentException("Empleado no encontrado");
 
        if (await _context.Employees.AnyAsync(e => e.Email == dto.Email && e.Id != id))
            throw new InvalidOperationException("Ya existe otro empleado con ese correo electrónico");
 
        var status = await GetEmployeeStatusAsync(dto.Status);
 
        var oldValues = new
        {
            employee.FirstName,
            employee.LastName,
            employee.BaseSalary,
            employee.EmployeeStatusId
        };
 
        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.BirthDate = dto.BirthDate;
        employee.BaseSalary = dto.BaseSalary;
        employee.PositionId = dto.PositionId;
        employee.ScheduleId = dto.ScheduleId;
        employee.SupervisorId = dto.SupervisorId;
        employee.VacationDaysPerYear = dto.VacationDaysPerYear;
        employee.EmployeeStatusId = status.Id;
        employee.UpdatedAt = DateTime.UtcNow;
 
        await SavePrimaryPhoneAsync(employee.Id, dto.Phone);
        await SavePrimaryAddressAsync(employee.Id, dto.Address);
 
        await _context.SaveChangesAsync();
 
        var newValues = new
        {
            employee.FirstName,
            employee.LastName,
            employee.BaseSalary,
            employee.EmployeeStatusId
        };
 
        await _auditService.LogAsync(
            updatedByUserId,
            "UPDATE",
            "EMPLEADOS",
            "Employee",
            id,
            oldValues: oldValues,
            newValues: newValues,
            description: $"Empleado actualizado: {employee.FirstName} {employee.LastName}"
        );
 
        return await GetByIdAsync(id) ?? throw new Exception("Error al actualizar empleado");
    }
 
    public async Task DeactivateAsync(int id, int updatedByUserId)
    {
        var employee = await _context.Employees.FindAsync(id);
 
        if (employee == null)
            throw new ArgumentException("Empleado no encontrado");
 
        var inactiveStatus = await GetEmployeeStatusAsync("Inactivo");
 
        employee.EmployeeStatusId = inactiveStatus.Id;
        employee.UpdatedAt = DateTime.UtcNow;
 
        await _context.SaveChangesAsync();
 
        await _auditService.LogAsync(
            updatedByUserId,
            "DEACTIVATE",
            "EMPLEADOS",
            "Employee",
            id,
            description: $"Empleado desactivado: {employee.FirstName} {employee.LastName}"
        );
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
 
    // === GEOGRAFÍA COSTA RICA ===
 
    public async Task<IEnumerable<GeoItemDto>> GetProvincesAsync()
    {
        return await _context.Provinces
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new GeoItemDto { Id = p.Id, Name = p.Name })
            .ToListAsync();
    }
 
    public async Task<IEnumerable<GeoItemDto>> GetCantonsByProvinceAsync(int provinceId)
    {
        return await _context.Cantons
            .Where(c => c.ProvinceId == provinceId && c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new GeoItemDto { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }
 
    public async Task<IEnumerable<GeoItemDto>> GetDistrictsByCantonAsync(int cantonId)
    {
        return await _context.Districts
            .Where(d => d.CantonId == cantonId && d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new GeoItemDto { Id = d.Id, Name = d.Name })
            .ToListAsync();
    }
 
    // === MÉTODOS AUXILIARES ===
 
    private static EmployeeDto MapToDto(Employee e)
    {
        var primaryPhone = e.EmployeePhones?
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.IsPrimary)
            .FirstOrDefault();
 
        var primaryAddress = e.EmployeeAddresses?
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.IsPrimary)
            .FirstOrDefault();
 
        return new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            IdentificationNumber = e.IdentificationNumber,
            Email = e.Email,
            Phone = primaryPhone?.PhoneNumber,
            Address = primaryAddress?.ExactAddress,
            BirthDate = e.BirthDate,
            HireDate = e.HireDate,
            BaseSalary = e.BaseSalary,
            Status = e.EmployeeStatus?.Name ?? string.Empty,
            PositionId = e.PositionId,
            PositionName = e.Position?.Name,
            ScheduleId = e.ScheduleId,
            ScheduleName = e.Schedule?.Name,
            VacationDaysPerYear = e.VacationDaysPerYear,
            CreatedAt = e.CreatedAt
        };
    }
 
    private async Task<EmployeeStatus> GetEmployeeStatusAsync(string? statusName)
    {
        var name = string.IsNullOrWhiteSpace(statusName) ? "Activo" : statusName.Trim();
 
        var status = await _context.EmployeeStatuses
            .FirstOrDefaultAsync(s => s.Name == name);
 
        if (status != null)
            return status;
 
        var activeStatus = await _context.EmployeeStatuses
            .FirstOrDefaultAsync(s => s.Name == "Activo");
 
        if (activeStatus != null)
            return activeStatus;
 
        throw new InvalidOperationException("No existe el catálogo de estados de empleado");
    }
 
    private async Task<UserRole> GetUserRoleAsync(string roleName)
    {
        var role = await _context.UserRoles
            .FirstOrDefaultAsync(r => r.Name == roleName);
 
        if (role != null)
            return role;
 
        var empleadoRole = await _context.UserRoles
            .FirstOrDefaultAsync(r => r.Name == "Empleado");
 
        if (empleadoRole != null)
            return empleadoRole;
 
        throw new InvalidOperationException("No existe el catálogo de roles de usuario");
    }
 
    private static string NormalizeRoleName(string? userRole)
    {
        var value = userRole?.Trim().ToLower();
 
        return value switch
        {
            "admin" => "Admin",
            "administrador" => "Admin",
            "rrhh" => "RRHH",
            "recursos humanos" => "RRHH",
            "jefatura" => "Jefatura",
            "jefe" => "Jefatura",
            _ => "Empleado"
        };
    }
 
    private async Task SavePrimaryPhoneAsync(int employeeId, string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return;
 
        var phoneType = await _context.PhoneTypes
            .FirstOrDefaultAsync(pt => pt.Name == "Personal");
 
        if (phoneType == null)
            throw new InvalidOperationException("No existe el tipo de teléfono Personal");
 
        var currentPhone = await _context.EmployeePhones
            .FirstOrDefaultAsync(ep => ep.EmployeeId == employeeId && ep.IsPrimary);
 
        if (currentPhone == null)
        {
            _context.EmployeePhones.Add(new EmployeePhone
            {
                EmployeeId = employeeId,
                PhoneTypeId = phoneType.Id,
                PhoneNumber = phone,
                IsPrimary = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            currentPhone.PhoneNumber = phone;
            currentPhone.PhoneTypeId = phoneType.Id;
            currentPhone.IsActive = true;
            currentPhone.UpdatedAt = DateTime.UtcNow;
        }
    }
 
    private async Task SavePrimaryAddressAsync(int employeeId, string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return;
 
        var addressType = await _context.AddressTypes
            .FirstOrDefaultAsync(at => at.Name == "Casa");
 
        if (addressType == null)
            throw new InvalidOperationException("No existe el tipo de dirección Casa");
 
        var province = await _context.Provinces.FirstOrDefaultAsync();
 
        if (province == null)
            throw new InvalidOperationException("No existen provincias registradas");
 
        var canton = await _context.Cantons
            .FirstOrDefaultAsync(c => c.ProvinceId == province.Id);
 
        if (canton == null)
            throw new InvalidOperationException("No existen cantones registrados");
 
        var district = await _context.Districts
            .FirstOrDefaultAsync(d => d.CantonId == canton.Id);
 
        if (district == null)
            throw new InvalidOperationException("No existen distritos registrados");
 
        var currentAddress = await _context.EmployeeAddresses
            .FirstOrDefaultAsync(ea => ea.EmployeeId == employeeId && ea.IsPrimary);
 
        if (currentAddress == null)
        {
            _context.EmployeeAddresses.Add(new EmployeeAddress
            {
                EmployeeId = employeeId,
                AddressTypeId = addressType.Id,
                ProvinceId = province.Id,
                CantonId = canton.Id,
                DistrictId = district.Id,
                ExactAddress = address,
                IsPrimary = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            currentAddress.AddressTypeId = addressType.Id;
            currentAddress.ProvinceId = province.Id;
            currentAddress.CantonId = canton.Id;
            currentAddress.DistrictId = district.Id;
            currentAddress.ExactAddress = address;
            currentAddress.IsActive = true;
            currentAddress.UpdatedAt = DateTime.UtcNow;
        }
    }
}