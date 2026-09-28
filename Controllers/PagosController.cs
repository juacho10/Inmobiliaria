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
    public class PagosController : Controller
    {
        private readonly IRepository<Pago> _repository;
        private readonly IRepository<Contrato> _contratoRepo;
        private readonly IRepository<Usuario> _usuarioRepo;
        private const int PageSize = 10;

        public PagosController(
            IRepository<Pago> repository,
            IRepository<Contrato> contratoRepo,
            IRepository<Usuario> usuarioRepo)
        {
            _repository = repository;
            _contratoRepo = contratoRepo;
            _usuarioRepo = usuarioRepo;
        }

        // GET: Pagos
        public async Task<IActionResult> Index(int pagina = 1, int? contratoId = null, string search = "", bool? anulado = null)
        {
            Expression<Func<Pago, bool>>? filtro = null;

            if (contratoId.HasValue)
            {
                Expression<Func<Pago, bool>> f = p => p.ContratoId == contratoId.Value;
                filtro = filtro == null ? f : filtro.And(f);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                Expression<Func<Pago, bool>> f = p => p.Concepto.ToLower().Contains(s);
                filtro = filtro == null ? f : filtro.And(f);
            }

            if (anulado.HasValue)
            {
                Expression<Func<Pago, bool>> f = p => p.Anulado == anulado.Value;
                filtro = filtro == null ? f : filtro.And(f);
            }

            var (items, total) = await _repository.GetPagedAsync(
                pagina,
                PageSize,
                filtro,
                q => q.OrderByDescending(p => p.FechaPago),
                p => p.Contrato!,
                p => p.UsuarioCreacion!);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.TotalRegistros = total;
            ViewBag.Search = search;
            ViewBag.Anulado = anulado;
            ViewBag.ContratoId = contratoId;

            return View(items);
        }

        // GET: Pagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pago = await _repository.GetByIdAsync(id.Value);
            if (pago == null) return NotFound();

            if (pago.ContratoId > 0)
                pago.Contrato = await _contratoRepo.GetByIdAsync(pago.ContratoId);
            if (pago.UsuarioCreacionId > 0)
                pago.UsuarioCreacion = await _usuarioRepo.GetByIdAsync(pago.UsuarioCreacionId);
            if (pago.UsuarioAnulacionId.HasValue)
                pago.UsuarioAnulacion = await _usuarioRepo.GetByIdAsync(pago.UsuarioAnulacionId.Value);

            return View(pago);
        }

        // GET: Pagos/Create?contratoId=X
        public async Task<IActionResult> Create(int? contratoId = null)
        {
            if (contratoId.HasValue)
            {
                var contrato = await _contratoRepo.GetByIdAsync(contratoId.Value);
                if (contrato == null) return NotFound();

                var nuevoPago = new Pago
                {
                    ContratoId = contrato.Id,
                    FechaPago = DateTime.Today,
                    Importe = contrato.Monto,
                    Concepto = $"Pago de contrato #{contrato.Id}",
                    Contrato = contrato
                };
                return View(nuevoPago);
            }

            return View(new Pago { FechaPago = DateTime.Today });
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pago pago)
        {
            var contrato = await _contratoRepo.GetByIdAsync(pago.ContratoId);
            if (contrato == null)
                ModelState.AddModelError("ContratoId", "El contrato seleccionado no existe.");

            if (ModelState.IsValid)
            {
                var pagosExistentes = await _repository.FindAsync(p => p.ContratoId == pago.ContratoId);
                pago.NumeroPago = pagosExistentes.Any()
                    ? pagosExistentes.Max(p => p.NumeroPago) + 1
                    : 1;

                var usuarioEmail = User?.Identity?.Name;
                if (!string.IsNullOrEmpty(usuarioEmail))
                {
                    var usuario = (await _usuarioRepo.FindAsync(u => u.Email == usuarioEmail)).FirstOrDefault();
                    if (usuario != null) pago.UsuarioCreacionId = usuario.Id;
                }

                await _repository.AddAsync(pago);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Pago registrado exitosamente.";
                return RedirectToAction(nameof(Index), new { contratoId = pago.ContratoId });
            }

            pago.Contrato = await _contratoRepo.GetByIdAsync(pago.ContratoId);
            return View(pago);
        }

        // GET: Pagos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pago = await _repository.GetByIdAsync(id.Value);
            if (pago == null) return NotFound();

            if (pago.ContratoId > 0)
                pago.Contrato = await _contratoRepo.GetByIdAsync(pago.ContratoId);

            return View(pago);
        }

        // POST: Pagos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string concepto)
        {
            var pago = await _repository.GetByIdAsync(id);
            if (pago == null) return NotFound();

            if (string.IsNullOrEmpty(concepto))
            {
                ModelState.AddModelError("Concepto", "El concepto es obligatorio.");
            }
            else
            {
                pago.Concepto = concepto;
                pago.FechaModificacion = DateTime.Now;
                _repository.Update(pago);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Concepto actualizado correctamente.";
                return RedirectToAction(nameof(Index), new { contratoId = pago.ContratoId });
            }

            pago.Contrato = await _contratoRepo.GetByIdAsync(pago.ContratoId);
            return View(pago);
        }

        // GET: Pagos/Anular/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Anular(int? id)
        {
            if (id == null) return NotFound();

            var pago = await _repository.GetByIdAsync(id.Value);
            if (pago == null) return NotFound();

            if (pago.ContratoId > 0)
                pago.Contrato = await _contratoRepo.GetByIdAsync(pago.ContratoId);

            return View(pago);
        }

        // POST: Pagos/Anular/5
        [HttpPost, ActionName("Anular")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AnularConfirmed(int id)
        {
            var pago = await _repository.GetByIdAsync(id);
            if (pago != null)
            {
                pago.Anulado = true;
                pago.FechaModificacion = DateTime.Now;

                var usuarioEmail = User?.Identity?.Name;
                if (!string.IsNullOrEmpty(usuarioEmail))
                {
                    var usuario = (await _usuarioRepo.FindAsync(u => u.Email == usuarioEmail)).FirstOrDefault();
                    if (usuario != null) pago.UsuarioAnulacionId = usuario.Id;
                }

                _repository.Update(pago);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Pago anulado correctamente.";
            }
            return RedirectToAction(nameof(Index), new { contratoId = pago?.ContratoId });
        }
    }
}