-- ============================================================================
-- ALTEC - Facturación electrónica SRI (esquema offline, factura v1.1.0)
-- Fecha: 2026-10-27
--
-- Crea: SriSettings, Customers, ElectronicInvoices, ElectronicInvoiceItems, ElectronicInvoicePayments
--
-- Formato compatible con DBeaver: cada sentencia termina en ";" y no hay bloques BEGIN/END ni GO.
-- Es idempotente: se puede ejecutar varias veces sin error ni pérdida de datos.
-- Ejecutar en DBeaver sobre altecprod con Alt+X (todo el script).
-- ============================================================================

-- 1) Configuración del emisor (una sola fila, Id = 1)
IF OBJECT_ID('dbo.SriSettings', 'U') IS NULL CREATE TABLE dbo.SriSettings (
    Id                            int            NOT NULL CONSTRAINT PK_SriSettings PRIMARY KEY,
    Ruc                           char(13)       NOT NULL,
    RazonSocial                   nvarchar(300)  NOT NULL,
    NombreComercial               nvarchar(300)  NULL,
    DirMatriz                     nvarchar(300)  NOT NULL,
    DirEstablecimiento            nvarchar(300)  NOT NULL,
    ObligadoContabilidad          varchar(2)     NOT NULL,
    ContribuyenteRimpe            nvarchar(100)  NULL,
    ContribuyenteEspecial         nvarchar(13)   NULL,
    AgenteRetencion               nvarchar(8)    NULL,
    Estab                         char(3)        NOT NULL,
    PtoEmi                        char(3)        NOT NULL,
    SiguienteSecuencialPruebas    int            NOT NULL,
    SiguienteSecuencialProduccion int            NOT NULL,
    CodigoPorcentajeIva           varchar(4)     NOT NULL,
    TarifaIva                     decimal(5,2)   NOT NULL,
    EmailCopia                    nvarchar(300)  NULL,
    UpdatedAt                     datetimeoffset NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM dbo.SriSettings WHERE Id = 1) INSERT INTO dbo.SriSettings (
    Id, Ruc, RazonSocial, NombreComercial, DirMatriz, DirEstablecimiento, ObligadoContabilidad,
    ContribuyenteRimpe, Estab, PtoEmi, SiguienteSecuencialPruebas, SiguienteSecuencialProduccion,
    CodigoPorcentajeIva, TarifaIva, EmailCopia, UpdatedAt)
VALUES (1, '1707873145001', N'REYES CUJI ROBINSON ISAAC', N'MECATRONICA.',
    N'Barrio: VILLA FLORA Calle: AV. RODRIGO DE CHAVEZ Numero: 0E1-72 Interseccion: FRANCISCO GOMEZ',
    N'Barrio: VILLA FLORA Calle: AV. RODRIGO DE CHAVEZ Numero: 0E1-72 Interseccion: FRANCISCO GOMEZ',
    'NO', N'CONTRIBUYENTE NEGOCIO POPULAR - RÉGIMEN RIMPE', '001', '001',
    1,     -- pruebas: empieza en 1
    307,   -- producción: CONFIRMAR antes de pasar a producción (último emitido + 1)
    '0', 0, N'mecatronicaltec@gmail.com', SYSDATETIMEOFFSET());

