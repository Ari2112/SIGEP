using Microsoft.EntityFrameworkCore;
using SigepDomain.Entities;

namespace SigepInfrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // =========================
        // CATÁLOGOS BASE
        // =========================

        if (!await context.UserRoles.AnyAsync())
        {
            await context.UserRoles.AddRangeAsync(
                new UserRole { Name = "Admin", Description = "Administrador del sistema", IsActive = true },
                new UserRole { Name = "RRHH", Description = "Usuario de Recursos Humanos", IsActive = true },
                new UserRole { Name = "Empleado", Description = "Empleado regular", IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.EmployeeStatuses.AnyAsync())
        {
            await context.EmployeeStatuses.AddRangeAsync(
                new EmployeeStatus { Name = "Activo", Description = "Empleado activo", IsActive = true },
                new EmployeeStatus { Name = "Inactivo", Description = "Empleado inactivo", IsActive = true },
                new EmployeeStatus { Name = "Suspendido", Description = "Empleado suspendido", IsActive = true },
                new EmployeeStatus { Name = "Despedido", Description = "Empleado despedido", IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.PhoneTypes.AnyAsync())
        {
            await context.PhoneTypes.AddRangeAsync(
                new PhoneType { Name = "Personal", Description = "Teléfono personal", IsActive = true },
                new PhoneType { Name = "Trabajo", Description = "Teléfono laboral", IsActive = true },
                new PhoneType { Name = "Emergencia", Description = "Teléfono de emergencia", IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.AddressTypes.AnyAsync())
        {
            await context.AddressTypes.AddRangeAsync(
                new AddressType { Name = "Casa", Description = "Dirección de residencia", IsActive = true },
                new AddressType { Name = "Trabajo", Description = "Dirección laboral", IsActive = true },
                new AddressType { Name = "Temporal", Description = "Dirección temporal", IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.Provinces.AnyAsync())
        {
            var sanJose = new Province { Name = "San José", IsActive = true };
            var alajuela = new Province { Name = "Alajuela", IsActive = true };
            var heredia = new Province { Name = "Heredia", IsActive = true };
            var cartago = new Province { Name = "Cartago", IsActive = true };

            await context.Provinces.AddRangeAsync(sanJose, alajuela, heredia, cartago);
            await context.SaveChangesAsync();

            var cantonSanJose = new Canton { Name = "San José", ProvinceId = sanJose.Id, IsActive = true };
            var cantonAlajuela = new Canton { Name = "Alajuela", ProvinceId = alajuela.Id, IsActive = true };
            var cantonHeredia = new Canton { Name = "Heredia", ProvinceId = heredia.Id, IsActive = true };
            var cantonCartago = new Canton { Name = "Cartago", ProvinceId = cartago.Id, IsActive = true };

            await context.Cantons.AddRangeAsync(cantonSanJose, cantonAlajuela, cantonHeredia, cantonCartago);
            await context.SaveChangesAsync();

            await context.Districts.AddRangeAsync(
                new District { Name = "Carmen", CantonId = cantonSanJose.Id, IsActive = true },
                new District { Name = "Alajuela", CantonId = cantonAlajuela.Id, IsActive = true },
                new District { Name = "Heredia", CantonId = cantonHeredia.Id, IsActive = true },
                new District { Name = "Oriental", CantonId = cantonCartago.Id, IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.PayrollStatuses.AnyAsync())
        {
            await context.PayrollStatuses.AddRangeAsync(
                new PayrollStatus { Name = "Borrador", Description = "Planilla en borrador", IsActive = true },
                new PayrollStatus { Name = "Procesada", Description = "Planilla procesada", IsActive = true },
                new PayrollStatus { Name = "Aprobada", Description = "Planilla aprobada", IsActive = true },
                new PayrollStatus { Name = "Pagada", Description = "Planilla pagada", IsActive = true },
                new PayrollStatus { Name = "Anulada", Description = "Planilla anulada", IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.PayrollPeriodTypes.AnyAsync())
        {
            await context.PayrollPeriodTypes.AddRangeAsync(
                new PayrollPeriodType { Name = "Primera Quincena", Description = "Primera quincena del mes", IsActive = true },
                new PayrollPeriodType { Name = "Segunda Quincena", Description = "Segunda quincena del mes", IsActive = true },
                new PayrollPeriodType { Name = "Mensual", Description = "Periodo mensual", IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.RequestStatuses.AnyAsync())
        {
            await context.RequestStatuses.AddRangeAsync(
                new RequestStatus { Name = "Pendiente", Description = "Solicitud pendiente de revisión", IsActive = true },
                new RequestStatus { Name = "Aprobada", Description = "Solicitud aprobada", IsActive = true },
                new RequestStatus { Name = "Rechazada", Description = "Solicitud rechazada", IsActive = true },
                new RequestStatus { Name = "Cancelada", Description = "Solicitud cancelada", IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.AttendanceStatuses.AnyAsync())
        {
            await context.AttendanceStatuses.AddRangeAsync(
                new AttendanceStatus { Name = "Parcial", Description = "Asistencia parcial", IsActive = true },
                new AttendanceStatus { Name = "Completo", Description = "Asistencia completa", IsActive = true },
                new AttendanceStatus { Name = "Ausente", Description = "Empleado ausente", IsActive = true },
                new AttendanceStatus { Name = "Permiso", Description = "Empleado con permiso", IsActive = true },
                new AttendanceStatus { Name = "Vacaciones", Description = "Empleado en vacaciones", IsActive = true },
                new AttendanceStatus { Name = "Incapacidad", Description = "Empleado con incapacidad", IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.DisabilityTypes.AnyAsync())
        {
            await context.DisabilityTypes.AddRangeAsync(
                new DisabilityType { Name = "Enfermedad Común", Description = "Incapacidad por enfermedad común", IsActive = true },
                new DisabilityType { Name = "Accidente Laboral", Description = "Incapacidad por accidente laboral", IsActive = true },
                new DisabilityType { Name = "Maternidad", Description = "Licencia o incapacidad por maternidad", IsActive = true },
                new DisabilityType { Name = "Otro", Description = "Otro tipo de incapacidad", IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.TerminationTypes.AnyAsync())
        {
            await context.TerminationTypes.AddRangeAsync(
                new TerminationType { Name = "Renuncia", Description = "Renuncia voluntaria", HasSeverance = false, IsActive = true },
                new TerminationType { Name = "Despido con Responsabilidad", Description = "Despido con responsabilidad patronal", HasSeverance = true, IsActive = true },
                new TerminationType { Name = "Despido sin Responsabilidad", Description = "Despido sin responsabilidad patronal", HasSeverance = false, IsActive = true },
                new TerminationType { Name = "Mutuo Acuerdo", Description = "Terminación por mutuo acuerdo", HasSeverance = false, IsActive = true },
                new TerminationType { Name = "Jubilación", Description = "Salida por jubilación", HasSeverance = false, IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.PublicHolidays.AnyAsync())
        {
            await context.PublicHolidays.AddRangeAsync(
                new PublicHoliday { Date = new DateTime(2026, 1, 1), Name = "Año Nuevo", IsMandatoryPayment = true, IsActive = true },
                new PublicHoliday { Date = new DateTime(2026, 4, 11), Name = "Batalla de Rivas", IsMandatoryPayment = true, IsActive = true },
                new PublicHoliday { Date = new DateTime(2026, 5, 1), Name = "Día del Trabajador", IsMandatoryPayment = true, IsActive = true },
                new PublicHoliday { Date = new DateTime(2026, 7, 25), Name = "Anexión del Partido de Nicoya", IsMandatoryPayment = true, IsActive = true },
                new PublicHoliday { Date = new DateTime(2026, 8, 15), Name = "Día de la Madre", IsMandatoryPayment = true, IsActive = true },
                new PublicHoliday { Date = new DateTime(2026, 9, 15), Name = "Día de la Independencia", IsMandatoryPayment = true, IsActive = true },
                new PublicHoliday { Date = new DateTime(2026, 12, 25), Name = "Navidad", IsMandatoryPayment = true, IsActive = true }
            );

            await context.SaveChangesAsync();
        }

        // =========================
        // HORARIOS
        // =========================

        if (!await context.Schedules.AnyAsync())
        {
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
        }

        // =========================
        // POSICIONES
        // =========================

        if (!await context.Positions.AnyAsync())
        {
            var positions = new List<Position>
            {
                new Position { Name = "Gerente General", Description = "Gerente de la empresa", IsActive = true },
                new Position { Name = "Supervisor", Description = "Supervisor de área", IsActive = true },
                new Position { Name = "Analista", Description = "Analista general", IsActive = true },
                new Position { Name = "Asistente", Description = "Asistente administrativo", IsActive = true }
            };

            await context.Positions.AddRangeAsync(positions);
            await context.SaveChangesAsync();
        }

        // =========================
        // EMPLEADOS
        // =========================

        if (!await context.Employees.AnyAsync())
        {
            var activeStatus = await context.EmployeeStatuses.FirstAsync(x => x.Name == "Activo");
            var positions = await context.Positions.OrderBy(x => x.Id).ToListAsync();
            var schedules = await context.Schedules.OrderBy(x => x.Id).ToListAsync();

            var employees = new List<Employee>
            {
                new Employee
                {
                    FirstName = "Admin",
                    LastName = "Sistema",
                    IdentificationNumber = "000000000",
                    Email = "admin@sigep.com",
                    HireDate = DateTime.UtcNow,
                    BaseSalary = 1000000,
                    EmployeeStatusId = activeStatus.Id,
                    PositionId = positions[0].Id,
                    ScheduleId = schedules[0].Id
                },
                new Employee
                {
                    FirstName = "Juan",
                    LastName = "Pérez",
                    IdentificationNumber = "123456789",
                    Email = "juan.perez@sigep.com",
                    HireDate = DateTime.UtcNow.AddYears(-2),
                    BaseSalary = 500000,
                    EmployeeStatusId = activeStatus.Id,
                    PositionId = positions[2].Id,
                    ScheduleId = schedules[0].Id
                },
                new Employee
                {
                    FirstName = "María",
                    LastName = "González",
                    IdentificationNumber = "987654321",
                    Email = "maria.gonzalez@sigep.com",
                    HireDate = DateTime.UtcNow.AddYears(-1),
                    BaseSalary = 600000,
                    EmployeeStatusId = activeStatus.Id,
                    PositionId = positions[1].Id,
                    ScheduleId = schedules[0].Id
                }
            };

            await context.Employees.AddRangeAsync(employees);
            await context.SaveChangesAsync();

            var personalPhoneType = await context.PhoneTypes.FirstAsync(x => x.Name == "Personal");
            var homeAddressType = await context.AddressTypes.FirstAsync(x => x.Name == "Casa");

            var province = await context.Provinces.FirstAsync(x => x.Name == "San José");
            var canton = await context.Cantons.FirstAsync(x => x.Name == "San José" && x.ProvinceId == province.Id);
            var district = await context.Districts.FirstAsync(x => x.CantonId == canton.Id);

            await context.EmployeePhones.AddRangeAsync(
                new EmployeePhone
                {
                    EmployeeId = employees[0].Id,
                    PhoneTypeId = personalPhoneType.Id,
                    PhoneNumber = "0000-0000",
                    IsPrimary = true,
                    IsActive = true
                },
                new EmployeePhone
                {
                    EmployeeId = employees[1].Id,
                    PhoneTypeId = personalPhoneType.Id,
                    PhoneNumber = "8888-8888",
                    IsPrimary = true,
                    IsActive = true
                },
                new EmployeePhone
                {
                    EmployeeId = employees[2].Id,
                    PhoneTypeId = personalPhoneType.Id,
                    PhoneNumber = "7777-7777",
                    IsPrimary = true,
                    IsActive = true
                }
            );

            await context.EmployeeAddresses.AddRangeAsync(
                new EmployeeAddress
                {
                    EmployeeId = employees[0].Id,
                    AddressTypeId = homeAddressType.Id,
                    ProvinceId = province.Id,
                    CantonId = canton.Id,
                    DistrictId = district.Id,
                    ExactAddress = "San José, Costa Rica",
                    IsPrimary = true,
                    IsActive = true
                },
                new EmployeeAddress
                {
                    EmployeeId = employees[1].Id,
                    AddressTypeId = homeAddressType.Id,
                    ProvinceId = province.Id,
                    CantonId = canton.Id,
                    DistrictId = district.Id,
                    ExactAddress = "San José, Costa Rica",
                    IsPrimary = true,
                    IsActive = true
                },
                new EmployeeAddress
                {
                    EmployeeId = employees[2].Id,
                    AddressTypeId = homeAddressType.Id,
                    ProvinceId = province.Id,
                    CantonId = canton.Id,
                    DistrictId = district.Id,
                    ExactAddress = "San José, Costa Rica",
                    IsPrimary = true,
                    IsActive = true
                }
            );

            await context.SaveChangesAsync();
        }

        // =========================
        // USUARIOS
        // =========================

        if (!await context.Users.AnyAsync())
        {
            var adminRole = await context.UserRoles.FirstAsync(x => x.Name == "Admin");
            var rrhhRole = await context.UserRoles.FirstAsync(x => x.Name == "RRHH");
            var empleadoRole = await context.UserRoles.FirstAsync(x => x.Name == "Empleado");

            var adminEmployee = await context.Employees.FirstAsync(x => x.Email == "admin@sigep.com");
            var juanEmployee = await context.Employees.FirstAsync(x => x.Email == "juan.perez@sigep.com");

            var users = new List<User>
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@sigep.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    RoleId = adminRole.Id,
                    IsActive = true,
                    EmployeeId = adminEmployee.Id
                },
                new User
                {
                    Username = "rrhh",
                    Email = "rrhh@sigep.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    RoleId = rrhhRole.Id,
                    IsActive = true
                },
                new User
                {
                    Username = "juan.perez",
                    Email = "juan.perez@sigep.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    RoleId = empleadoRole.Id,
                    IsActive = true,
                    EmployeeId = juanEmployee.Id
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }
    }
}