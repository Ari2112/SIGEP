using Microsoft.EntityFrameworkCore;
using SigepDomain.Entities;
using SigepDomain.Enums;

namespace SigepInfrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!await context.Users.AnyAsync())
        {
            // Crear horarios
            var schedules = new List<Schedule>
            {
                new Schedule
                {
                    Name = "Jornada Completa 8:00-17:00",
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    WorkHoursPerDay = 8,
                    IsActive = true
                },
                new Schedule
                {
                    Name = "Jornada Matutina 6:00-14:00",
                    StartTime = new TimeSpan(6, 0, 0),
                    EndTime = new TimeSpan(14, 0, 0),
                    WorkHoursPerDay = 8,
                    IsActive = true
                }
            };
            await context.Schedules.AddRangeAsync(schedules);
            await context.SaveChangesAsync();

            // Crear posiciones
            var positions = new List<Position>
            {
                new Position { Name = "Gerente General", Description = "Gerente de la empresa", IsActive = true },
                new Position { Name = "Supervisor", Description = "Supervisor de área", IsActive = true },
                new Position { Name = "Analista", Description = "Analista general", IsActive = true },
                new Position { Name = "Asistente", Description = "Asistente administrativo", IsActive = true }
            };
            await context.Positions.AddRangeAsync(positions);
            await context.SaveChangesAsync();

            // Crear empleados
            var employees = new List<Employee>
            {
                new Employee
                {
                    FirstName = "Admin",
                    LastName = "Sistema",
                    IdentificationNumber = "000000000",
                    Email = "admin@sigep.com",
                    Phone = "0000-0000",
                    HireDate = DateTime.UtcNow,
                    BaseSalary = 1000000,
                    Status = EmployeeStatus.Activo,
                    PositionId = positions[0].Id,
                    ScheduleId = schedules[0].Id
                },
                new Employee
                {
                    FirstName = "Juan",
                    LastName = "Pérez",
                    IdentificationNumber = "123456789",
                    Email = "juan.perez@sigep.com",
                    Phone = "8888-8888",
                    HireDate = DateTime.UtcNow.AddYears(-2),
                    BaseSalary = 500000,
                    Status = EmployeeStatus.Activo,
                    PositionId = positions[2].Id,
                    ScheduleId = schedules[0].Id
                },
                new Employee
                {
                    FirstName = "María",
                    LastName = "González",
                    IdentificationNumber = "987654321",
                    Email = "maria.gonzalez@sigep.com",
                    Phone = "7777-7777",
                    HireDate = DateTime.UtcNow.AddYears(-1),
                    BaseSalary = 600000,
                    Status = EmployeeStatus.Activo,
                    PositionId = positions[1].Id,
                    ScheduleId = schedules[0].Id
                }
            };
            await context.Employees.AddRangeAsync(employees);
            await context.SaveChangesAsync();

            // Crear usuarios (password: admin123)
            var users = new List<User>
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@sigep.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = UserRole.Admin,
                    IsActive = true,
                    EmployeeId = employees[0].Id
                },
                new User
                {
                    Username = "rrhh",
                    Email = "rrhh@sigep.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = UserRole.RRHH,
                    IsActive = true
                },
                new User
                {
                    Username = "juan.perez",
                    Email = "juan.perez@sigep.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = UserRole.Empleado,
                    IsActive = true,
                    EmployeeId = employees[1].Id
                }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }
    }
}
