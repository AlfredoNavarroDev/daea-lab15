-- =============================================================
-- TattooStudio - Script de base de datos (SQL Server)
-- Variante para MonsterASP.NET: sin CREATE DATABASE ni USE, porque
-- el panel ya entrega la base creada (con su propio nombre) y no
-- permite esos comandos. Correr conectado directamente a esa base.
-- Contenido identico a database/TattooStudio.sql salvo ese recorte.
-- =============================================================

IF OBJECT_ID('dbo.CitaTatuaje', 'U') IS NOT NULL DROP TABLE dbo.CitaTatuaje;
IF OBJECT_ID('dbo.Tatuador', 'U') IS NOT NULL DROP TABLE dbo.Tatuador;
IF OBJECT_ID('dbo.Usuario', 'U') IS NOT NULL DROP TABLE dbo.Usuario;
GO

CREATE TABLE dbo.Tatuador (
    Id                INT IDENTITY(1,1) NOT NULL,
    Nombre            NVARCHAR(100)     NOT NULL,
    Especialidad      NVARCHAR(50)      NOT NULL,
    AniosExperiencia  INT               NOT NULL,
    Activo            BIT               NOT NULL CONSTRAINT DF_Tatuador_Activo DEFAULT (1),
    CONSTRAINT PK_Tatuador PRIMARY KEY (Id),
    CONSTRAINT CK_Tatuador_AniosExperiencia CHECK (AniosExperiencia BETWEEN 0 AND 60)
);
GO

CREATE TABLE dbo.CitaTatuaje (
    Id                INT IDENTITY(1,1) NOT NULL,
    ClienteNombre     NVARCHAR(100)     NOT NULL,
    ClienteCorreo     NVARCHAR(150)     NOT NULL,
    FechaHora         DATETIME2         NOT NULL,
    EstimacionHoras   DECIMAL(5,2)      NOT NULL,
    SeñaPagada        DECIMAL(10,2)     NOT NULL CONSTRAINT DF_CitaTatuaje_SeñaPagada DEFAULT (0),
    Estado            NVARCHAR(20)      NOT NULL CONSTRAINT DF_CitaTatuaje_Estado DEFAULT ('Pendiente'),
    TatuadorId        INT               NOT NULL,
    CONSTRAINT PK_CitaTatuaje PRIMARY KEY (Id),
    CONSTRAINT FK_CitaTatuaje_Tatuador FOREIGN KEY (TatuadorId) REFERENCES dbo.Tatuador (Id),
    CONSTRAINT CK_CitaTatuaje_Estado CHECK (Estado IN ('Pendiente', 'Confirmada', 'Cancelada')),
    CONSTRAINT CK_CitaTatuaje_EstimacionHoras CHECK (EstimacionHoras BETWEEN 0.5 AND 12),
    CONSTRAINT CK_CitaTatuaje_SeñaPagada CHECK (SeñaPagada BETWEEN 0 AND 100000)
);
GO

CREATE INDEX IX_CitaTatuaje_TatuadorId ON dbo.CitaTatuaje (TatuadorId);
GO

-- =============================================================
-- Usuario - cuentas de acceso a la API (autenticacion JWT, Semana 14)
-- ClaveHash se completa con BCrypt: registrar por la API (rol por
-- defecto "Recepcionista") y luego ajustar el rol con UPDATE.
-- =============================================================

CREATE TABLE dbo.Usuario (
    Id         INT IDENTITY(1,1) NOT NULL,
    Correo     NVARCHAR(150)     NOT NULL,
    ClaveHash  NVARCHAR(200)     NOT NULL,
    Rol        NVARCHAR(30)      NOT NULL,
    CONSTRAINT PK_Usuario PRIMARY KEY (Id),
    CONSTRAINT UQ_Usuario_Correo UNIQUE (Correo),
    CONSTRAINT CK_Usuario_Rol CHECK (Rol IN ('Recepcionista', 'Tatuador'))
);
GO

-- =============================================================
-- Datos de ejemplo
-- =============================================================

INSERT INTO dbo.Tatuador (Nombre, Especialidad, AniosExperiencia, Activo) VALUES
    ('Mariana Rojas',   'Realismo',     9, 1),
    ('Diego Fernandez', 'Tradicional',  6, 1),
    ('Lucia Paredes',   'Fine Line',    3, 1),
    ('Hector Salas',    'Tradicional', 12, 0);
GO

INSERT INTO dbo.CitaTatuaje (ClienteNombre, ClienteCorreo, FechaHora, EstimacionHoras, SeñaPagada, Estado, TatuadorId) VALUES
    ('Carla Munoz',     'carla.munoz@mail.com',    '2026-11-20T10:00:00', 3.0, 50.00,  'Confirmada', 1),
    ('Jorge Delgado',   'jorge.delgado@mail.com',  '2026-11-20T15:00:00', 2.5, 40.00,  'Pendiente',  1),
    ('Valeria Nunez',   'valeria.nunez@mail.com',  '2026-11-21T09:00:00', 4.0, 60.00,  'Confirmada', 2),
    ('Pedro Castillo',  'pedro.castillo@mail.com', '2026-11-21T14:30:00', 1.5, 30.00,  'Pendiente',  2),
    ('Andrea Vidal',    'andrea.vidal@mail.com',   '2026-11-22T11:00:00', 1.0, 25.00,  'Confirmada', 3),
    ('Ruben Ortega',    'ruben.ortega@mail.com',   '2026-11-18T16:00:00', 2.0, 35.00,  'Cancelada',  3),
    ('Sofia Carrasco',  'sofia.carrasco@mail.com', '2026-11-23T12:00:00', 5.0, 80.00,  'Pendiente',  1);
GO

-- Un usuario por rol para las pruebas de la Semana 14. El hash se obtiene
-- registrando cada correo por POST api/auth/registro (clave de prueba,
-- p.ej. "Clave123") y luego corriendo el UPDATE de abajo para fijar el rol.
-- recepcion@tattoostudio.com    -> rol por defecto al registrarse: Recepcionista (sin UPDATE)
-- artista@tattoostudio.com      -> UPDATE dbo.Usuario SET Rol = 'Tatuador' WHERE Correo = 'artista@tattoostudio.com';
GO
