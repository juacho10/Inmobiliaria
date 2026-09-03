-- Crear la base de datos si no existe
CREATE DATABASE IF NOT EXISTS inmobiliaria_db;
USE inmobiliaria_db;

-- ============================================
-- TABLA: Usuarios
-- ============================================
CREATE TABLE IF NOT EXISTS Usuarios (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    Avatar VARCHAR(255) NULL,
    Rol VARCHAR(20) NOT NULL DEFAULT 'Empleado',
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NULL,
    Activo BOOLEAN NOT NULL DEFAULT TRUE
);

-- ============================================
-- TABLA: Propietarios
-- ============================================
CREATE TABLE IF NOT EXISTS Propietarios (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Dni VARCHAR(10) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Telefono VARCHAR(20) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NULL,
    Activo BOOLEAN NOT NULL DEFAULT TRUE
);

-- ============================================
-- TABLA: Inquilinos
-- ============================================
CREATE TABLE IF NOT EXISTS Inquilinos (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Dni VARCHAR(10) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Telefono VARCHAR(20) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NULL,
    Activo BOOLEAN NOT NULL DEFAULT TRUE
);

-- ============================================
-- TABLA: Inmuebles
-- ============================================
CREATE TABLE IF NOT EXISTS Inmuebles (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Direccion VARCHAR(200) NOT NULL,
    PropietarioId INT NOT NULL,
    Tipo VARCHAR(50) NOT NULL,
    Uso VARCHAR(20) NOT NULL,
    Ambientes INT NOT NULL,
    Precio DECIMAL(18,2) NOT NULL,
    Coordenadas VARCHAR(100) NULL,
    Disponible BOOLEAN NOT NULL DEFAULT TRUE,
    Activo BOOLEAN NOT NULL DEFAULT TRUE,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NULL,
    FOREIGN KEY (PropietarioId) REFERENCES Propietarios(Id) ON DELETE RESTRICT
);

-- ============================================
-- TABLA: Contratos
-- ============================================
CREATE TABLE IF NOT EXISTS Contratos (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    InmuebleId INT NOT NULL,
    InquilinoId INT NOT NULL,
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    Monto DECIMAL(18,2) NOT NULL,
    Vigente BOOLEAN NOT NULL DEFAULT TRUE,
    FechaTerminacionAnticipada DATE NULL,
    Multa DECIMAL(18,2) NULL,
    UsuarioCreacionId INT NULL,
    UsuarioModificacionId INT NULL,
    UsuarioTerminacionId INT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NULL,
    FOREIGN KEY (InmuebleId) REFERENCES Inmuebles(Id) ON DELETE RESTRICT,
    FOREIGN KEY (InquilinoId) REFERENCES Inquilinos(Id) ON DELETE RESTRICT,
    FOREIGN KEY (UsuarioCreacionId) REFERENCES Usuarios(Id) ON DELETE SET NULL,
    FOREIGN KEY (UsuarioModificacionId) REFERENCES Usuarios(Id) ON DELETE SET NULL,
    FOREIGN KEY (UsuarioTerminacionId) REFERENCES Usuarios(Id) ON DELETE SET NULL,
    CHECK (FechaFin > FechaInicio)
);

-- ============================================
-- TABLA: Pagos
-- ============================================
CREATE TABLE IF NOT EXISTS Pagos (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    ContratoId INT NOT NULL,
    NumeroPago INT NOT NULL,
    FechaPago DATE NOT NULL,
    Concepto VARCHAR(100) NOT NULL,
    Importe DECIMAL(18,2) NOT NULL,
    Anulado BOOLEAN NOT NULL DEFAULT FALSE,
    UsuarioCreacionId INT NULL,
    UsuarioAnulacionId INT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaAnulacion DATETIME NULL,
    FOREIGN KEY (ContratoId) REFERENCES Contratos(Id) ON DELETE RESTRICT,
    FOREIGN KEY (UsuarioCreacionId) REFERENCES Usuarios(Id) ON DELETE SET NULL,
    FOREIGN KEY (UsuarioAnulacionId) REFERENCES Usuarios(Id) ON DELETE SET NULL
);

-- ============================================
-- ÍNDICES
-- ============================================
CREATE INDEX IX_Inmuebles_PropietarioId ON Inmuebles(PropietarioId);
CREATE INDEX IX_Inmuebles_Disponible ON Inmuebles(Disponible);
CREATE INDEX IX_Contratos_InmuebleId ON Contratos(InmuebleId);
CREATE INDEX IX_Contratos_InquilinoId ON Contratos(InquilinoId);
CREATE INDEX IX_Contratos_Vigente ON Contratos(Vigente);
CREATE INDEX IX_Contratos_Fechas ON Contratos(FechaInicio, FechaFin);
CREATE INDEX IX_Pagos_ContratoId ON Pagos(ContratoId);

-- ============================================
-- DATOS DE PRUEBA
-- ============================================

