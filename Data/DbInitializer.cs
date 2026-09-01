using Inmobiliaria.Models;
using Microsoft.EntityFrameworkCore;

namespace Inmobiliaria.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            // Asegurar que las migraciones están aplicadas
            await context.Database.MigrateAsync();

            // ✅ CREAR USUARIO ADMIN
            if (!await context.Usuarios.AnyAsync())
            {
                Console.WriteLine("🔧 Creando usuario administrador...");
                
                var adminUser = new Usuario
                {
                    Nombre = "Admin",
                    Apellido = "Sistema",
                    Email = "admin@inmobiliaria.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Rol = "Administrador",
                    Activo = true
                };

                context.Usuarios.Add(adminUser);
                await context.SaveChangesAsync();
                
                Console.WriteLine("✅ Usuario admin creado: admin@inmobiliaria.com / admin123");
            }

            // ✅ CREAR PROPIETARIOS DE PRUEBA
            if (!await context.Propietarios.AnyAsync())
            {
                Console.WriteLine("🔧 Creando propietarios de prueba...");
                
                var propietarios = new List<Propietario>
                {
                    new Propietario
                    {
                        Dni = "30123456",
                        Nombre = "Juan",
                        Apellido = "Pérez",
                        Telefono = "1151234567",
                        Email = "juan.perez@email.com",
                        Activo = true
                    },
                    new Propietario
                    {
                        Dni = "30234567", 
                        Nombre = "María",
                        Apellido = "Gómez",
                        Telefono = "1152345678",
                        Email = "maria.gomez@email.com",
                        Activo = true
                    }
                };

                context.Propietarios.AddRange(propietarios);
                await context.SaveChangesAsync();
                
                Console.WriteLine($"✅ {propietarios.Count} propietarios creados");
            }

            // ✅ CREAR INQUILINOS DE PRUEBA
            if (!await context.Inquilinos.AnyAsync())
            {
                Console.WriteLine("🔧 Creando inquilinos de prueba...");
                
                var inquilinos = new List<Inquilino>
                {
                    new Inquilino
                    {
                        Dni = "40123456",
                        Nombre = "Carlos",
                        Apellido = "López", 
                        Telefono = "1153456789",
                        Email = "carlos.lopez@email.com",
                        Activo = true
                    },
                    new Inquilino
                    {
                        Dni = "40234567",
                        Nombre = "Ana",
                        Apellido = "Martínez",
                        Telefono = "1154567890", 
                        Email = "ana.martinez@email.com",
                        Activo = true
                    }
                };

                context.Inquilinos.AddRange(inquilinos);
                await context.SaveChangesAsync();
                
                Console.WriteLine($"✅ {inquilinos.Count} inquilinos creados");
            }

            Console.WriteLine("🎉 Base de datos inicializada con datos de prueba");
        }
    }
}