-- 2) Clientes
IF OBJECT_ID('dbo.Customers', 'U') IS NULL CREATE TABLE dbo.Customers (
    Id                  uniqueidentifier NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    TipoIdentificacion  char(2)          NOT NULL,   -- 04 RUC, 05 cédula, 06 pasaporte, 07 consumidor final
    Identificacion      nvarchar(20)     NOT NULL,
    RazonSocial         nvarchar(300)    NOT NULL,
    Direccion           nvarchar(300)    NULL,
    Email               nvarchar(300)    NULL,
    Telefono            nvarchar(30)     NULL,
    CreatedAt           datetimeoffset   NOT NULL,
    UpdatedAt           datetimeoffset   NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Customers_Identificacion')
    CREATE UNIQUE INDEX UX_Customers_Identificacion ON dbo.Customers (Identificacion);

-- 3) Facturas electrónicas
IF OBJECT_ID('dbo.ElectronicInvoices', 'U') IS NULL CREATE TABLE dbo.ElectronicInvoices (
    Id                          uniqueidentifier NOT NULL CONSTRAINT PK_ElectronicInvoices PRIMARY KEY,
    Ambiente                    tinyint          NOT NULL,   -- 1 pruebas, 2 producción
    Estab                       char(3)          NOT NULL,
    PtoEmi                      char(3)          NOT NULL,
    Secuencial                  int              NOT NULL,
    NumeroFactura               varchar(17)      NOT NULL,
    ClaveAcceso                 char(49)         NOT NULL,
    FechaEmision                date             NOT NULL,
    CustomerId                  uniqueidentifier NOT NULL CONSTRAINT FK_ElectronicInvoices_Customers REFERENCES dbo.Customers (Id),
    TipoIdentificacionComprador char(2)          NOT NULL,
    IdentificacionComprador     nvarchar(20)     NOT NULL,
    RazonSocialComprador        nvarchar(300)    NOT NULL,
    DireccionComprador          nvarchar(300)    NULL,
    EmailComprador              nvarchar(300)    NULL,
    TelefonoComprador           nvarchar(30)     NULL,
    TotalSinImpuestos           decimal(18,2)    NOT NULL,
    TotalDescuento              decimal(18,2)    NOT NULL,
    TotalIva                    decimal(18,2)    NOT NULL,
    ImporteTotal                decimal(18,2)    NOT NULL,
    Estado                      varchar(20)      NOT NULL,   -- FIRMADA, RECIBIDA, DEVUELTA, AUTORIZADO, NO AUTORIZADO, ERROR_ENVIO
    MensajesSri                 nvarchar(max)    NULL,
    NumeroAutorizacion          varchar(49)      NULL,
    FechaAutorizacion           datetimeoffset   NULL,
    XmlFirmado                  nvarchar(max)    NOT NULL,
    XmlAutorizado               nvarchar(max)    NULL,
    SaleInvoiceNumber           nvarchar(450)    NULL,
    EmailEnviado                bit              NOT NULL CONSTRAINT DF_ElectronicInvoices_EmailEnviado DEFAULT 0,
    CreatedAt                   datetimeoffset   NOT NULL,
    UpdatedAt                   datetimeoffset   NOT NULL,
    CreatedBy                   nvarchar(100)    NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_ElectronicInvoices_ClaveAcceso')
    CREATE UNIQUE INDEX UX_ElectronicInvoices_ClaveAcceso ON dbo.ElectronicInvoices (ClaveAcceso);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_ElectronicInvoices_Numero')
    CREATE UNIQUE INDEX UX_ElectronicInvoices_Numero ON dbo.ElectronicInvoices (Ambiente, Estab, PtoEmi, Secuencial);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ElectronicInvoices_FechaEmision')
    CREATE INDEX IX_ElectronicInvoices_FechaEmision ON dbo.ElectronicInvoices (FechaEmision DESC);

-- 4) Detalle de la factura
IF OBJECT_ID('dbo.ElectronicInvoiceItems', 'U') IS NULL CREATE TABLE dbo.ElectronicInvoiceItems (
    Id                     uniqueidentifier NOT NULL CONSTRAINT PK_ElectronicInvoiceItems PRIMARY KEY,
    ElectronicInvoiceId    uniqueidentifier NOT NULL CONSTRAINT FK_ElectronicInvoiceItems_Invoices
                               REFERENCES dbo.ElectronicInvoices (Id) ON DELETE CASCADE,
    Linea                  int              NOT NULL,
    ProductId              uniqueidentifier NULL,
    CodigoPrincipal        nvarchar(25)     NOT NULL,
    Descripcion            nvarchar(300)    NOT NULL,
    Cantidad               decimal(18,6)    NOT NULL,
    PrecioUnitario         decimal(18,6)    NOT NULL,
    Descuento              decimal(18,2)    NOT NULL,
    PrecioTotalSinImpuesto decimal(18,2)    NOT NULL,
    CodigoPorcentajeIva    varchar(4)       NOT NULL,
    TarifaIva              decimal(5,2)     NOT NULL,
    ValorIva               decimal(18,2)    NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ElectronicInvoiceItems_Invoice')
    CREATE INDEX IX_ElectronicInvoiceItems_Invoice ON dbo.ElectronicInvoiceItems (ElectronicInvoiceId);

-- 5) Formas de pago de la factura
IF OBJECT_ID('dbo.ElectronicInvoicePayments', 'U') IS NULL CREATE TABLE dbo.ElectronicInvoicePayments (
    Id                  uniqueidentifier NOT NULL CONSTRAINT PK_ElectronicInvoicePayments PRIMARY KEY,
    ElectronicInvoiceId uniqueidentifier NOT NULL CONSTRAINT FK_ElectronicInvoicePayments_Invoices
                            REFERENCES dbo.ElectronicInvoices (Id) ON DELETE CASCADE,
    FormaPago           char(2)          NOT NULL,   -- 01 efectivo, 20 transferencia
    Total               decimal(18,2)    NOT NULL,
    Plazo               int              NOT NULL,
    UnidadTiempo        varchar(10)      NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ElectronicInvoicePayments_Invoice')
    CREATE INDEX IX_ElectronicInvoicePayments_Invoice ON dbo.ElectronicInvoicePayments (ElectronicInvoiceId);

-- Verificación: deben aparecer las 5 tablas
SELECT name AS tabla_creada FROM sys.tables
WHERE name IN ('SriSettings', 'Customers', 'ElectronicInvoices', 'ElectronicInvoiceItems', 'ElectronicInvoicePayments')
ORDER BY name;
