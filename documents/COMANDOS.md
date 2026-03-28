# 📝 Comandos Útiles - SIGEP

## 🎯 Inicio Rápido

### Iniciar todo el sistema
```powershell
.\start-all.ps1
```

### Solo Backend
```powershell
.\start-backend.ps1
```

### Solo Frontend
```powershell
.\start-frontend.ps1
```

---

## 🗄️ Base de Datos (Entity Framework)

### Crear una nueva migración
```powershell
cd code\SigepAPI
dotnet ef migrations add NombreMigracion --project ..\SigepInfrastructure\SigepInfrastructure.csproj --startup-project .\SigepAPI.csproj
```

### Aplicar migraciones
```powershell
dotnet ef database update --project ..\SigepInfrastructure\SigepInfrastructure.csproj --startup-project .\SigepAPI.csproj
```

### Revertir migración
```powershell
dotnet ef database update NombreMigracionAnterior --project ..\SigepInfrastructure\SigepInfrastructure.csproj --startup-project .\SigepAPI.csproj
```

### Eliminar última migración (no aplicada)
```powershell
dotnet ef migrations remove --project ..\SigepInfrastructure\SigepInfrastructure.csproj --startup-project .\SigepAPI.csproj
```

### Generar script SQL de migración
```powershell
dotnet ef migrations script --project ..\SigepInfrastructure\SigepInfrastructure.csproj --startup-project .\SigepAPI.csproj
```

### Ver información de la base de datos
```powershell
dotnet ef dbcontext info --project ..\SigepInfrastructure\SigepInfrastructure.csproj --startup-project .\SigepAPI.csproj
```

### Eliminar base de datos
```powershell
dotnet ef database drop --project ..\SigepInfrastructure\SigepInfrastructure.csproj --startup-project .\SigepAPI.csproj
```

---

## 🔨 Backend (.NET)

### Restaurar paquetes
```powershell
cd code\SigepAPI
dotnet restore
```

### Compilar
```powershell
dotnet build
```

### Ejecutar
```powershell
dotnet run
```

### Ejecutar con hot reload
```powershell
dotnet watch run
```

### Limpiar
```powershell
dotnet clean
```

### Agregar paquete NuGet
```powershell
dotnet add package NombrePaquete
```

### Listar paquetes
```powershell
dotnet list package
```

### Ejecutar tests (cuando se creen)
```powershell
dotnet test
```

---

## ⚛️ Frontend (React)

### Instalar dependencias
```powershell
cd coderontend
npm install
```

### Ejecutar en desarrollo
```powershell
npm run dev
```

### Compilar para producción
```powershell
npm run build
```

### Previsualizar build de producción
```powershell
npm run preview
```

### Agregar paquete
```powershell
npm install nombre-paquete
```

### Agregar paquete de desarrollo
```powershell
npm install --save-dev nombre-paquete
```

### Actualizar paquetes
```powershell
npm update
```

### Limpiar node_modules
```powershell
Remove-Item -Recurse -Force node_modules
npm install
```

---

## 🛠️ SQL Server LocalDB

### Ver instancias
```powershell
sqllocaldb info
```

### Iniciar instancia
```powershell
sqllocaldb start MSSQLLocalDB
```

### Detener instancia
```powershell
sqllocaldb stop MSSQLLocalDB
```

### Ver información de instancia
```powershell
sqllocaldb info MSSQLLocalDB
```

### Crear nueva instancia
```powershell
sqllocaldb create NombreInstancia
```

### Eliminar instancia
```powershell
sqllocaldb delete NombreInstancia
```

---

## 🔐 Certificados SSL

### Confiar en certificado de desarrollo
```powershell
dotnet dev-certs https --trust
```

### Limpiar certificados
```powershell
dotnet dev-certs https --clean
```

### Verificar certificados
```powershell
dotnet dev-certs https --check
```

---

## 📦 Git (Control de Versiones)

### Inicializar repositorio
```powershell
git init
```

### Ver estado
```powershell
git status
```

### Agregar todos los archivos
```powershell
git add .
```

### Hacer commit
```powershell
git commit -m "Mensaje del commit"
```

### Ver historial
```powershell
git log --oneline
```

### Crear rama
```powershell
git branch nombre-rama
```

### Cambiar de rama
```powershell
git checkout nombre-rama
```

### Crear y cambiar a rama
```powershell
git checkout -b nombre-rama
```

---

## 🧪 Testing del API (con PowerShell)

### Health Check
```powershell
Invoke-RestMethod -Uri "https://localhost:7087/api/health" -Method Get
```

### Login
```powershell
$body = @{
    username = "admin"
    password = "admin123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7087/api/v1/auth/login" -Method Post -Body $body -ContentType "application/json"
$token = $response.token
```

### Obtener empleados (con token)
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
}

Invoke-RestMethod -Uri "https://localhost:7087/api/v1/employees" -Method Get -Headers $headers
```

---

## 🔍 Diagnóstico

### Verificar versiones instaladas
```powershell
dotnet --version
node --version
npm --version
dotnet ef --version
```

### Ver procesos en puertos específicos
```powershell
# Puerto del backend (7087)
netstat -ano | findstr :7087

# Puerto del frontend (3000)
netstat -ano | findstr :3000
```

### Matar proceso por PID
```powershell
Stop-Process -Id PID -Force
```

### Limpiar caché de NuGet
```powershell
dotnet nuget locals all --clear
```

### Limpiar caché de npm
```powershell
npm cache clean --force
```

---

## 📊 Información del Proyecto

### Ver estructura de la solución
```powershell
cd code
dotnet sln list
```

### Agregar proyecto a la solución
```powershell
dotnet sln add .\NuevoProyecto\NuevoProyecto.csproj
```

### Ver dependencias del proyecto
```powershell
cd SigepAPI
dotnet list reference
```

---

## 🎨 Formato y Calidad de Código

### Formatear código .NET
```powershell
cd code\SigepAPI
dotnet format
```

### Ver advertencias de compilación
```powershell
dotnet build /p:TreatWarningsAsErrors=true
```

---

## 🚀 Producción

### Publicar backend
```powershell
cd code\SigepAPI
dotnet publish -c Release -o ./publish
```

### Build de frontend para producción
```powershell
cd coderontend
npm run build
```

---

## 💡 Tips

### Ejecutar múltiples comandos en PowerShell
```powershell
comando1; comando2; comando3
```

### Ejecutar comando en background
```powershell
Start-Job -ScriptBlock { dotnet run }
```

### Ver ayuda de comandos dotnet
```powershell
dotnet --help
dotnet ef --help
dotnet build --help
```

---

## 📖 Recursos Útiles

- **Swagger UI**: https://localhost:7087/swagger
- **Health Check**: https://localhost:7087/api/health
- **Frontend**: http://localhost:3000
- **Documentación EF Core**: https://docs.microsoft.com/ef/core/
- **Documentación .NET**: https://docs.microsoft.com/dotnet/
- **Documentación React**: https://react.dev/

---

**Mantén este archivo a mano para consultar comandos rápidamente! 📝**
