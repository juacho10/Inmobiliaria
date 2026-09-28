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
    [Authorize(Policy = "SoloPropietarios")]
    public class PropietariosController : Controller
    {
        private readonly IRepository<Propietario> _repository;
        private const int PageSize = 10;

        public PropietariosController(IRepository<Propietario> repository)
        {
            _repository = repository;
        }

        // GET: Propietarios
        public async Task<IActionResult> Index(int pagina = 1, string search = "")
        {
            Expression<Func<Propietario, bool>>? filtro = null;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                filtro = p => p.Nombre.ToLower().Contains(s)
                           || p.Apellido.ToLower().Contains(s)
                           || p.Dni.Contains(s)
                           || p.Email.ToLower().Contains(s);
            }

            var (items, total) = await _repository.GetPagedAsync(
                pagina,
                PageSize,
                filtro,
                q => q.OrderBy(p => p.Apellido).ThenBy(p => p.Nombre));

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.TotalRegistros = total;
            ViewBag.Search = search;

            return View(items);
        }

        // ✅ Endpoint JSON para Select2
        [HttpGet]
        public async Task<IActionResult> Buscar(string term = "", int page = 1)
        {
            const int pageSize = 20;

            var (items, total) = await _repository.SearchAsync(
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

        // GET: Propietarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var propietario = await _repository.GetByIdAsync(id.Value);
            if (propietario == null) return NotFound();

            return View(propietario);
        }

        // GET: Propietarios/Create
        public IActionResult Create() => View();

        // POST: Propietarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Propietario propietario)
        {
            var existeDni = await _repository.FindAsync(x => x.Dni == propietario.Dni);
            if (existeDni.Any())
                ModelState.AddModelError("Dni", "Ya existe un propietario con este DNI");

            var existeEmail = await _repository.FindAsync(x => x.Email == propietario.Email);
            if (existeEmail.Any())
                ModelState.AddModelError("Email", "Ya existe un propietario con este Email");

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
            if (id == null) return NotFound();
            var propietario = await _repository.GetByIdAsync(id.Value);
            if (propietario == null) return NotFound();
            return View(propietario);
        }

        // POST: Propietarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Propietario propietario)
        {
            if (id != propietario.Id) return NotFound();

            var existeDni = await _repository.FindAsync(x => x.Dni == propietario.Dni && x.Id != propietario.Id);
            if (existeDni.Any())
                ModelState.AddModelError("Dni", "Ya existe un propietario con este DNI");

            var existeEmail = await _repository.FindAsync(x => x.Email == propietario.Email && x.Id != propietario.Id);
            if (existeEmail.Any())
                ModelState.AddModelError("Email", "Ya existe un propietario con este Email");

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
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var propietario = await _repository.GetByIdAsync(id.Value);
            if (propietario == null) return NotFound();
            return View(propietario);
        }

        // POST: Propietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
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