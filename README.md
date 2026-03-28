# SIGEP - Sistema Integral de Gestión de Personal

MVP (Producto Mínimo Viable) del sistema de gestión de recursos humanos desarrollado con:
- **Backend**: ASP.NET Core Web API (.NET 8)
- **Frontend**: React 18 + Vite
- **Base de datos**: SQL Server (LocalDB)
- **Arquitectura**: Clean Architecture por capas

## 📋 Estructura del Proyecto

```
SIGEP/
├── code/
│   ├── SigepAPI/          # Capa de presentación (API)
│   ├── SigepApplication/  # Capa de aplicación (servicios, DTOs)
│   ├── SigepDomain/       # Capa de dominio (entidades, reglas)
│   └── SigepInfrastructure/ # Capa de infraestructura (DB, repos)
├── code/frontend/              # Aplicación React
└── instructions/          # Documentación del plan
```

## 🚀 Configuración Inicial

### Backend (.NET)

1. **Navegar a la carpeta del API:**
   ```powershell
   cd "c:\Users\USUARIO\OneDrive\Escritorio\SIGEP\code\SigepAPI"
   ```

2. **Restaurar dependencias:**
   ```powershell
   dotnet restore
   ```

3. **Crear la base de datos:**
   ```powershell
   dotnet ef migrations add InitialCreate --project ..\SigepInfrastructure\SigepInfrastructure.csproj --startup-project .\SigepAPI.csproj
   dotnet ef database update --project ..\SigepInfrastructure\SigepInfrastructure.csproj --startup-project .\SigepAPI.csproj
   ```

   > **Nota**: Si no tienes instalada la herramienta de EF Core:
   ```powershell
   dotnet tool install --global dotnet-ef
   ```

4. **Ejecutar el API:**
   ```powershell
   dotnet run
   ```

   El API estará disponible en:
   - HTTPS: `https://localhost:7087`
   - HTTP: `http://localhost:5000`
   - Swagger UI: `https://localhost:7087/swagger`

### Frontend (React)

1. **Navegar a la carpeta del frontend:**
   ```powershell
   cd "c:\Users\USUARIO\OneDrive\Escritorio\SIGEP\code\frontend"
   ```

2. **Instalar dependencias:**
   ```powershell
   npm install
   ```

3. **Ejecutar la aplicación:**
   ```powershell
   npm run dev
   ```

   La aplicación estará disponible en: `http://localhost:3000`

## 👤 Usuarios de Prueba

El sistema incluye tres usuarios precargados:

| Usuario | Contraseña | Rol | Descripción |
|---------|-----------|-----|-------------|
| `admin` | `admin123` | Admin | Acceso completo al sistema |
| `rrhh` | `admin123` | RRHH | Gestión de recursos humanos |
| `juan.perez` | `admin123` | Empleado | Usuario empleado estándar |

## ✨ Funcionalidades del MVP

### Implementado ✅
- ✅ **Autenticación JWT** con roles
- ✅ **Gestión de usuarios** por rol (Admin, RRHH, Jefatura, Empleado)
- ✅ **Visualización de empleados** con información completa
- ✅ **Dashboard** con estadísticas básicas
- ✅ **Entidades base**: User, Employee, Position, Schedule
- ✅ **Seed de datos** inicial automático
- ✅ **CORS** configurado para desarrollo
- ✅ **Swagger** para documentación del API

### Próximamente 📋
- ⏳ Asistencia y marcas de entrada/salida
- ⏳ Gestión de horas extra
- ⏳ Solicitudes de vacaciones
- ⏳ Permisos e incapacidades
- ⏳ Cálculo de planilla
- ⏳ Aguinaldo
- ⏳ Liquidaciones
- ⏳ Reportes y exportación

## 🗂️ Base de Datos

### Tablas Creadas

1. **Users** - Usuarios del sistema
2. **Employees** - Empleados
3. **Positions** - Puestos de trabajo
4. **Schedules** - Horarios laborales

### Conexión

La cadena de conexión por defecto usa **SQL Server LocalDB**:
```
Server=(localdb)\mssqllocaldb;Database=SigepDB;Trusted_Connection=true
```

Si necesitas cambiarla, edita el archivo `code/SigepAPI/appsettings.json`.

## 🛠️ Tecnologías Utilizadas

### Backend
- ASP.NET Core Web API 8.0
- Entity Framework Core 8.0
- SQL Server
- JWT Authentication
- BCrypt.Net para hash de contraseñas
- Swagger/OpenAPI

### Frontend
- React 18
- React Router DOM 6
- Axios
- Vite
- CSS Modules

## 📝 API Endpoints

### Autenticación
- `POST /api/v1/auth/login` - Iniciar sesión
- `GET /api/v1/auth/me` - Obtener usuario actual

### Empleados
- `GET /api/v1/employees` - Listar todos los empleados
- `GET /api/v1/employees/{id}` - Obtener empleado por ID

### Salud
- `GET /api/health` - Health check del API

## 🔒 Seguridad

- Contraseñas hasheadas con BCrypt
- JWT con expiración de 8 horas
- Roles y permisos por endpoint
- CORS configurado para desarrollo

## 📚 Arquitectura

El proyecto sigue **Clean Architecture** con separación de capas:

```
┌─────────────────────────┐
│     SigepAPI (UI)       │  Controllers, Middleware
├─────────────────────────┤
│  SigepApplication       │  Services, DTOs, Interfaces
├─────────────────────────┤
│    SigepDomain          │  Entities, Enums, Rules
├─────────────────────────┤
│ SigepInfrastructure     │  DbContext, Repositories
└─────────────────────────┘
```

## 🐛 Solución de Problemas

### El API no inicia
- Verifica que tienes .NET 8 SDK instalado: `dotnet --version`
- Comprueba que el puerto 7087 no esté en uso
- Revisa los logs en la consola

### Error de conexión a base de datos
- Asegúrate de tener SQL Server LocalDB instalado
- Ejecuta las migraciones: `dotnet ef database update`

### El frontend no conecta con el API
- Verifica que el API esté corriendo en `https://localhost:7087`
- Revisa la configuración de CORS en `Program.cs`
- Comprueba la URL del API en `code/frontend/src/api/api.js`

### Error de certificado HTTPS
- En desarrollo, acepta el certificado de desarrollo
- O configura el API para usar HTTP temporalmente

## 📖 Siguientes Pasos

1. **Explorar el sistema**: Inicia sesión con los usuarios de prueba
2. **Revisar el código**: Familiarízate con la estructura
3. **Agregar módulos**: Sigue el plan en `instructions/plan.md`
4. **Personalizar**: Adapta el sistema a tus necesidades

## 📄 Licencia

Este proyecto es un sistema interno de gestión.

## 👥 Contribuciones

Para agregar nuevas funcionalidades, sigue el plan definido en `instructions/plan.md` y mantén la arquitectura por capas.

---

**Desarrollado con 💙 siguiendo Clean Architecture y mejores prácticas**
