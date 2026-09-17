# 🏠 SISTEMA DE GESTIÓN INMOBILIARIA

Aplicación web para la gestión integral de propietarios, inquilinos, inmuebles, contratos y pagos.

---

## 👥 INTEGRANTES DEL GRUPO

| N° | Nombre |
|----|--------|
| 1 | [Molina Juan Ramon]


---

## 📊 DIAGRAMA ENTIDAD-RELACIÓN (DER)

### Modelo de Datos

┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ Propietario │1───*│  Inmueble   │1───*│  Contrato   │*───1│  Inquilino  │
├─────────────┤     ├─────────────┤     ├─────────────┤     ├─────────────┤
│ Id (PK)     │     │ Id (PK)     │     │ Id (PK)     │     │ Id (PK)     │
│ Dni         │     │ Direccion   │     │ FechaInicio │     │ Dni         │
│ Nombre      │     │ Uso         │     │ FechaFin    │     │ Nombre      │
│ Apellido    │     │ Tipo        │     │ Monto       │     │ Apellido    │
│ Telefono    │     │ Ambientes   │     │ Vigente     │     │ Telefono    │
│ Email       │     │ Precio      │     │ InmuebleId  │     │ Email       │
└─────────────┘     │ Disponible  │     │ InquilinoId │     └─────────────┘
                    │ PropietarioId│     └─────────────┘
                    └─────────────┘            │
                                               │ *───1
                                       ┌─────────────┐
                                       │    Pago     │
                                       ├─────────────┤
                                       │ Id (PK)     │
                                       │ NumeroPago  │
                                       │ FechaPago   │
                                       │ Importe     │
                                       │ Concepto    │
                                       │ Anulado     │
                                       │ ContratoId  │
                                       └─────────────┘

                                       ┌─────────────┐
                                       │   Usuario   │
                                       ├─────────────┤
                                       │ Id (PK)     │
                                       │ Nombre      │
                                       │ Apellido    │
                                       │ Email       │
                                       │ Password    │
                                       │ Rol         │
                                       └─────────────┘

Relaciones 

Relación	      Cardinalidad	        Descripción
Propietario      → Inmueble	 1 : N	Un propietario tiene varios inmuebles
Inmueble         → Contrato	 1 : N	Un inmueble tiene varios contratos
Contrato         → Inquilino N : 1	Un contrato pertenece a un inquilino
Contrato         → Pago	     1 : N	Un contrato tiene varios pagos
Usuario          → Contrato	 1 : N	Un usuario crea/modifica contratos
Usuario          → Pago	     1 : N	Un usuario registra/anula pagos

