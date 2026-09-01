-- Crear la base de datos si no existe
SELECT 'CREATE DATABASE inmobiliaria_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'inmobiliaria_db')\gexec

-- Conectar a la base de datos
\c inmobiliaria_db;

-- CREAR TABLAS
CREATE TABLE IF NOT EXISTS Usuarios (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    Avatar VARCHAR(255) NULL,
    Rol VARCHAR(20) NOT NULL DEFAULT 'Empleado',
    FechaCreacion TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion TIMESTAMP WITHOUT TIME ZONE NULL,
    Activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS Propietarios (
    Id SERIAL PRIMARY KEY,
    Dni VARCHAR(10) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Telefono VARCHAR(20) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    FechaCreacion TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion TIMESTAMP WITHOUT TIME ZONE NULL,
    Activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS Inquilinos (
    Id SERIAL PRIMARY KEY,
    Dni VARCHAR(10) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Telefono VARCHAR(20) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    FechaCreacion TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion TIMESTAMP WITHOUT TIME ZONE NULL,
    Activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE Inmuebles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Direccion NVARCHAR(200) NOT NULL,
    PropietarioId INT NOT NULL,
    Tipo NVARCHAR(50) NOT NULL,
    Uso NVARCHAR(20) NOT NULL CHECK (Uso IN ('Residencial', 'Comercial')),
    Ambientes INT NOT NULL,
    Precio DECIMAL(18,2) NOT NULL,
    Coordenadas NVARCHAR(100) NULL,
    Disponible BIT NOT NULL DEFAULT 1,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    FOREIGN KEY (PropietarioId) REFERENCES Propietarios(Id)
);

-- Tabla de Contratos
CREATE TABLE Contratos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    InmuebleId INT NOT NULL,
    InquilinoId INT NOT NULL,
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    Monto DECIMAL(18,2) NOT NULL,
    Vigente BIT NOT NULL DEFAULT 1,
    FechaTerminacionAnticipada DATE NULL,
    Multa DECIMAL(18,2) NULL,
    UsuarioCreacionId INT NULL,
    UsuarioModificacionId INT NULL,
    UsuarioTerminacionId INT NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    FOREIGN KEY (InmuebleId) REFERENCES Inmuebles(Id),
    FOREIGN KEY (InquilinoId) REFERENCES Inquilinos(Id),
    FOREIGN KEY (UsuarioCreacionId) REFERENCES Usuarios(Id),
    FOREIGN KEY (UsuarioModificacionId) REFERENCES Usuarios(Id),
    FOREIGN KEY (UsuarioTerminacionId) REFERENCES Usuarios(Id),
    CHECK (FechaFin > FechaInicio)
);

-- Tabla de Pagos
CREATE TABLE Pagos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ContratoId INT NOT NULL,
    NumeroPago INT NOT NULL,
    FechaPago DATE NOT NULL,
    Concepto NVARCHAR(100) NOT NULL,
    Importe DECIMAL(18,2) NOT NULL,
    Anulado BIT NOT NULL DEFAULT 0,
    UsuarioCreacionId INT NULL,
    UsuarioAnulacionId INT NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaAnulacion DATETIME2 NULL,
    FOREIGN KEY (ContratoId) REFERENCES Contratos(Id),
    FOREIGN KEY (UsuarioCreacionId) REFERENCES Usuarios(Id),
    FOREIGN KEY (UsuarioAnulacionId) REFERENCES Usuarios(Id)
);

-- Índices para mejor performance
CREATE INDEX IX_Inmuebles_PropietarioId ON Inmuebles(PropietarioId);
CREATE INDEX IX_Inmuebles_Disponible ON Inmuebles(Disponible);
CREATE INDEX IX_Contratos_InmuebleId ON Contratos(InmuebleId);
CREATE INDEX IX_Contratos_InquilinoId ON Contratos(InquilinoId);
CREATE INDEX IX_Contratos_Vigente ON Contratos(Vigente);
CREATE INDEX IX_Contratos_Fechas ON Contratos(FechaInicio, FechaFin);
CREATE INDEX IX_Pagos_ContratoId ON Pagos(ContratoId);
-- INSERTAR USUARIO ADMINISTRADOR POR DEFECTO
INSERT INTO Usuarios (Nombre, Apellido, Email, Password, Rol)
SELECT 'Admin', 'Sistema', 'admin@inmobiliaria.com', '$2a$11$rLk5fmI0yU.6lZb6JQqQJ.TUq6Q9Q6Q6Q6Q6Q6Q6Q6Q6Q6Q6Q6Q6Q6', 'Administrador'
WHERE NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'admin@inmobiliaria.com');

-- DATOS DE EJEMPLO
INSERT INTO Propietarios (Dni, Nombre, Apellido, Telefono, Email) VALUES
('30123456', 'Juan', 'Pérez', '1151234567', 'juan.perez@email.com'),
('30234567', 'María', 'Gómez', '1152345678', 'maria.gomez@email.com')
ON CONFLICT (Dni) DO NOTHING;

INSERT INTO Inquilinos (Dni, Nombre, Apellido, Telefono, Email) VALUES
('40123456', 'Carlos', 'López', '1153456789', 'carlos.lopez@email.com'),
('40234567', 'Ana', 'Martínez', '1154567890', 'ana.martinez@email.com')
ON CONFLICT (Dni) DO NOTHING;

-- Datos de ejemplo para Inmuebles
INSERT INTO Inmuebles (Direccion, PropietarioId, Tipo, Uso, Ambientes, Precio, Coordenadas, Disponible) VALUES
('Av. Corrientes 1234', 1, 'Departamento', 'Comercial', 2, 50000.00, '-34.6037, -58.3816', 1),
('Calle Florida 567', 2, 'Local', 'Comercial', 1, 75000.00, '-34.6085, -58.3735', 1),
('Av. Santa Fe 2345', 3, 'Departamento', 'Residencial', 3, 45000.00, '-34.5950, -58.4020', 1),
('Calle Lavalle 789', 1, 'Oficina', 'Comercial', 2, 60000.00, '-34.6012, -58.3781', 0);

-- Datos de ejemplo para Contratos
INSERT INTO Contratos (InmuebleId, InquilinoId, FechaInicio, FechaFin, Monto, Vigente, UsuarioCreacionId) VALUES
(1, 1, '2024-01-01', '2024-12-31', 50000.00, 1, 1),
(2, 2, '2024-02-01', '2024-11-30', 75000.00, 1, 1),
(3, 3, '2024-03-01', '2024-10-31', 45000.00, 1, 1);

-- Datos de ejemplo para Pagos
INSERT INTO Pagos (ContratoId, NumeroPago, FechaPago, Concepto, Importe, UsuarioCreacionId) VALUES
(1, 1, '2024-01-05', 'Mes de Enero 2024', 50000.00, 1),
(1, 2, '2024-02-05', 'Mes de Febrero 2024', 50000.00, 1),
(2, 1, '2024-02-05', 'Mes de Febrero 2024', 75000.00, 1),
(3, 1, '2024-03-05', 'Mes de Marzo 2024', 45000.00, 1);