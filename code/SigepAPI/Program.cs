//  Este es el PUNTO DE ARRANQUE del backend, es el servidor.
//  sistema empieza a atender peticiones:
//    - La conexión a la base de datos.
//    - La seguridad con tokens (JWT).
//    - El permiso para que el frontend pueda hablar con el backend (CORS).
//    - El registro de todos los servicios (la lógica de cada módulo).
//    - La documentación automática (Swagger).
//  Al final, arranca el servidor y queda escuchando peticiones.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SigepApplication.Interfaces;
using SigepInfrastructure.Services;
using SigepInfrastructure.Persistence;
using System.Text;

// "builder" es el armador de la aplicación. Aquí le vamos agregando piezas.
var builder = WebApplication.CreateBuilder(args);

// Activamos los controladores  y
// configuramos cómo se traduce la información a JSON: nombres en minúscula
// inicial (camelCase) y permitiendo tildes y caracteres especiales sin romperse.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

// Conexión a la base de datos SQL Server. La cadena de conexión (dónde está
// la base y cómo entrar) se lee del archivo de configuración, no se escribe aquí.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuración de correo (EmailSettings): lee la sección "EmailSettings" de
// appsettings.json / user-secrets y la deja disponible como IOptions<EmailSettings>
// para quien la necesite (hoy: EmailService).
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Seguridad con tokens (JWT) 
// Leemos la configuración del token y la llave secreta para firmarlo/validarlo.
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "MySecretKeyForSigepSystem2026VeryLongAndSecure123!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Aquí decimos QUÉ revisar de cada token para considerarlo válido, que
    // venga de nuestro emisor, para nuestra audiencia, que no esté vencido y
    // que su firma coincida con nuestra llave secreta. Es lo que evita tokens
    // falsificados.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Por seguridad, los navegadores bloquean que una página le hable a un servidor
// de otra dirección. Aquí damos permiso explícito a nuestro frontend (que corre
// en localhost:5173 con Vite, o 3000) para que SÍ pueda comunicarse con la API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
// Aquí presentamos cada  interfaz con su implementación real. Esto
// es la "inyección de dependencias": cuando un controlador pide, por ejemplo,
// un IPayrollService, .NET sabe que debe entregarle un PayrollService.
// "Scoped" significa que se crea uno nuevo por cada petición.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IVacationService, VacationService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IOvertimeService, OvertimeService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<ISettlementService, SettlementService>();
builder.Services.AddScoped<IAnnualBonusService, AnnualBonusService>();
builder.Services.AddScoped<IPerformanceEvaluationService, PerformanceEvaluationService>();
builder.Services.AddScoped<IDisabilityService, DisabilityService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<SigepInfrastructure.Services.PayrollPdfService>();

// Swagger genera una página web automática para PROBAR la API durante el
// desarrollo (ver todos los endpoints y ejecutarlos). Aquí también le decimos
// que acepte el token para poder probar las rutas protegidas.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SIGEP API",
        Version = "v1",
        Description = "Sistema Integral de Gestión de Personal"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Activamos la licencia gratuita (Community) de QuestPDF, la librería que usamos
// para generar los PDF (por ejemplo, las colillas de pago).
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Con todas las piezas configuradas, construimos la aplicación.
var app = builder.Build();

// Carpeta física donde se guardan y se sirven los archivos subidos (por ejemplo,
// los documentos de las incapacidades). La anclamos a la raíz del proyecto para
// que NO dependa de configuraciones que podrían venir vacías al iniciar. El
// controlador de incapacidades guarda en ESTA misma ruta, así subir y mostrar
// siempre coinciden.
var uploadsRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(Path.Combine(uploadsRoot, "uploads", "disabilities"));

// Antes de empezar, probamos si podemos conectarnos a la base. Si sí, además
// sembramos los catálogos básicos (roles, tipos de permiso, etc.) en caso de
// que estén vacíos. Si no conecta, lo registramos para avisar qué hacer.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (await context.Database.CanConnectAsync())
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Conexión a base de datos exitosa.");
            await SigepInfrastructure.Persistence.DbInitializer.SeedAsync(context);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "No se puede conectar a la base de datos. Ejecuta reset-db.bat en SigepDataBase/");
    }
}

// Cada petición pasa por estos pasos en orden. Pensemos en una fila de filtros.

// Solo en desarrollo mostramos la página de Swagger.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();      // forzar conexión segura (https)
app.UseCors("AllowFrontend");   // aplicar el permiso para el frontend

// Servir los archivos subidos (documentos de incapacidades) desde la carpeta
// que preparamos arriba.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot)
});

app.UseAuthentication();        // ¿quién es la persona? (revisa el token)
app.UseAuthorization();         // ¿tiene permiso para esto? (revisa el rol)
app.MapControllers();           // dirigir cada petición a su controlador
app.Run();                      // arrancar!                // arrancar!