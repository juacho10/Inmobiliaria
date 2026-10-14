using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Inmobiliaria.Models;
using Inmobiliaria.Repository;
using Inmobiliaria.Services;

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

        public async Task<IActionResult> Index()
        {
            var email = User?.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var usuarios = await _repository.FindAsync(u => u.Email == email);
            var usuario = usuarios.FirstOrDefault();
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        public async Task<IActionResult> Editar()
        {
            var email = User?.Identity?.Name;
            var usuarios = await _repository.FindAsync(u => u.Email == email);
            var usuario = usuarios.FirstOrDefault();
            if (usuario == null) return NotFound();
            return View(usuario);
        }

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

        public IActionResult CambiarPassword() => View();

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

        // ✅ CAMBIAR AVATAR CON CLOUDINARY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarAvatar(IFormFile archivo, [FromServices] CloudinaryService cloudinary)
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

            var extension = Path.GetExtension(archivo.FileName).ToLower();
            var permitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!permitidas.Contains(extension))
            {
                TempData["ErrorMessage"] = "Solo se permiten imágenes JPG, PNG, GIF o WEBP.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var url = await cloudinary.SubirImagenAsync(archivo, "inmobiliaria/avatares");
                if (string.IsNullOrEmpty(url))
                {
                    TempData["ErrorMessage"] = "No se pudo subir la imagen.";
                    return RedirectToAction(nameof(Index));
                }

                usuario.Avatar = url;
                usuario.FechaModificacion = DateTime.Now;
                _repository.Update(usuario);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Foto de perfil actualizada en Cloudinary.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al subir: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

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