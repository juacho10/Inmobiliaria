using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Inmobiliaria.Models;
using Inmobiliaria.Repository;

namespace Inmobiliaria.Controllers
{
    [Authorize(Policy = "SoloPropietarios")]
    public class InquilinosController : Controller
    {
        private readonly IRepository<Inquilino> _repository;
        private const int PageSize = 10;

        public InquilinosController(IRepository<Inquilino> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index(int pagina = 1, string search = "")
        {
            Expression<Func<Inquilino, bool>>? filtro = null;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                filtro = i => i.Nombre.ToLower().Contains(s)
                           || i.Apellido.ToLower().Contains(s)
                           || i.Dni.Contains(s)
                           || i.Email.ToLower().Contains(s);
            }

            var (items, total) = await _repository.GetPagedAsync(
                pagina,
                PageSize,
                filtro,
                q => q.OrderBy(i => i.Apellido).ThenBy(i => i.Nombre));

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
                extraFilter: i => i.Activo,
                orderBy: q => q.OrderBy(i => i.Apellido).ThenBy(i => i.Nombre),
                searchFields: new Expression<Func<Inquilino, string>>[]
                {
                    i => i.Nombre,
                    i => i.Apellido,
                    i => i.Dni,
                    i => i.Email
                });

            return Json(new
            {
                results = items.Select(i => new
                {
                    id = i.Id,
                    text = $"{i.Apellido}, {i.Nombre} - DNI {i.Dni}"
                }),
                pagination = new { more = page * pageSize < total }
            });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var inquilino = await _repository.GetByIdAsync(id.Value);
            if (inquilino == null) return NotFound();
            return View(inquilino);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inquilino inquilino)
        {
            var existeDni = await _repository.FindAsync(x => x.Dni == inquilino.Dni);
            if (existeDni.Any())
                ModelState.AddModelError("Dni", "Ya existe un inquilino con este DNI");

            var existeEmail = await _repository.FindAsync(x => x.Email == inquilino.Email);
            if (existeEmail.Any())
                ModelState.AddModelError("Email", "Ya existe un inquilino con este Email");

            if (ModelState.IsValid)
            {
                await _repository.AddAsync(inquilino);
                await _repository.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inquilino);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var inquilino = await _repository.GetByIdAsync(id.Value);
            if (inquilino == null) return NotFound();
            return View(inquilino);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inquilino inquilino)
        {
            if (id != inquilino.Id) return NotFound();

            var existeDni = await _repository.FindAsync(x => x.Dni == inquilino.Dni && x.Id != inquilino.Id);
            if (existeDni.Any())
                ModelState.AddModelError("Dni", "Ya existe un inquilino con este DNI");

            var existeEmail = await _repository.FindAsync(x => x.Email == inquilino.Email && x.Id != inquilino.Id);
            if (existeEmail.Any())
                ModelState.AddModelError("Email", "Ya existe un inquilino con este Email");

            if (ModelState.IsValid)
            {
                inquilino.FechaModificacion = DateTime.Now;
                _repository.Update(inquilino);
                await _repository.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inquilino);
        }

        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var inquilino = await _repository.GetByIdAsync(id.Value);
            if (inquilino == null) return NotFound();
            return View(inquilino);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
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