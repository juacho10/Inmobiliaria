using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Inmobiliaria.Models;
using Inmobiliaria.Repository;

namespace Inmobiliaria.Controllers
{
    [Authorize(Policy = "SoloPropietarios")] // ✅ POLÍTICA ESPECÍFICA
    public class PropietariosController : Controller
    {
        private readonly IRepository<Propietario> _repository;

        public PropietariosController(IRepository<Propietario> repository)
        {
            _repository = repository;
        }

        // GET: Propietarios
        public async Task<IActionResult> Index(int pagina = 1, string search = "")
        {
            int elementosPorPagina = 10;
            var propietarios = await _repository.GetAllAsync();
            
            // Filtrar si hay búsqueda
            if (!string.IsNullOrEmpty(search))
            {
                propietarios = propietarios.Where(p => 
                    p.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                    p.Apellido.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Dni.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            
            // Paginación
            var totalElementos = propietarios.Count();
            var totalPaginas = (int)Math.Ceiling(totalElementos / (double)elementosPorPagina);
            var propietariosPagina = propietarios.Skip((pagina - 1) * elementosPorPagina).Take(elementosPorPagina);
            
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.Search = search;
            
            return View(propietariosPagina);
        }

        // GET: Propietarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propietario = await _repository.GetByIdAsync(id.Value);
            if (propietario == null)
            {
                return NotFound();
            }

            return View(propietario);
        }

        // GET: Propietarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Propietarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Propietario propietario)
        {
            // Validar que no exista otro propietario con el mismo DNI
            var existeDni = await _repository.FindAsync(x => x.Dni == propietario.Dni);
            if (existeDni.Any())
            {
                ModelState.AddModelError("Dni", "Ya existe un propietario con este DNI");
            }

            // Validar que no exista otro propietario con el mismo Email
            var existeEmail = await _repository.FindAsync(x => x.Email == propietario.Email);
            if (existeEmail.Any())
            {
                ModelState.AddModelError("Email", "Ya existe un propietario con este Email");
            }

            if (ModelState.IsValid)
            {
                await _repository.AddAsync(propietario);
                await _repository.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        // GET: Propietarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propietario = await _repository.GetByIdAsync(id.Value);
            if (propietario == null)
            {
                return NotFound();
            }

            return View(propietario);
        }

        // POST: Propietarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Propietario propietario)
        {
            if (id != propietario.Id)
            {
                return NotFound();
            }

            // Validar que no exista otro propietario con el mismo DNI (excluyendo el actual)
            var existeDni = await _repository.FindAsync(x => x.Dni == propietario.Dni && x.Id != propietario.Id);
            if (existeDni.Any())
            {
                ModelState.AddModelError("Dni", "Ya existe un propietario con este DNI");
            }

            // Validar que no exista otro propietario con el mismo Email (excluyendo el actual)
            var existeEmail = await _repository.FindAsync(x => x.Email == propietario.Email && x.Id != propietario.Id);
            if (existeEmail.Any())
            {
                ModelState.AddModelError("Email", "Ya existe un propietario con este Email");
            }

            if (ModelState.IsValid)
            {
                propietario.FechaModificacion = DateTime.Now;
                _repository.Update(propietario);
                await _repository.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        // GET: Propietarios/Delete/5
        [Authorize(Policy = "Administrador")] // ✅ SOLO ADMINISTRADORES
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propietario = await _repository.GetByIdAsync(id.Value);
            if (propietario == null)
            {
                return NotFound();
            }

            return View(propietario);
        }

        // POST: Propietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")] // ✅ SOLO ADMINISTRADORES
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var propietario = await _repository.GetByIdAsync(id);
            if (propietario != null)
            {
                _repository.Remove(propietario);
                await _repository.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}