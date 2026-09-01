using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Inmobiliaria.Models;
using Inmobiliaria.Repository;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository<Usuario> _repository;

        public AccountController(IRepository<Usuario> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [AllowAnonymous] // ✅ FILTRO PARA PERMITIR ANÓNIMOS (Page 3 del PDF)
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous] // ✅ FILTRO PARA PERMITIR ANÓNIMOS
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var usuarios = await _repository.FindAsync(u => u.Email == model.Email && u.Activo);
                var usuario = usuarios.FirstOrDefault();
                
                if (usuario != null && BCrypt.Net.BCrypt.Verify(model.Password, usuario.Password))
                {
                    // ✅ CREAR COOKIE DE AUTENTICACIÓN (Page 7 del PDF)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                        new Claim(ClaimTypes.Name, usuario.Email), // Para User.Identity.Name
                        new Claim("FullName", usuario.NombreCompleto), // ✅ Como en el PDF
                        new Claim(ClaimTypes.Email, usuario.Email),
                        new Claim(ClaimTypes.Role, usuario.Rol)
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(7)),
                        AllowRefresh = true,
                        IssuedUtc = DateTimeOffset.UtcNow
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Credenciales inválidas");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // ✅ ACCIÓN DE LOGOUT (Page 9 del PDF)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize] // ✅ FILTRO DE AUTORIZACIÓN
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ✅ MÉTODO PARA PROBAR User.Identity (Page 10 del PDF)
        [Authorize]
        public IActionResult UserInfo()
        {
            var userInfo = new
            {
                Name = User.Identity.Name,
                IsAuthenticated = User.Identity.IsAuthenticated,
                IsInRoleAdmin = User.IsInRole("Administrador"),
                IsInRoleEmpleado = User.IsInRole("Empleado"),
                Role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value,
                FullName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value,
                Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };
            
            return Json(userInfo);
        }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }
    }
}