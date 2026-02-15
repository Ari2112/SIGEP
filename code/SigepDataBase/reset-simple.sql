-- ============================================
-- SIGEP - Reset Completo de Base de Datos
-- SQL Server
-- Ejecuta: DROP + CREATE DB + Schema + Seed
-- ============================================

USE master;
GO

-- Cerrar conexiones existentes y eliminar la base de datos si existe
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'SigepDB')
BEGIN
    ALTER DATABASE SigepDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SigepDB;
END
GO

-- Crear la base de datos
CREATE DATABASE SigepDB;
GO

USE SigepDB;
GO

-- ============================================
-- Incluir schema.sql
-- ============================================
:r schema.sql
GO

-- ============================================
-- Incluir seed.sql
-- ============================================
:r seed.sql
GO

PRINT '';
PRINT '==============================================';
PRINT 'Reset completado exitosamente';
PRINT '==============================================';
