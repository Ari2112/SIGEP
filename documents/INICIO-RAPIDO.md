# 🚀 Guía Rápida de Inicio - SIGEP MVP

## Opción 1: Inicio Automático (Recomendado)

Simplemente ejecuta el script principal que iniciará todo:

```powershell
.\start-all.ps1
```

Este script abrirá dos ventanas de PowerShell:
- Una para el Backend (API)
- Otra para el Frontend (React)

## Opción 2: Inicio Manual

### Backend (Terminal 1)

```powershell
cd "c:\Users\USUARIO\OneDrive\Escritorio\SIGEP\code\SigepAPI"
dotnet restore
dotnet ef database update --project ..\SigepInfrastructure\SigepInfrastructure.csproj
dotnet run
```

### Frontend (Terminal 2)

```powershell
cd "c:\Users\USUARIO\OneDrive\Escritorio\SIGEP\code\frontend"
npm install
npm run dev
```

## ⚙️ Requisitos Previos

Asegúrate de tener instalado:

- ✅ .NET 8 SDK ([Descargar](https://dotnet.microsoft.com/download/dotnet/8.0))
- ✅ Node.js 18+ ([Descargar](https://nodejs.org/))
- ✅ SQL Server LocalDB (viene con Visual Studio o SQL Server Express)

### Verificar instalación:

```powershell
dotnet --version    # Debe mostrar 8.x.x
node --version      # Debe mostrar v18.x.x o superior
npm --version       # Debe mostrar 9.x.x o superior
```

## 📝 Primera Vez

Si es la primera vez que ejecutas el proyecto:

### 1. Instalar herramienta EF Core (solo una vez)

```powershell
dotnet tool install --global dotnet-ef
```

### 2. Crear la base de datos

```powershell
cd "c:\Users\USUARIO\OneDrive\Escritorio\SIGEP\code\SigepAPI"
dotnet ef migrations add InitialCreate --project ..\SigepInfrastructure\SigepInfrastructure.csproj
dotnet ef database update --project ..\SigepInfrastructure\SigepInfrastructure.csproj
```

### 3. Instalar dependencias del frontend

```powershell
cd "c:\Users\USUARIO\OneDrive\Escritorio\SIGEP\code\frontend"
npm install
```

## 🌐 URLs del Sistema

Una vez iniciado:

- **Frontend**: http://localhost:3000
- **API**: https://localhost:7087
- **Swagger UI**: https://localhost:7087/swagger
- **Health Check**: https://localhost:7087/api/health

## 👤 Credenciales de Acceso

| Usuario | Contraseña | Rol | Descripción |
|---------|-----------|-----|-------------|
| `admin` | `admin123` | Admin | Administrador del sistema |
| `rrhh` | `admin123` | RRHH | Recursos Humanos |
| `juan.perez` | `admin123` | Empleado | Usuario empleado |

## ✅ Verificar que Todo Funciona

1. **Backend**:
   - Abre https://localhost:7087/api/health
   - Deberías ver: `{"status":"healthy","timestamp":"...","version":"1.0.0"}`

2. **Frontend**:
   - Abre http://localhost:3000
   - Deberías ver la pantalla de login

3. **Login**:
   - Usuario: `admin`
   - Contraseña: `admin123`
   - Deberías entrar al Dashboard

4. **Empleados**:
   - Click en "Empleados" en el menú
   - Deberías ver 3 empleados (Admin Sistema, Juan Pérez, María González)

## 🐛 Solución de Problemas Comunes

### Error: Puerto en uso

Si el puerto 7087 o 3000 está en uso:

**Backend**: Edita `code/SigepAPI/Properties/launchSettings.json`
**Frontend**: Edita `code/frontend/vite.config.js` y cambia el puerto

### Error: No se puede conectar a la base de datos

```powershell
# Verificar SQL Server LocalDB
sqllocaldb info

# Iniciar LocalDB si está detenido
sqllocaldb start MSSQLLocalDB
```

### Error: Certificado SSL no confiable

En desarrollo, acepta el certificado en tu navegador o ejecuta:

```powershell
dotnet dev-certs https --trust
```

### Error: npm install falla

Intenta limpiar el caché:

```powershell
npm cache clean --force
npm install
```

### Error: EF Core no instalado

```powershell
dotnet tool install --global dotnet-ef --version 8.0.23
```

## 📚 Próximos Pasos

1. ✅ Explora el Dashboard
2. ✅ Revisa la lista de empleados
3. ✅ Prueba diferentes usuarios y roles
4. 📖 Lee el README.md completo
5. 🔨 Revisa el código fuente
6. 📋 Consulta el plan de desarrollo en `instructions/plan.md`

## 📞 Estructura de Carpetas

```
SIGEP/
├── code/
│   ├── SigepAPI/              # ← Backend API
│   ├── SigepApplication/      # ← Servicios y lógica
│   ├── SigepDomain/           # ← Entidades y reglas
│   └── SigepInfrastructure/   # ← Base de datos
├── code/frontend/                   # ← React App
│   ├── src/
│   │   ├── api/               # ← Llamadas al API
│   │   ├── components/        # ← Componentes reutilizables
│   │   ├── context/           # ← Context API (Auth)
│   │   └── pages/             # ← Páginas de la app
│   └── package.json
├── instructions/
│   └── plan.md                # ← Plan de desarrollo
├── start-all.ps1              # ← Script para iniciar todo
├── start-backend.ps1          # ← Script solo backend
├── start-frontend.ps1         # ← Script solo frontend
└── README.md                  # ← Documentación completa
```

## 🎯 Funcionalidades del MVP

- ✅ Login con JWT
- ✅ Gestión de roles
- ✅ Dashboard
- ✅ Lista de empleados
- ⏳ Más módulos en desarrollo...

---

**¡Listo! Ya tienes el MVP funcionando. Ahora puedes empezar a desarrollar más funcionalidades siguiendo el plan.md**
