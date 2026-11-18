-- Requiere permiso para crear bases en una instancia de desarrollo.
USE [master];
GO
IF DB_ID(N'INF5130_TF02') IS NULL
    EXEC(N'CREATE DATABASE [INF5130_TF02]');
GO
