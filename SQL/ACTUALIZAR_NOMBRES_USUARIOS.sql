-- ============================================================
-- Script para actualizar nombres de usuarios con nombres reales
-- ============================================================
USE [JCF_DEV]
GO

-- Actualizar usuarios existentes con nombres reales de personas
UPDATE Usuarios 
SET Nombres = 'Juan Carlos', 
    Apellidos = 'Fernández'
WHERE ID_Usuario = 1;

UPDATE Usuarios 
SET Nombres = 'María José', 
    Apellidos = 'González'
WHERE ID_Usuario = 3;

UPDATE Usuarios 
SET Nombres = 'Pedro Antonio', 
    Apellidos = 'Rojas'
WHERE ID_Usuario = 6;

-- Verificar los cambios
SELECT ID_Usuario, Nombres, Apellidos, Nombres + ' ' + Apellidos AS NombreCompleto, Rol, Activo 
FROM Usuarios 
WHERE Activo = 1;

PRINT '✓ Nombres de usuarios actualizados exitosamente';
GO

