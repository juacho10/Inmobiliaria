using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Inmobiliaria.Data;
using Inmobiliaria.Repository;
using Inmobiliaria.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Register repositories
builder.Services.AddScoped<IRepository<Propietario>, Repository<Propietario>>();
builder.Services.AddScoped<IRepository<Inquilino>, Repository<Inquilino>>();
builder.Services.AddScoped<IRepository<Inmueble>, Repository<Inmueble>>();
builder.Services.AddScoped<IRepository<Contrato>, Repository<Contrato>>();
builder.Services.AddScoped<IRepository<Pago>, Repository<Pago>>();
builder.Services.AddScoped<IRepository<Usuario>, Repository<Usuario>>();

// ✅ CONFIGURACIÓN DE AUTENTICACIÓN CON COOKIES
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";  
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// ✅ CREAR POLÍTICAS DE AUTORIZACIÓN
builder.Services.AddAuthorization(options =>  
{  
    options.AddPolicy("Administrador", policy =>  
        policy.RequireClaim(ClaimTypes.Role, "Administrador"));
    
    options.AddPolicy("Empleado", policy =>  
        policy.RequireClaim(ClaimTypes.Role, "Empleado", "Administrador"));
    
    options.AddPolicy("SoloPropietarios", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => 
                c.Type == ClaimTypes.Role && 
                (c.Value == "Administrador" || c.Value == "Empleado"))));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ✅ HABILITAR AUTENTICACIÓN Y AUTORIZACIÓN
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        
        // Seed initial data
        await DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();