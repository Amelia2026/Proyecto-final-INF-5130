USE [INF5130_TF02];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

INSERT dbo.Clientes (Nombre, Email, Documento) VALUES
    (N'María Reyes', N'maria@example.invalid', N'DOC-DEMO-01'),
    (N'Luis Peña', N'luis@example.invalid', N'DOC-DEMO-02'),
    (N'Sofía Díaz', N'sofia@example.invalid', N'DOC-DEMO-03');

INSERT dbo.TiposHabitacion (Nombre, TarifaBase, Estrategia)
VALUES (N'Sencilla', 2500.00, 'Plana'), (N'Doble', 3800.00, 'FinDeSemana');

DECLARE @Sencilla int = (SELECT Id FROM dbo.TiposHabitacion WHERE Nombre = N'Sencilla');
DECLARE @Doble int = (SELECT Id FROM dbo.TiposHabitacion WHERE Nombre = N'Doble');
INSERT dbo.Habitaciones (TipoHabitacionId, Numero, Activa)
VALUES (@Sencilla, N'101', 1), (@Sencilla, N'102', 1), (@Doble, N'201', 1), (@Doble, N'202', 0);

DECLARE @Hoy date = CONVERT(date, SYSDATETIMEOFFSET() AT TIME ZONE 'SA Western Standard Time');
DECLARE @Maria int = (SELECT Id FROM dbo.Clientes WHERE Documento = N'DOC-DEMO-01');
DECLARE @Luis int = (SELECT Id FROM dbo.Clientes WHERE Documento = N'DOC-DEMO-02');
DECLARE @Sofia int = (SELECT Id FROM dbo.Clientes WHERE Documento = N'DOC-DEMO-03');
DECLARE @Hab101 int = (SELECT Id FROM dbo.Habitaciones WHERE Numero = N'101');
DECLARE @Hab102 int = (SELECT Id FROM dbo.Habitaciones WHERE Numero = N'102');
DECLARE @Hab201 int = (SELECT Id FROM dbo.Habitaciones WHERE Numero = N'201');
DECLARE @ImporteDoble decimal(12,2) = (
    SELECT SUM(CASE WHEN DATEDIFF(day, CONVERT(date, '19000101', 112), Fecha) % 7 IN (4, 5)
                    THEN 4560.00 ELSE 3800.00 END)
    FROM (VALUES (DATEADD(day,-1,@Hoy)), (@Hoy)) AS Noches(Fecha)
);

INSERT dbo.Reservas (HabitacionId, ClienteId, Entrada, Salida, Estado, FechaCheckIn, FechaCheckOut, Importe)
VALUES (@Hab102, @Maria, DATEADD(day,-4,@Hoy), DATEADD(day,-2,@Hoy), 'Finalizada',
    DATEADD(day,-4,@Hoy), DATEADD(day,-2,@Hoy), 5000.00);
DECLARE @ReservaPagada int = CONVERT(int, SCOPE_IDENTITY());
INSERT dbo.Pagos (ReservaId, Monto, Metodo, Fecha)
VALUES (@ReservaPagada, 5000.00, 'Efectivo', DATEADD(day,-2,@Hoy));

INSERT dbo.Reservas (HabitacionId, ClienteId, Entrada, Salida, Estado, FechaCheckIn, Importe) VALUES
    (@Hab101, @Luis, DATEADD(day,1,@Hoy), DATEADD(day,3,@Hoy), 'Reservada', NULL, 5000.00),
    (@Hab201, @Sofia, DATEADD(day,-1,@Hoy), DATEADD(day,1,@Hoy), 'EnEstadia', DATEADD(day,-1,@Hoy), @ImporteDoble);

COMMIT TRANSACTION;
GO
