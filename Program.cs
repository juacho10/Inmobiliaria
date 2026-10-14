using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.Text.Json;
using Inmobiliaria.Data;
using Inmobiliaria.Repository;
using Inmobiliaria.Models;
using Inmobiliaria.Services;
using CloudinaryDotNet;

var builder = WebApplication.CreateBuilder(args);

// ==================== MVC ====================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// ==================== BASE DE DATOS ====================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ==================== REPOSITORIOS ====================
builder.Services.AddScoped<IRepository<Propietario>, Repository<Propietario>>();
builder.Services.AddScoped<IRepository<Inquilino>, Repository<Inquilino>>();
builder.Services.AddScoped<IRepository<Inmueble>, Repository<Inmueble>>();
builder.Services.AddScoped<IRepository<Contrato>, Repository<Contrato>>();
builder.Services.AddScoped<IRepository<Pago>, Repository<Pago>>();
builder.Services.AddScoped<IRepository<Usuario>, Repository<Usuario>>();

// ==================== CLOUDINARY ====================
var cloudName = builder.Configuration["Cloudinary:CloudName"]!;
var apiKey = builder.Configuration["Cloudinary:ApiKey"]!;
var apiSecret = builder.Configuration["Cloudinary:ApiSecret"]!;
var cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
cloudinary.Api.Secure = true;
builder.Services.AddSingleton(cloudinary);
builder.Services.AddScoped<CloudinaryService>();

// ==================== AUTENTICACIÓN (COOKIES + JWT) ====================
var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey))
    };
});

// ==================== AUTORIZACIÓN ====================
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

// ==================== PIPELINE ====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ==================== MIGRACIÓN + SEED ====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        await DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();