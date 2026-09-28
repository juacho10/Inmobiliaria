using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Inmobiliaria.Helpers;
using Inmobiliaria.Models;
using Inmobiliaria.Repository;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class InmueblesController : Controller
    {
        private readonly IRepository<Inmueble> _repository;
        private readonly IRepository<Propietario> _propietarioRepo;
        private const int PageSize = 10;

        public InmueblesController(IRepository<Inmueble> repository, IRepository<Propietario> propietarioRepo)
        {
            _repository = repository;
            _propietarioRepo = propietarioRepo;
        }

        // GET: Inmuebles
        public async Task<IActionResult> Index(int pagina = 1, string search = "", bool? disponible = null, int? propietarioId = null)
        {
            Expression<Func<Inmueble, bool>>? filtro = null;

            if (propietarioId.HasValue)
            {
                Expression<Func<Inmueble, bool>> f1 = i => i.PropietarioId == propietarioId.Value;
                filtro = filtro == null ? f1 : filtro.And(f1);

                var prop = await _propietarioRepo.GetByIdAsync(propietarioId.Value);
                if (prop != null)
                    ViewBag.TituloEspecial = $"Inmuebles de: {prop.NombreCompleto}";
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                Expression<Func<Inmueble, bool>> f2 = i =>
                    i.Direccion.ToLower().Contains(s) ||
                    i.Tipo.ToLower().Contains(s);
                filtro = filtro == null ? f2 : filtro.And(f2);
            }

            if (disponible.HasValue)
            {
                Expression<Func<Inmueble, bool>> f3 = i => i.Disponible == disponible.Value;
                filtro = filtro == null ? f3 : filtro.And(f3);
            }

            var (items, total) = await _repository.GetPagedAsync(
                pagina,
                PageSize,
                filtro,
                q => q.OrderBy(i => i.Direccion),
                i => i.Propietario!);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.TotalRegistros = total;
            ViewBag.Search = search;
            ViewBag.Disponible = disponible;
            ViewBag.PropietarioId = propietarioId;

            return View(items);
        }

        // ✅ Endpoint JSON para Select2 (en Create/Edit)
        [HttpGet]
        public async Task<IActionResult> BuscarPropietarios(string term = "", int page = 1)
        {
            const int pageSize = 20;

            var (items, total) = await _propietarioRepo.SearchAsync(
                term,
                page,
                pageSize,
                extraFilter: p => p.Activo,
                orderBy: q => q.OrderBy(p => p.Apellido).ThenBy(p => p.Nombre),
                searchFields: new Expression<Func<Propietario, string>>[]
                {
                    p => p.Nombre,
                    p => p.Apellido,
                    p => p.Dni,
                    p => p.Email
                });

            return Json(new
            {
                results = items.Select(p => new
                {
                    id = p.Id,
                    text = $"{p.Apellido}, {p.Nombre} - DNI {p.Dni}"
                }),
                pagination = new { more = page * pageSize < total }
            });
        }

        // GET: Inmuebles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = await _repository.GetByIdAsync(id.Value);
            if (inmueble == null) return NotFound();

            if (inmueble.PropietarioId > 0)
                inmueble.Propietario = await _propietarioRepo.GetByIdAsync(inmueble.PropietarioId);

            return View(inmueble);
        }

        // GET: Inmuebles/Create
        public IActionResult Create() => View();

        // POST: Inmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(inmueble);
                await _repository.SaveAsync();
                TempData["SuccessMessage"] = "Inmueble creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(inmueble);
        }

        // GET: Inmuebles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = await _repository.GetByIdAsync(id.Value);
            if (inmueble == null) return NotFound();

            // Cargar propietario actual para mostrar en el select
            if (inmueble.PropietarioId > 0)
                inmueble.Propietario = await _propietarioRepo.GetByIdAsync(inmueble.PropietarioId);

            return View(inmueble);
        }

        // POST: Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inmueble inmueble)
        {
            if (id != inmueble.Id) return NotFound();

            if (ModelState.IsValid)
            {
                inmueble.FechaModificacion = DateTime.Now;
                _repository.Update(inmueble);
                await _repository.SaveAsync();
                TempData["SuccessMessage"] = "Inmueble actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            // Recargar propietario actual si hay error
            if (inmueble.PropietarioId > 0)
                inmueble.Propietario = await _propietarioRepo.GetByIdAsync(inmueble.PropietarioId);

            return View(inmueble);
        }

        // GET: Inmuebles/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = await _repository.GetByIdAsync(id.Value);
            if (inmueble == null) return NotFound();

            if (inmueble.PropietarioId > 0)
                inmueble.Propietario = await _propietarioRepo.GetByIdAsync(inmueble.PropietarioId);

            var contratosRepo = HttpContext.RequestServices.GetService<IRepository<Contrato>>();
            var contratosActivos = Enumerable.Empty<Contrato>();
            var contratosHistoricos = Enumerable.Empty<Contrato>();

            if (contratosRepo != null)
            {
                contratosActivos = await contratosRepo.FindAsync(c => c.InmuebleId == id.Value && c.Vigente);
                contratosHistoricos = await contratosRepo.FindAsync(c => c.InmuebleId == id.Value && !c.Vigente);
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
                TempData["SuccessMessage"] = "Inmueble eliminado exitosamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}