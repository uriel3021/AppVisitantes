-- Script para optimizar índices en Azure SQL Database
-- Ejecutar en Azure Data Studio o SQL Server Management Studio

-- 1. Verificar el estado actual de los índices
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_primary_key,
    i.is_unique,
    ic.column_id,
    c.name AS ColumnName,
    c.system_type_id,
    c.max_length
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE t.name IN ('CodigoQR', 'Visitantes')
ORDER BY t.name, i.name, ic.key_ordinal;

-- 2. Verificar fragmentación de índices
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.index_type_desc,
    ips.avg_fragmentation_in_percent,
    ips.page_count
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'DETAILED') ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE OBJECT_NAME(ips.object_id) IN ('CodigoQR', 'Visitantes')
    AND ips.avg_fragmentation_in_percent > 10
ORDER BY ips.avg_fragmentation_in_percent DESC;

-- 3. OPCIÓN A: Si la fragmentación es alta (>30%), optimizar el PK existente
-- Cambiar el DEFAULT del GUID para que sea secuencial en lugar de aleatorio
ALTER TABLE dbo.CodigoQR 
ADD CONSTRAINT DF_CodigoQR_Id DEFAULT NEWSEQUENTIALID() FOR Id;

-- Reconstruir el índice clustered con FILLFACTOR 90
ALTER INDEX PK__CodigoQR__3214EC0799C3E79B ON dbo.CodigoQR 
REBUILD WITH (FILLFACTOR = 90, ONLINE = ON);

-- 4. OPCIÓN B (MÁS AGRESIVA): Cambiar a INT IDENTITY como clustered
-- SOLO SI TIENES MUCHOS PROBLEMAS DE RENDIMIENTO

-- Paso 1: Agregar columna INT IDENTITY
-- ALTER TABLE dbo.CodigoQR ADD IdInt INT IDENTITY(1,1);

-- Paso 2: Eliminar PK actual y crear nuevo PK en INT
-- ALTER TABLE dbo.CodigoQR DROP CONSTRAINT PK__CodigoQR__3214EC0799C3E79B;
-- ALTER TABLE dbo.CodigoQR ADD CONSTRAINT PK_CodigoQR_IdInt PRIMARY KEY CLUSTERED (IdInt);

-- Paso 3: Crear índice único en el GUID
-- CREATE UNIQUE NONCLUSTERED INDEX UX_CodigoQR_Id ON dbo.CodigoQR(Id);

-- 5. Crear índices de cobertura para las consultas más comunes
-- Índice para búsquedas por GUID con inclusión de VisitanteId
CREATE NONCLUSTERED INDEX IX_CodigoQR_Id_Include_VisitanteId 
ON dbo.CodigoQR (Id) 
INCLUDE (VisitanteId, Codigo)
WITH (ONLINE = ON, FILLFACTOR = 90);

-- Índice para búsquedas por Codigo (si usamos esa columna)
CREATE NONCLUSTERED INDEX IX_CodigoQR_Codigo 
ON dbo.CodigoQR (Codigo) 
INCLUDE (Id, VisitanteId)
WITH (ONLINE = ON, FILLFACTOR = 90);

-- 6. Para la tabla Visitantes, crear índice para búsquedas frecuentes
CREATE NONCLUSTERED INDEX IX_Visitantes_Id_Include_BasicInfo 
ON dbo.Visitantes (Id) 
INCLUDE (Nombre, ApellidoPaterno, ApellidoMaterno, CorreoElectronico, Telefono, FechaVisita)
WITH (ONLINE = ON, FILLFACTOR = 90);

-- 7. Actualizar estadísticas
UPDATE STATISTICS dbo.CodigoQR WITH FULLSCAN;
UPDATE STATISTICS dbo.Visitantes WITH FULLSCAN;

-- 8. Verificar planes de ejecución después de los cambios
-- Ejecutar una consulta típica y revisar el plan:
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

DECLARE @TestGuid UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.CodigoQR);

SELECT cqr.Id, cqr.Codigo, cqr.VisitanteId,
       v.Nombre, v.ApellidoPaterno, v.ApellidoMaterno, 
       v.CorreoElectronico, v.Telefono, v.FechaVisita
FROM dbo.CodigoQR cqr
INNER JOIN dbo.Visitantes v ON cqr.VisitanteId = v.Id
WHERE cqr.Id = @TestGuid;

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;

-- 9. Script de mantenimiento para ejecutar periódicamente
-- (Crear como Stored Procedure o Automation Runbook en Azure)
/*
-- Reorganizar índices con fragmentación media (10-30%)
ALTER INDEX ALL ON dbo.CodigoQR REORGANIZE;
ALTER INDEX ALL ON dbo.Visitantes REORGANIZE;

-- Reconstruir índices con fragmentación alta (>30%)
-- ALTER INDEX ALL ON dbo.CodigoQR REBUILD WITH (ONLINE = ON, FILLFACTOR = 90);
-- ALTER INDEX ALL ON dbo.Visitantes REBUILD WITH (ONLINE = ON, FILLFACTOR = 90);

-- Actualizar estadísticas
UPDATE STATISTICS dbo.CodigoQR WITH FULLSCAN;
UPDATE STATISTICS dbo.Visitantes WITH FULLSCAN;
*/
