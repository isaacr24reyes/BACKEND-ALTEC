-- ============================================================================
-- ALTEC - Una sola importación: Yanni + Cámaras
-- Fecha: 2026-11-16
--
-- Las importaciones "Yanni" y "Cámaras" llegaron en un mismo envío. Sus costos extras
-- (Yanni $540.12 + Cámaras $399.11 = $939.23) se habían cargado como si fueran de envíos distintos.
--
-- Este script reparte el total de $939.23 entre las DOS importaciones según el valor de
-- sus productos en China. Así cada producto absorbe el mismo % de recargo, igual que si
-- fueran un solo registro.
--
-- No mueve ni borra productos, archivos ni fotos: solo cambia CostosExtras y
-- CostosExtrasNota de esos dos registros. Los valores anteriores quedan escritos en la nota.
--
-- Seguro de repetir: solo actúa si encuentra exactamente la importación de Yanni con $540.12
-- y la de Cámaras con $399.11. Una vez aplicado ya no los encuentra y no hace nada.
-- ============================================================================

-- 1) Antes: los dos registros que se van a ajustar
SELECT q.Nombre, q.Fecha, q.CostosExtras,
       (SELECT SUM(i.Total) FROM dbo.ImportedQuotationItems i WHERE i.ImportedQuotationId = q.Id) AS ValorChina
FROM dbo.ImportedQuotations q
WHERE (q.Nombre LIKE '%yanni%' AND q.CostosExtras = 540.12)
   OR (q.Nombre LIKE '%mara%' AND q.CostosExtras = 399.11);

-- 2) Reparto del total según el valor de cada importación
;WITH q AS (
    SELECT iq.Id, iq.CostosExtras,
           (SELECT SUM(i.Total) FROM dbo.ImportedQuotationItems i WHERE i.ImportedQuotationId = iq.Id) AS Valor
    FROM dbo.ImportedQuotations iq
    WHERE (iq.Nombre LIKE '%yanni%' AND iq.CostosExtras = 540.12)
       OR (iq.Nombre LIKE '%mara%' AND iq.CostosExtras = 399.11)
),
chk AS (
    SELECT COUNT(*) AS n,
           SUM(CASE WHEN CostosExtras = 540.12 THEN 1 ELSE 0 END) AS nYanni,
           SUM(CASE WHEN CostosExtras = 399.11 THEN 1 ELSE 0 END) AS nCamaras,
           SUM(Valor) AS ValorTotal
    FROM q
)
UPDATE iq
SET iq.CostosExtras = ROUND(939.23 * q.Valor / chk.ValorTotal, 4),
    iq.CostosExtrasNota = LEFT(CONCAT(
        'Envío conjunto Yanni + Cámaras: $939.23 repartido por valor. ',
        'Antes: $', CONVERT(varchar(20), q.CostosExtras), '. ',
        ISNULL(iq.CostosExtrasNota, '')), 300)
FROM dbo.ImportedQuotations iq
JOIN q ON q.Id = iq.Id
CROSS JOIN chk
WHERE chk.n = 2 AND chk.nYanni = 1 AND chk.nCamaras = 1 AND chk.ValorTotal > 0;

-- 3) Después: el total repartido debe sumar 939.23 y el % de recargo ser el mismo en ambas
SELECT q.Nombre, q.CostosExtras,
       v.ValorChina,
       CAST(q.CostosExtras / NULLIF(v.ValorChina, 0) * 100 AS decimal(9,2)) AS PorcentajeRecargo,
       q.CostosExtrasNota
FROM dbo.ImportedQuotations q
CROSS APPLY (SELECT SUM(i.Total) AS ValorChina FROM dbo.ImportedQuotationItems i WHERE i.ImportedQuotationId = q.Id) v
WHERE q.CostosExtrasNota LIKE 'Envío conjunto Yanni + Cámaras%';
