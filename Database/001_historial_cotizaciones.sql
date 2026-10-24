-- ============================================================================
-- ALTEC - Historial de Cotizaciones (importación de Excel)
-- Fecha: 2026-10-24
--
-- Crea:
--   ImportedQuotations      -> una fila por Excel subido (incluye el archivo original)
--   ImportedQuotationItems  -> filas leídas del Excel (producto, cantidad, precio, total)
--
-- Es idempotente: se puede ejecutar varias veces sin error ni pérdida de datos.
-- Ejecutar en DBeaver conectado a la BD de ALTEC (Alt+X = ejecutar script).
-- ============================================================================

IF OBJECT_ID('dbo.ImportedQuotations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ImportedQuotations (
        Id              uniqueidentifier NOT NULL CONSTRAINT PK_ImportedQuotations PRIMARY KEY,
        Nombre          nvarchar(200)    NOT NULL,
        Fecha           date             NOT NULL,
        NombreArchivo   nvarchar(260)    NOT NULL,
        ContentType     nvarchar(150)    NOT NULL,
        TamanoBytes     bigint           NOT NULL,
        Archivo         varbinary(max)   NOT NULL,   -- Excel original, tal cual se subió (con fotos)
        TotalItems      int              NOT NULL,
        TotalMonto      decimal(18,4)    NOT NULL,
        CreatedAt       datetimeoffset   NOT NULL,
        CreatedBy       nvarchar(100)    NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ImportedQuotations_Fecha'
               AND object_id = OBJECT_ID('dbo.ImportedQuotations'))
    CREATE INDEX IX_ImportedQuotations_Fecha ON dbo.ImportedQuotations (Fecha DESC);
GO

IF OBJECT_ID('dbo.ImportedQuotationItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ImportedQuotationItems (
        Id                  uniqueidentifier NOT NULL CONSTRAINT PK_ImportedQuotationItems PRIMARY KEY,
        ImportedQuotationId uniqueidentifier NOT NULL,
        Fila                int              NOT NULL,   -- nº de fila en el Excel
        Producto            nvarchar(500)    NOT NULL,
        Cantidad            decimal(18,4)    NOT NULL,
        PrecioUnitario      decimal(18,6)    NOT NULL,
        Total               decimal(18,6)    NOT NULL,
        CONSTRAINT FK_ImportedQuotationItems_ImportedQuotations
            FOREIGN KEY (ImportedQuotationId) REFERENCES dbo.ImportedQuotations (Id)
            ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ImportedQuotationItems_ImportedQuotationId'
               AND object_id = OBJECT_ID('dbo.ImportedQuotationItems'))
    CREATE INDEX IX_ImportedQuotationItems_ImportedQuotationId
        ON dbo.ImportedQuotationItems (ImportedQuotationId);
GO
