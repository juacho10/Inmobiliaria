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

🚀 INSTRUCCIONES PARA LEVANTAR LA BASE DE DATOS

Requisitos Previos
Software	Versión	Descripción
Laragon	Última	Entorno de desarrollo local
MySQL	5.7+	Base de datos
MySQL Workbench	Opcional	Cliente gráfico
.NET SDK	8.0	Framework de desarrollo

🔹 PASO 1: Iniciar Laragon y MySQL
Abrir Laragon

Hacer clic en "Start All"

Verificar que MySQL esté corriendo (ícono verde)

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
Entity Framework Core	9.0.8	ORM
MySQL	8.0+	Base de datos
Pomelo.EntityFrameworkCore.MySql	9.0.0	Provider MySQL
BCrypt.Net-Next	4.0.3	Encriptación
Bootstrap	5.x	Estilos
Font Awesome	6.x	Iconos
jQuery	3.x	JavaScript

Paquetes NuGet

<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.8" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.8" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0-preview.1" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
