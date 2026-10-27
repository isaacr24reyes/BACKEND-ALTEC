-- ============================================================================
-- ALTEC - Anulación de facturas electrónicas
-- Fecha: 2026-10-27
--
-- Agrega a ElectronicInvoices dos columnas opcionales (no modifica ni borra datos):
--   FechaAnulacion   fecha en que se registró la anulación en ALTEC
--   MotivoAnulacion  motivo indicado por el usuario
--
-- IMPORTANTE: ejecutar ANTES de desplegar el backend que las usa.
-- Formato compatible con DBeaver: sentencias independientes, sin BEGIN/END ni GO.
-- Es idempotente: se puede ejecutar varias veces sin error.
-- Ejecutar en DBeaver sobre altecprod con Alt+X (todo el script).
-- ============================================================================

IF COL_LENGTH('dbo.ElectronicInvoices', 'FechaAnulacion') IS NULL ALTER TABLE dbo.ElectronicInvoices ADD FechaAnulacion datetimeoffset NULL;

IF COL_LENGTH('dbo.ElectronicInvoices', 'MotivoAnulacion') IS NULL ALTER TABLE dbo.ElectronicInvoices ADD MotivoAnulacion nvarchar(300) NULL;

-- Comprobación: debe devolver las dos columnas
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ElectronicInvoices' AND COLUMN_NAME IN ('FechaAnulacion', 'MotivoAnulacion');
