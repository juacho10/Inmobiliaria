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
    public class InquilinosController : Controller
    {
        private readonly IRepository<Inquilino> _repository;

        public InquilinosController(IRepository<Inquilino> repository)
        {
            _repository = repository;
        }

        // GET: Inquilinos con paginación y búsqueda
        public async Task<IActionResult> Index(int pagina = 1, string search = "")
        {
            int elementosPorPagina = 10;
            var inquilinos = await _repository.GetAllAsync();

            // Filtrar si hay búsqueda
            if (!string.IsNullOrEmpty(search))
            {
                inquilinos = inquilinos.Where(i =>
                    i.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    i.Apellido.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    i.Dni.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Paginación
            var totalElementos = inquilinos.Count();
            var totalPaginas = (int)Math.Ceiling(totalElementos / (double)elementosPorPagina);
            var inquilinosPagina = inquilinos.Skip((pagina - 1) * elementosPorPagina).Take(elementosPorPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.Search = search;

            return View(inquilinosPagina);
        }

        // GET: Inquilinos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquilino = await _repository.GetByIdAsync(id.Value);
            if (inquilino == null)
            {
                return NotFound();
            }

            return View(inquilino);
        }

        // GET: Inquilinos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inquilinos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inquilino inquilino)
        {
            // Validar que no exista otro inquilino con el mismo DNI
            var existeDni = await _repository.FindAsync(x => x.Dni == inquilino.Dni);
            if (existeDni.Any())
            {
                ModelState.AddModelError("Dni", "Ya existe un inquilino con este DNI");
            }

            // Validar que no exista otro inquilino con el mismo Email
            var existeEmail = await _repository.FindAsync(x => x.Email == inquilino.Email);
            if (existeEmail.Any())
            {
                ModelState.AddModelError("Email", "Ya existe un inquilino con este Email");
            }

            if (ModelState.IsValid)
            {
                await _repository.AddAsync(inquilino);
                await _repository.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inquilino);
        }

        // GET: Inquilinos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquilino = await _repository.GetByIdAsync(id.Value);
            if (inquilino == null)
            {
                return NotFound();
            }
            return View(inquilino);
        }

        // POST: Inquilinos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inquilino inquilino)
        {
            if (id != inquilino.Id)
            {
                return NotFound();
            }

            // Validar que no exista otro inquilino con el mismo DNI (excluyendo el actual)
            var existeDni = await _repository.FindAsync(x => x.Dni == inquilino.Dni && x.Id != inquilino.Id);
            if (existeDni.Any())
            {
                ModelState.AddModelError("Dni", "Ya existe un inquilino con este DNI");
            }

            // Validar que no exista otro inquilino con el mismo Email (excluyendo el actual)
            var existeEmail = await _repository.FindAsync(x => x.Email == inquilino.Email && x.Id != inquilino.Id);
            if (existeEmail.Any())
            {
                ModelState.AddModelError("Email", "Ya existe un inquilino con este Email");
            }

            if (ModelState.IsValid)
            {
                inquilino.FechaModificacion = DateTime.Now;
                _repository.Update(inquilino);
                await _repository.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inquilino);
        }

        // GET: Inquilinos/Delete/5
        [Authorize(Policy = "Administrador")] // ✅ SOLO ADMINISTRADORES
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquilino = await _repository.GetByIdAsync(id.Value);
            if (inquilino == null)
            {
                return NotFound();
            }

            return View(inquilino);
        }

        // POST: Inquilinos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")] // ✅ SOLO ADMINISTRADORES
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inquilino = await _repository.GetByIdAsync(id);
            if (inquilino != null)
            {
                _repository.Remove(inquilino);
                await _repository.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}