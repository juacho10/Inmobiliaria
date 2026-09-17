using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Inmobiliaria.Models;
using Inmobiliaria.Repository;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly IRepository<Usuario> _repository;

        public PerfilController(IRepository<Usuario> repository)
        {
            _repository = repository;
        }

        // GET: Perfil
        public async Task<IActionResult> Index()
        {
            var email = User?.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var usuarios = await _repository.FindAsync(u => u.Email == email);
            var usuario = usuarios.FirstOrDefault();
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // GET: Perfil/Editar
        public async Task<IActionResult> Editar()
        {
            var email = User?.Identity?.Name;
            var usuarios = await _repository.FindAsync(u => u.Email == email);
            var usuario = usuarios.FirstOrDefault();
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Perfil/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(string nombre, string apellido, string email)
        {
            var emailActual = User?.Identity?.Name;
            var usuarios = await _repository.FindAsync(u => u.Email == emailActual);
            var usuario = usuarios.FirstOrDefault();
            if (usuario == null) return NotFound();

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) || string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios.");
                return View(usuario);
            }

            usuario.Nombre = nombre;
            usuario.Apellido = apellido;
            usuario.Email = email;
            usuario.FechaModificacion = DateTime.Now;

            _repository.Update(usuario);
            await _repository.SaveAsync();

            TempData["SuccessMessage"] = "Perfil actualizado. Cierre sesión para aplicar cambios de email.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Perfil/CambiarPassword
        public IActionResult CambiarPassword() => View();

        // POST: Perfil/CambiarPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(string passwordActual, string passwordNueva, string passwordConfirmar)
        {
            if (string.IsNullOrEmpty(passwordActual) || string.IsNullOrEmpty(passwordNueva))
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios.");
                return View();
            }

            if (passwordNueva != passwordConfirmar)
            {
                ModelState.AddModelError("", "Las contraseñas nuevas no coinciden.");
                return View();
            }

            if (passwordNueva.Length < 6)
            {
                ModelState.AddModelError("", "La nueva contraseña debe tener al menos 6 caracteres.");
                return View();
            }

            var email = User?.Identity?.Name;
            var usuarios = await _repository.FindAsync(u => u.Email == email);
            var usuario = usuarios.FirstOrDefault();
            if (usuario == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(passwordActual, usuario.Password))
            {
                ModelState.AddModelError("", "La contraseña actual es incorrecta.");
                return View();
            }

            usuario.Password = BCrypt.Net.BCrypt.HashPassword(passwordNueva);
            usuario.FechaModificacion = DateTime.Now;
            _repository.Update(usuario);
            await _repository.SaveAsync();

            TempData["SuccessMessage"] = "Contraseña cambiada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Perfil/CambiarAvatar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarAvatar(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["ErrorMessage"] = "Debe seleccionar un archivo.";
                return RedirectToAction(nameof(Index));
            }

            var email = User?.Identity?.Name;
            var usuarios = await _repository.FindAsync(u => u.Email == email);
            var usuario = usuarios.FirstOrDefault();
            if (usuario == null) return NotFound();

            // Validar extensión
            var extension = Path.GetExtension(archivo.FileName).ToLower();
            var permitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            if (!permitidas.Contains(extension))
            {
                TempData["ErrorMessage"] = "Solo se permiten imágenes JPG, PNG o GIF.";
                return RedirectToAction(nameof(Index));
            }

            // Guardar en wwwroot/avatars/
            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"{usuario.Id}_{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            usuario.Avatar = $"/avatars/{nombreArchivo}";
            usuario.FechaModificacion = DateTime.Now;
            _repository.Update(usuario);
            await _repository.SaveAsync();

            TempData["SuccessMessage"] = "Foto de perfil actualizada.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Perfil/QuitarAvatar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarAvatar()
        {
            var email = User?.Identity?.Name;
            var usuarios = await _repository.FindAsync(u => u.Email == email);
            var usuario = usuarios.FirstOrDefault();
            if (usuario == null) return NotFound();

            usuario.Avatar = "";
            usuario.FechaModificacion = DateTime.Now;
            _repository.Update(usuario);
            await _repository.SaveAsync();

            TempData["SuccessMessage"] = "Foto de perfil eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }
}