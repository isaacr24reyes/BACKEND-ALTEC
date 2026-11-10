-- ============================================================================
-- ALTEC - Costos extras de importación
-- Fecha: 2026-11-10
--
-- Agrega a ImportedQuotations dos columnas (no modifica ni borra datos):
--   CostosExtras      gastos para que la mercadería llegue a Ecuador (aduana, IVA,
--                     comisión Alibaba, impuesto TC, flete local, etc.). Por defecto 0.
--   CostosExtrasNota  detalle opcional de esos gastos.
--
-- Con este valor ALTEC calcula el "Costo en Ecuador" de cada producto, repartiendo
-- los gastos según el valor de cada línea en la importación.
--
-- IMPORTANTE: ejecutar ANTES de usar el backend que las lee.
-- Formato compatible con DBeaver: sentencias independientes, sin BEGIN/END ni GO.
-- Es idempotente: se puede ejecutar varias veces sin error.
-- Ejecutar en DBeaver sobre altecprod con Alt+X (todo el script) y luego Commit.
-- ============================================================================

IF COL_LENGTH('dbo.ImportedQuotations', 'CostosExtras') IS NULL ALTER TABLE dbo.ImportedQuotations ADD CostosExtras decimal(18,4) NOT NULL CONSTRAINT DF_ImportedQuotations_CostosExtras DEFAULT (0);

IF COL_LENGTH('dbo.ImportedQuotations', 'CostosExtrasNota') IS NULL ALTER TABLE dbo.ImportedQuotations ADD CostosExtrasNota nvarchar(300) NULL;

-- Comprobación: debe devolver las dos columnas
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ImportedQuotations' AND COLUMN_NAME IN ('CostosExtras', 'CostosExtrasNota');
