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
    public class PagosController : Controller
    {
        private readonly IRepository<Pago> _repository;
        private readonly IRepository<Contrato> _contratoRepo;
        private readonly IRepository<Usuario> _usuarioRepo;

        public PagosController(
            IRepository<Pago> repository,
            IRepository<Contrato> contratoRepo,
            IRepository<Usuario> usuarioRepo)
        {
            _repository = repository;
            _contratoRepo = contratoRepo;
            _usuarioRepo = usuarioRepo;
        }

        // GET: Pagos?contratoId=X
        public async Task<IActionResult> Index(int pagina = 1, int? contratoId = null, string search = "", bool? anulado = null)
        {
            int elementosPorPagina = 10;
            var pagos = await _repository.GetAllAsync();

            // Cargar contratos y usuarios relacionados
            foreach (var pago in pagos)
            {
                if (pago.ContratoId > 0)
                    pago.Contrato = await _contratoRepo.GetByIdAsync(pago.ContratoId);
                if (pago.UsuarioCreacionId > 0)
                    pago.UsuarioCreacion = await _usuarioRepo.GetByIdAsync(pago.UsuarioCreacionId);
                if (pago.UsuarioAnulacionId.HasValue && pago.UsuarioAnulacionId > 0)
                    pago.UsuarioAnulacion = await _usuarioRepo.GetByIdAsync(pago.UsuarioAnulacionId.Value);
            }

            // Filtrar por contrato
            if (contratoId.HasValue)
            {
                pagos = pagos.Where(p => p.ContratoId == contratoId.Value);
                ViewBag.ContratoId = contratoId;
            }

            // Filtrar por búsqueda (concepto)
            if (!string.IsNullOrEmpty(search))
            {
                pagos = pagos.Where(p => p.Concepto.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Filtrar por estado anulado
            if (anulado.HasValue)
            {
                pagos = pagos.Where(p => p.Anulado == anulado.Value);
            }

            // Paginación
            var totalElementos = pagos.Count();
            var totalPaginas = (int)Math.Ceiling(totalElementos / (double)elementosPorPagina);
            var pagosPagina = pagos.OrderByDescending(p => p.FechaPago)
                                   .Skip((pagina - 1) * elementosPorPagina)
                                   .Take(elementosPorPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.Search = search;
            ViewBag.Anulado = anulado;

            return View(pagosPagina);
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
            // Validar que el contrato existe
            var contrato = await _contratoRepo.GetByIdAsync(pago.ContratoId);
            if (contrato == null)
            {
                ModelState.AddModelError("ContratoId", "El contrato seleccionado no existe.");
            }

            if (ModelState.IsValid)
            {
                // Asignar número de pago automáticamente
                var pagosExistentes = await _repository.FindAsync(p => p.ContratoId == pago.ContratoId);
                pago.NumeroPago = pagosExistentes.Any() 
                    ? pagosExistentes.Max(p => p.NumeroPago) + 1 
                    : 1;

                // Asignar usuario de creación
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

            // Recargar contrato si hay error
            pago.Contrato = await _contratoRepo.GetByIdAsync(pago.ContratoId);
            return View(pago);
        }

        // GET: Pagos/Edit/5 (solo permite editar el CONCEPTO)
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
        public async Task<IActionResult> Edit(int id, string concepto, string? fechaPago, string? importe)
        {
            var pago = await _repository.GetByIdAsync(id);
            if (pago == null) return NotFound();

            // Solo actualizar el concepto (NO monto ni fecha)
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

                // Asignar usuario que anuló
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