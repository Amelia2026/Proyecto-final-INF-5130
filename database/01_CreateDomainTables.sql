USE [INF5130_TF02];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

CREATE TABLE dbo.Clientes (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
    Nombre nvarchar(120) NOT NULL,
    Email nvarchar(254) COLLATE Latin1_General_100_CI_AS_SC NOT NULL,
    Documento nvarchar(32) COLLATE Latin1_General_100_CI_AS_SC NOT NULL,
    CONSTRAINT UQ_Clientes_Email UNIQUE (Email),
    CONSTRAINT UQ_Clientes_Documento UNIQUE (Documento),
    CONSTRAINT CK_Clientes_Texto CHECK (LEN(TRIM(Nombre)) > 0 AND LEN(TRIM(Email)) > 0 AND LEN(TRIM(Documento)) > 0)
);

CREATE TABLE dbo.TiposHabitacion (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TiposHabitacion PRIMARY KEY,
    Nombre nvarchar(100) COLLATE Latin1_General_100_CI_AS_SC NOT NULL,
    TarifaBase decimal(10,2) NOT NULL,
    Estrategia varchar(12) COLLATE Latin1_General_100_BIN2 NOT NULL,
    CONSTRAINT UQ_TiposHabitacion_Nombre UNIQUE (Nombre),
    CONSTRAINT CK_TiposHabitacion_Nombre CHECK (LEN(TRIM(Nombre)) > 0),
    CONSTRAINT CK_TiposHabitacion_Tarifa CHECK (TarifaBase BETWEEN 0.01 AND 99999.99),
    CONSTRAINT CK_TiposHabitacion_Estrategia CHECK (Estrategia IN ('Plana', 'FinDeSemana'))
);

CREATE TABLE dbo.Habitaciones (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Habitaciones PRIMARY KEY,
    TipoHabitacionId int NOT NULL,
    Numero nvarchar(12) COLLATE Latin1_General_100_CI_AS_SC NOT NULL,
    Activa bit NOT NULL CONSTRAINT DF_Habitaciones_Activa DEFAULT (1),
    CONSTRAINT FK_Habitaciones_TiposHabitacion FOREIGN KEY (TipoHabitacionId) REFERENCES dbo.TiposHabitacion (Id),
    CONSTRAINT UQ_Habitaciones_Numero UNIQUE (Numero),
    CONSTRAINT CK_Habitaciones_Numero CHECK (LEN(TRIM(Numero)) > 0)
);

CREATE TABLE dbo.Reservas (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reservas PRIMARY KEY,
    HabitacionId int NOT NULL,
    ClienteId int NOT NULL,
    Entrada date NOT NULL,
    Salida date NOT NULL,
    Estado varchar(12) COLLATE Latin1_General_100_BIN2 NOT NULL CONSTRAINT DF_Reservas_Estado DEFAULT ('Reservada'),
    FechaCancelacion date NULL,
    FechaCheckIn date NULL,
    FechaCheckOut date NULL,
    Importe decimal(12,2) NOT NULL,
    CONSTRAINT FK_Reservas_Habitaciones FOREIGN KEY (HabitacionId) REFERENCES dbo.Habitaciones (Id),
    CONSTRAINT FK_Reservas_Clientes FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes (Id),
    CONSTRAINT UQ_Reservas_DatosPago UNIQUE (Id, Importe, FechaCheckOut),
    CONSTRAINT CK_Reservas_Periodo CHECK (DATEDIFF(day, Entrada, Salida) BETWEEN 1 AND 30),
    CONSTRAINT CK_Reservas_Importe CHECK (Importe BETWEEN 0.01 AND 3600000.00),
    CONSTRAINT CK_Reservas_EstadoYFechas CHECK (
        (Estado = 'Reservada' AND FechaCancelacion IS NULL AND FechaCheckIn IS NULL AND FechaCheckOut IS NULL) OR
        (Estado = 'Cancelada' AND FechaCancelacion IS NOT NULL AND FechaCheckIn IS NULL AND FechaCheckOut IS NULL) OR
        (Estado = 'EnEstadia' AND FechaCancelacion IS NULL AND FechaCheckIn IS NOT NULL
            AND FechaCheckIn >= Entrada AND FechaCheckIn < Salida AND FechaCheckOut IS NULL) OR
        (Estado = 'Finalizada' AND FechaCancelacion IS NULL AND FechaCheckIn IS NOT NULL
            AND FechaCheckIn >= Entrada AND FechaCheckIn < Salida
            AND FechaCheckOut IS NOT NULL AND FechaCheckOut >= FechaCheckIn)
    )
);

CREATE TABLE dbo.Pagos (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pagos PRIMARY KEY,
    ReservaId int NOT NULL,
    Monto decimal(12,2) NOT NULL,
    Metodo varchar(15) COLLATE Latin1_General_100_BIN2 NOT NULL,
    Fecha date NOT NULL,
    CONSTRAINT UQ_Pagos_ReservaId UNIQUE (ReservaId),
    CONSTRAINT FK_Pagos_Reservas FOREIGN KEY (ReservaId, Monto, Fecha) REFERENCES dbo.Reservas (Id, Importe, FechaCheckOut),
    CONSTRAINT CK_Pagos_Metodo CHECK (Metodo IN ('Efectivo', 'Tarjeta', 'Transferencia'))
);

CREATE INDEX IX_Habitaciones_TipoHabitacionId ON dbo.Habitaciones (TipoHabitacionId);
CREATE INDEX IX_Reservas_ClienteId ON dbo.Reservas (ClienteId);
CREATE INDEX IX_Reservas_HabitacionPeriodo ON dbo.Reservas (HabitacionId, Entrada, Salida) INCLUDE (Estado);
CREATE UNIQUE INDEX UX_Reservas_EstadiaActiva ON dbo.Reservas (HabitacionId) WHERE Estado = 'EnEstadia';

COMMIT TRANSACTION;
GO
