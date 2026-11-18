USE [INF5130_TF02];
GO

SELECT N'Clientes' AS Tabla, COUNT_BIG(*) AS Filas FROM dbo.Clientes
UNION ALL SELECT N'TiposHabitacion', COUNT_BIG(*) FROM dbo.TiposHabitacion
UNION ALL SELECT N'Habitaciones', COUNT_BIG(*) FROM dbo.Habitaciones
UNION ALL SELECT N'Reservas', COUNT_BIG(*) FROM dbo.Reservas
UNION ALL SELECT N'Pagos', COUNT_BIG(*) FROM dbo.Pagos;

SELECT h.Numero, t.Nombre AS Tipo, t.TarifaBase, t.Estrategia, h.Activa
FROM dbo.Habitaciones AS h
JOIN dbo.TiposHabitacion AS t ON t.Id = h.TipoHabitacionId
ORDER BY h.Id;

SELECT r.Id, h.Numero AS Habitacion, c.Nombre AS Cliente, r.Entrada, r.Salida, r.Estado, r.Importe,
       p.Monto AS Pagado, p.Metodo, p.Fecha
FROM dbo.Reservas AS r
JOIN dbo.Habitaciones AS h ON h.Id = r.HabitacionId
JOIN dbo.Clientes AS c ON c.Id = r.ClienteId
LEFT JOIN dbo.Pagos AS p ON p.ReservaId = r.Id
ORDER BY r.Id;

-- Ambas consultas de integridad deben devolver cero filas.
SELECT r.Id AS ReservaFinalizadaSinPago
FROM dbo.Reservas AS r
WHERE r.Estado = 'Finalizada'
  AND NOT EXISTS (SELECT 1 FROM dbo.Pagos AS p WHERE p.ReservaId = r.Id);

SELECT a.HabitacionId, a.Id AS PrimeraReserva, b.Id AS SegundaReserva
FROM dbo.Reservas AS a
JOIN dbo.Reservas AS b ON b.HabitacionId = a.HabitacionId AND a.Id < b.Id
WHERE a.Estado IN ('Reservada', 'EnEstadia') AND b.Estado IN ('Reservada', 'EnEstadia')
  AND a.Entrada < b.Salida AND b.Entrada < a.Salida;
