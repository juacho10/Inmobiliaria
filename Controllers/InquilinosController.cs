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

        // ==================== VISTA VUE.JS ====================
        [HttpGet]
        public IActionResult VueIndex()
        {
            return View();
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

        // ==================== ENDPOINTS PARA VUE.JS (ABM AJAX) ====================

        [HttpGet]
        public async Task<IActionResult> GetInquilinos(int page = 1, int pageSize = 10, string search = "")
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
                page, pageSize, filtro,
                q => q.OrderBy(i => i.Apellido).ThenBy(i => i.Nombre));

            return Json(new
            {
                items = items.Select(i => new
                {
                    i.Id,
                    i.Dni,
                    i.Nombre,
                    i.Apellido,
                    i.Telefono,
                    i.Email,
                    i.Activo
                }),
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] Inquilino inquilino)
        {
            if (inquilino == null) return BadRequest(new { message = "Datos inválidos" });

            var existeDni = await _repository.FindAsync(x => x.Dni == inquilino.Dni);
            if (existeDni.Any())
                return BadRequest(new { message = "Ya existe un inquilino con este DNI" });

            var existeEmail = await _repository.FindAsync(x => x.Email == inquilino.Email);
            if (existeEmail.Any())
                return BadRequest(new { message = "Ya existe un inquilino con este Email" });

            await _repository.AddAsync(inquilino);
            await _repository.SaveAsync();

            return Json(new { success = true, id = inquilino.Id, message = "Inquilino creado exitosamente" });
        }

        [HttpPut]
        public async Task<IActionResult> EditAjax(int id, [FromBody] Inquilino inquilino)
        {
            if (inquilino == null) return BadRequest(new { message = "Datos inválidos" });
            if (id != inquilino.Id) return BadRequest(new { message = "ID no coincide" });

            var existeDni = await _repository.FindAsync(x => x.Dni == inquilino.Dni && x.Id != inquilino.Id);
            if (existeDni.Any())
                return BadRequest(new { message = "Ya existe un inquilino con este DNI" });

            var existeEmail = await _repository.FindAsync(x => x.Email == inquilino.Email && x.Id != inquilino.Id);
            if (existeEmail.Any())
                return BadRequest(new { message = "Ya existe un inquilino con este Email" });

            var entidad = await _repository.GetByIdAsync(id);
            if (entidad == null) return NotFound(new { message = "No encontrado" });

            entidad.Dni = inquilino.Dni;
            entidad.Nombre = inquilino.Nombre;
            entidad.Apellido = inquilino.Apellido;
            entidad.Telefono = inquilino.Telefono;
            entidad.Email = inquilino.Email;
            entidad.Activo = inquilino.Activo;
            entidad.FechaModificacion = DateTime.Now;

            _repository.Update(entidad);
            await _repository.SaveAsync();

            return Json(new { success = true, message = "Inquilino actualizado exitosamente" });
        }

        [HttpDelete]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var inquilino = await _repository.GetByIdAsync(id);
            if (inquilino == null) return NotFound(new { message = "Inquilino no encontrado" });

            _repository.Remove(inquilino);
            await _repository.SaveAsync();

            return Json(new { success = true, message = "Inquilino eliminado exitosamente" });
        }
    }
}