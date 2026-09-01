using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Inmobiliaria.Models;
using Inmobiliaria.Repository;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class InmueblesController : Controller
    {
        private readonly IRepository<Inmueble> _repository;
        private readonly IRepository<Propietario> _propietarioRepo;

        public InmueblesController(IRepository<Inmueble> repository, IRepository<Propietario> propietarioRepo)
        {
            _repository = repository;
            _propietarioRepo = propietarioRepo;
        }

        // GET: Inmuebles con paginación y búsqueda
        public async Task<IActionResult> Index(int pagina = 1, string search = "", bool? disponible = null)
        {
            int elementosPorPagina = 10;
            var inmuebles = await _repository.GetAllAsync();

            // Cargar datos relacionados
            foreach (var inmueble in inmuebles)
            {
                if (inmueble.PropietarioId > 0)
                {
                    inmueble.Propietario = await _propietarioRepo.GetByIdAsync(inmueble.PropietarioId);
                }
            }

            // Filtrar por búsqueda
            if (!string.IsNullOrEmpty(search))
            {
                inmuebles = inmuebles.Where(i =>
                    i.Direccion.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    i.Tipo.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (i.Propietario != null && i.Propietario.NombreCompleto.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            // Filtrar por disponibilidad
            if (disponible.HasValue)
            {
                inmuebles = inmuebles.Where(i => i.Disponible == disponible.Value);
            }

            // Paginación
            var totalElementos = inmuebles.Count();
            var totalPaginas = (int)Math.Ceiling(totalElementos / (double)elementosPorPagina);
            var inmueblesPagina = inmuebles.Skip((pagina - 1) * elementosPorPagina).Take(elementosPorPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.Search = search;
            ViewBag.Disponible = disponible;

            return View(inmueblesPagina);
        }

        // GET: Inmuebles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inmueble = await _repository.GetByIdAsync(id.Value);
            if (inmueble == null)
            {
                return NotFound();
            }

            // Cargar propietario
            if (inmueble.PropietarioId > 0)
            {
                inmueble.Propietario = await _propietarioRepo.GetByIdAsync(inmueble.PropietarioId);
            }

            return View(inmueble);
        }

        // GET: Inmuebles/Create
        public async Task<IActionResult> Create()
        {
            // Cargar lista de propietarios para el dropdown
            var propietarios = await _propietarioRepo.GetAllAsync();
            ViewBag.Propietarios = propietarios.Where(p => p.Activo).ToList();
            return View();
        }

        // POST: Inmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(inmueble);
                await _repository.SaveAsync();
                return RedirectToAction(nameof(Index));
            }

            // Recargar propietarios si hay error
            var propietarios = await _propietarioRepo.GetAllAsync();
            ViewBag.Propietarios = propietarios.Where(p => p.Activo).ToList();
            return View(inmueble);
        }

        // GET: Inmuebles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inmueble = await _repository.GetByIdAsync(id.Value);
            if (inmueble == null)
            {
                return NotFound();
            }

            // Cargar lista de propietarios
            var propietarios = await _propietarioRepo.GetAllAsync();
            ViewBag.Propietarios = propietarios.Where(p => p.Activo).ToList();

            return View(inmueble);
        }

        // POST: Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inmueble inmueble)
        {
            if (id != inmueble.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                inmueble.FechaModificacion = DateTime.Now;
                _repository.Update(inmueble);
                await _repository.SaveAsync();
                return RedirectToAction(nameof(Index));
            }

            // Recargar propietarios si hay error
            var propietarios = await _propietarioRepo.GetAllAsync();
            ViewBag.Propietarios = propietarios.Where(p => p.Activo).ToList();
            return View(inmueble);
        }

        // GET: Inmuebles/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inmueble = await _repository.GetByIdAsync(id.Value);
            if (inmueble == null)
            {
                return NotFound();
            }

            // Cargar propietario
            if (inmueble.PropietarioId > 0)
            {
                inmueble.Propietario = await _propietarioRepo.GetByIdAsync(inmueble.PropietarioId);
            }

            // Verificar si tiene contratos activos
            var contratosRepo = HttpContext.RequestServices.GetService<IRepository<Contrato>>();
            var contratosActivos = Enumerable.Empty<Contrato>();
            var contratosHistoricos = Enumerable.Empty<Contrato>();

            if (contratosRepo != null)
            {
                contratosActivos = await contratosRepo.FindAsync(c =>
                    c.InmuebleId == id.Value && c.Vigente);

                contratosHistoricos = await contratosRepo.FindAsync(c =>
                    c.InmuebleId == id.Value && !c.Vigente);
            }

            ViewBag.TieneContratosActivos = contratosActivos.Any();
            ViewBag.TieneContratosHistoricos = contratosHistoricos.Any();

            return View(inmueble);
        }

        // POST: Inmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inmueble = await _repository.GetByIdAsync(id);
            if (inmueble != null)
            {
                _repository.Remove(inmueble);
                await _repository.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}