-- Usuario Administrador (contraseña: admin123)
INSERT INTO Usuarios (Nombre, Apellido, Email, Password, Rol) 
SELECT 'Admin', 'Sistema', 'admin@inmobiliaria.com', '$2a$11$rLk5fmI0yU.6lZb6JQqQJ.TUq6Q9Q6Q6Q6Q6Q6Q6Q6Q6Q6Q6Q6Q6Q6', 'Administrador'
WHERE NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'admin@inmobiliaria.com');

-- Propietarios
INSERT INTO Propietarios (Dni, Nombre, Apellido, Telefono, Email) 
SELECT '30123456', 'Juan', 'Pérez', '1151234567', 'juan.perez@email.com'
WHERE NOT EXISTS (SELECT 1 FROM Propietarios WHERE Dni = '30123456');

INSERT INTO Propietarios (Dni, Nombre, Apellido, Telefono, Email) 
SELECT '30234567', 'María', 'Gómez', '1152345678', 'maria.gomez@email.com'
WHERE NOT EXISTS (SELECT 1 FROM Propietarios WHERE Dni = '30234567');

-- Inquilinos
INSERT INTO Inquilinos (Dni, Nombre, Apellido, Telefono, Email) 
SELECT '40123456', 'Carlos', 'López', '1153456789', 'carlos.lopez@email.com'
WHERE NOT EXISTS (SELECT 1 FROM Inquilinos WHERE Dni = '40123456');

INSERT INTO Inquilinos (Dni, Nombre, Apellido, Telefono, Email) 
SELECT '40234567', 'Ana', 'Martínez', '1154567890', 'ana.martinez@email.com'
WHERE NOT EXISTS (SELECT 1 FROM Inquilinos WHERE Dni = '40234567');

-- Inmuebles (necesitamos IDs de propietarios)
INSERT INTO Inmuebles (Direccion, PropietarioId, Tipo, Uso, Ambientes, Precio, Coordenadas, Disponible)
SELECT 'Av. Corrientes 1234', p.Id, 'Departamento', 'Comercial', 2, 50000.00, '-34.6037, -58.3816', TRUE
FROM Propietarios p WHERE p.Dni = '30123456'
AND NOT EXISTS (SELECT 1 FROM Inmuebles WHERE Direccion = 'Av. Corrientes 1234');

INSERT INTO Inmuebles (Direccion, PropietarioId, Tipo, Uso, Ambientes, Precio, Coordenadas, Disponible)
SELECT 'Calle Florida 567', p.Id, 'Local', 'Comercial', 1, 75000.00, '-34.6085, -58.3735', TRUE
FROM Propietarios p WHERE p.Dni = '30234567'
AND NOT EXISTS (SELECT 1 FROM Inmuebles WHERE Direccion = 'Calle Florida 567');

INSERT INTO Inmuebles (Direccion, PropietarioId, Tipo, Uso, Ambientes, Precio, Coordenadas, Disponible)
SELECT 'Av. Santa Fe 2345', p.Id, 'Departamento', 'Residencial', 3, 45000.00, '-34.5950, -58.4020', TRUE
FROM Propietarios p WHERE p.Dni = '30123456'
AND NOT EXISTS (SELECT 1 FROM Inmuebles WHERE Direccion = 'Av. Santa Fe 2345');

-- Contratos
INSERT INTO Contratos (InmuebleId, InquilinoId, FechaInicio, FechaFin, Monto, Vigente, UsuarioCreacionId)
SELECT i.Id, inq.Id, '2024-01-01', '2024-12-31', 50000.00, TRUE, u.Id
FROM Inmuebles i 
JOIN Inquilinos inq ON inq.Dni = '40123456'
JOIN Usuarios u ON u.Email = 'admin@inmobiliaria.com'
WHERE i.Direccion = 'Av. Corrientes 1234'
AND NOT EXISTS (SELECT 1 FROM Contratos WHERE InmuebleId = i.Id AND InquilinoId = inq.Id);

INSERT INTO Contratos (InmuebleId, InquilinoId, FechaInicio, FechaFin, Monto, Vigente, UsuarioCreacionId)
SELECT i.Id, inq.Id, '2024-02-01', '2024-11-30', 75000.00, TRUE, u.Id
FROM Inmuebles i 
JOIN Inquilinos inq ON inq.Dni = '40234567'
JOIN Usuarios u ON u.Email = 'admin@inmobiliaria.com'
WHERE i.Direccion = 'Calle Florida 567'
AND NOT EXISTS (SELECT 1 FROM Contratos WHERE InmuebleId = i.Id AND InquilinoId = inq.Id);

-- Pagos
INSERT INTO Pagos (ContratoId, NumeroPago, FechaPago, Concepto, Importe, UsuarioCreacionId)
SELECT c.Id, 1, '2024-01-05', 'Mes de Enero 2024', 50000.00, u.Id
FROM Contratos c
JOIN Usuarios u ON u.Email = 'admin@inmobiliaria.com'
WHERE c.Monto = 50000.00
AND NOT EXISTS (SELECT 1 FROM Pagos WHERE ContratoId = c.Id AND NumeroPago = 1);