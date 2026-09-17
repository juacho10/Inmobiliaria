using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Inmobiliaria.Models;
using Inmobiliaria.Repository;

namespace Inmobiliaria.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly IRepository<Usuario> _repository;

        public UsuariosController(IRepository<Usuario> repository)
        {
            _repository = repository;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(int pagina = 1, string search = "")
        {
            int elementosPorPagina = 10;
            var usuarios = await _repository.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
            {
                usuarios = usuarios.Where(u =>
                    u.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Apellido.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var totalElementos = usuarios.Count();
            var totalPaginas = (int)Math.Ceiling(totalElementos / (double)elementosPorPagina);
            var usuariosPagina = usuarios.Skip((pagina - 1) * elementosPorPagina).Take(elementosPorPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.Search = search;

            return View(usuariosPagina);
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var usuario = await _repository.GetByIdAsync(id.Value);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create() => View();

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario, string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                ModelState.AddModelError("Password", "La contraseña debe tener al menos 6 caracteres.");
            }

            var existeEmail = await _repository.FindAsync(u => u.Email == usuario.Email);
            if (existeEmail.Any())
            {
                ModelState.AddModelError("Email", "Ya existe un usuario con este email.");
            }

            if (ModelState.IsValid)
            {
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(password);
                usuario.Activo = true;
                await _repository.AddAsync(usuario);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Usuario creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var usuario = await _repository.GetByIdAsync(id.Value);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Usuario usuario, string? nuevaPassword)
        {
            if (id != usuario.Id) return NotFound();

            var existeEmail = await _repository.FindAsync(u => u.Email == usuario.Email && u.Id != usuario.Id);
            if (existeEmail.Any())
            {
                ModelState.AddModelError("Email", "Ya existe un usuario con este email.");
            }

            if (ModelState.IsValid)
            {
                var usuarioDb = await _repository.GetByIdAsync(id);
                if (usuarioDb == null) return NotFound();

                usuarioDb.Nombre = usuario.Nombre;
                usuarioDb.Apellido = usuario.Apellido;
                usuarioDb.Email = usuario.Email;
                usuarioDb.Rol = usuario.Rol;
                usuarioDb.Activo = usuario.Activo;
                usuarioDb.FechaModificacion = DateTime.Now;

                if (!string.IsNullOrEmpty(nuevaPassword))
                {
                    if (nuevaPassword.Length < 6)
                    {
                        ModelState.AddModelError("", "La contraseña debe tener al menos 6 caracteres.");
                        return View(usuario);
                    }
                    usuarioDb.Password = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
                }

                _repository.Update(usuarioDb);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var usuario = await _repository.GetByIdAsync(id.Value);
            if (usuario == null) return NotFound();

            // No permitir eliminarse a sí mismo
            var usuarioEmail = User?.Identity?.Name;
            if (usuario.Email == usuarioEmail)
            {
                TempData["ErrorMessage"] = "No puede eliminar su propio usuario.";
                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario != null)
            {
                var usuarioEmail = User?.Identity?.Name;
                if (usuario.Email == usuarioEmail)
                {
                    TempData["ErrorMessage"] = "No puede eliminar su propio usuario.";
                    return RedirectToAction(nameof(Index));
                }

                _repository.Remove(usuario);
                await _repository.SaveAsync();
                TempData["SuccessMessage"] = "Usuario eliminado exitosamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}