📐 DIAGRAMA DE CLASES
Modelo de Clases (C#)

┌─────────────────────────────────────────────────────────────┐
│                       BaseEntity                           │
├─────────────────────────────────────────────────────────────┤
│ + Id: int                                                  │
│ + FechaCreacion: DateTime                                  │
│ + FechaModificacion: DateTime?                             │
│ + Activo: bool                                             │
└─────────────────────────────────────────────────────────────┘
                           ▲
                           │
        ┌──────────────────┼──────────────────────────────────┐
        │                  │                                  │
        ▼                  ▼                                  ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────────────────┐
│  Propietario  │  │   Inquilino   │  │        Inmueble           │
├───────────────┤  ├───────────────┤  ├───────────────────────────┤
│ + Dni: string │  │ + Dni: string │  │ + Direccion: string       │
│ + Nombre: str │  │ + Nombre: str │  │ + PropietarioId: int      │
│ + Apellido:str│  │ + Apellido:str│  │ + Tipo: string            │
│ + Telefono:str│  │ + Telefono:str│  │ + Uso: string             │
│ + Email: str  │  │ + Email: str  │  │ + Ambientes: int          │
├───────────────┤  ├───────────────┤  │ + Precio: decimal         │
│ + Inmuebles   │  │ + Contratos   │  │ + Coordenadas: string?    │
└───────────────┘  └───────────────┘  │ + Disponible: bool        │
                                      ├───────────────────────────┤
                                      │ + Propietario             │
                                      │ + Contratos               │
                                      └───────────────────────────┘

┌───────────────────────────┐  ┌───────────────────────────────────┐
│        Contrato           │  │              Pago                 │
├───────────────────────────┤  ├───────────────────────────────────┤
│ + InmuebleId: int         │  │ + ContratoId: int                │
│ + InquilinoId: int        │  │ + NumeroPago: int                │
│ + FechaInicio: DateTime   │  │ + FechaPago: DateTime            │
│ + FechaFin: DateTime      │  │ + Importe: decimal               │
│ + Monto: decimal          │  │ + Concepto: string               │
│ + Vigente: bool           │  │ + Anulado: bool                  │
│ + FechaTerminacionAnt:Date│  │ + UsuarioCreacionId: int         │
│ + Multa: decimal?         │  │ + UsuarioAnulacionId: int?       │
│ + UsuarioCreacionId: int? │  ├───────────────────────────────────┤
│ + UsuarioModificacionId:  │  │ + Contrato                       │
├───────────────────────────┤  │ + UsuarioCreacion                │
│ + Inmueble                │  │ + UsuarioAnulacion               │
│ + Inquilino               │  └───────────────────────────────────┘
│ + Pagos                   │
│ + UsuarioCreacion         │
└───────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                           Usuario                              │
├─────────────────────────────────────────────────────────────────┤
│ + Nombre: string                                               │
│ + Apellido: string                                             │
│ + Email: string                                                │
│ + Password: string                                             │
│ + Avatar: string                                               │
│ + Rol: string                                                  │
├─────────────────────────────────────────────────────────────────┤
│ + ContratosCreados / ContratosModificados / ContratosTerminados│
│ + PagosCreados / PagosAnulados                                 │
└─────────────────────────────────────────────────────────────────┘

Interfaces


┌─────────────────────────────────────────────────────────────────┐
│                     IRepository<T>                             │
├─────────────────────────────────────────────────────────────────┤
│ + GetAllAsync(): Task<IEnumerable<T>>                         │
│ + GetByIdAsync(id: int): Task<T?>                             │
│ + FindAsync(predicate): Task<IEnumerable<T>>                  │
│ + AddAsync(entity: T): Task                                   │
│ + Update(entity: T): void                                     │
│ + Remove(entity: T): void                                     │
│ + SaveAsync(): Task<bool>                                     │
│ + ExistsAsync(id: int): Task<bool>                            │
└─────────────────────────────────────────────────────────────────┘
                           ▲
                           │
┌─────────────────────────────────────────────────────────────────┐
│                     Repository<T>                              │
├─────────────────────────────────────────────────────────────────┤
│ - _context: ApplicationDbContext                              │
│ - _dbSet: DbSet<T>                                            │
└─────────────────────────────────────────────────────────────────┘


---

## 🚀 INSTRUCCIONES PARA LEVANTAR EL PROYECTO

### Requisitos Previos

| Software | Versión     | Descripción |
|----------|-------------|-------------|
| Laragon  | Última      | Entorno de desarrollo local |
| MySQL    | 5.7+ / 8.0+ | Base de datos |
| .NET SDK | 8.0         | Framework de desarrollo |
| Git      | Última      | Control de versiones |

---

### 🔹 PASO 1: Iniciar Laragon y MySQL

1. Abrir **Laragon**
2. Hacer clic en **"Start All"**
3. Verificar que **MySQL** esté corriendo (ícono verde)
4. Verificar que la base de datos `inmobiliaria_db` **esté creada y vacía** (sin tablas)

Si no la tenés creada, creala desde HeidiSQL o phpMyAdmin:

```sql
CREATE DATABASE inmobiliaria_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

🔹 PASO 2: Clonar el repositorio
bash
git clone <URL_DEL_REPO>
cd Inmobiliaria
O si ya lo tenés:

bash
cd "/d/ULP 2026/Laboratorio de programacion 2/Inmobiliaria"



🛠️ EJECUTAR LA APLICACIÓN

PASO 1: Restaurar Paquetes

dotnet restore

PASO 2: Crear Migración

 # Instalar EF Tools (si no lo tienes)
dotnet tool install --global dotnet-ef

# Crear migración
dotnet ef migrations add InitialCreate

# Aplicar a la base de datos
dotnet ef database update

PASO 3: Ejecutar la Aplicación

dotnet run

🔹 Acceso
URL	Descripción
https://localhost:5001	Aplicación (HTTPS)
http://localhost:5000	Aplicación (HTTP)
https://localhost:5001/Account/Login	Inicio de sesión

🔹 Credenciales de Prueba

Email	Contraseña	Rol
admin@inmobiliaria.com	admin123	Administrador

🛠️ TECNOLOGÍAS UTILIZADAS

Tecnología	Versión	Uso
.NET Core	8.0	Framework principal
ASP.NET MVC	8.0	Patrón MVC
Entity Framework Core	8.0.8	ORM
Pomelo.EntityFrameworkCore.MySql	8.0.2	Provider MySQL
MySQL	8.0+	Base de datos
BCrypt.Net-Next	4.0.3	Encriptación de contraseñas
Bootstrap	5.3	Estilos
Font Awesome	6.0	Iconos


Paquetes NuGet

<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.8" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.8" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="8.0.8" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.2" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" Version="2.2.0" />
