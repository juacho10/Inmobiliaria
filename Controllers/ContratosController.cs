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
    public class ContratosController : Controller
    {
        private readonly IRepository<Contrato> _repository;
        private readonly IRepository<Inmueble> _inmuebleRepo;
        private readonly IRepository<Inquilino> _inquilinoRepo;
        private readonly IRepository<Usuario> _usuarioRepo;
        private readonly IRepository<Propietario> _propietarioRepo;
        private readonly IRepository<Pago> _pagoRepo;

        public ContratosController(
            IRepository<Contrato> repository,
            IRepository<Inmueble> inmuebleRepo,
            IRepository<Inquilino> inquilinoRepo,
            IRepository<Usuario> usuarioRepo,
            IRepository<Propietario> propietarioRepo,
            IRepository<Pago> pagoRepo)
        {
            _repository = repository;
            _inmuebleRepo = inmuebleRepo;
            _inquilinoRepo = inquilinoRepo;
            _usuarioRepo = usuarioRepo;
            _propietarioRepo = propietarioRepo;
            _pagoRepo = pagoRepo;
        }

        // GET: Contratos con paginación y filtros
        public async Task<IActionResult> Index(int pagina = 1, string search = "", bool? vigente = null)
        {
            int elementosPorPagina = 10;
            var contratos = await _repository.GetAllAsync();

            foreach (var contrato in contratos)
            {
                if (contrato.InmuebleId > 0)
                    contrato.Inmueble = await _inmuebleRepo.GetByIdAsync(contrato.InmuebleId);
                if (contrato.InquilinoId > 0)
                    contrato.Inquilino = await _inquilinoRepo.GetByIdAsync(contrato.InquilinoId);
            }

            if (!string.IsNullOrEmpty(search))
            {
                contratos = contratos.Where(c =>
                    (c.Inmueble != null && c.Inmueble.Direccion.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (c.Inquilino != null && c.Inquilino.NombreCompleto.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            if (vigente.HasValue)
            {
                contratos = contratos.Where(c => c.Vigente == vigente.Value);
            }

            var totalElementos = contratos.Count();
            var totalPaginas = (int)Math.Ceiling(totalElementos / (double)elementosPorPagina);
            var contratosPagina = contratos.OrderByDescending(c => c.FechaInicio)
                                           .Skip((pagina - 1) * elementosPorPagina)
                                           .Take(elementosPorPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.Search = search;
            ViewBag.Vigente = vigente;

            return View(contratosPagina);
        }

        // GET: Contratos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contrato = await _repository.GetByIdAsync(id.Value);
            if (contrato == null) return NotFound();

            if (contrato.InmuebleId > 0)
            {
                contrato.Inmueble = await _inmuebleRepo.GetByIdAsync(contrato.InmuebleId);
                if (contrato.Inmueble?.PropietarioId > 0)
                {
                    contrato.Inmueble.Propietario = await _propietarioRepo.GetByIdAsync(contrato.Inmueble.PropietarioId);
                }
            }
            if (contrato.InquilinoId > 0)
                contrato.Inquilino = await _inquilinoRepo.GetByIdAsync(contrato.InquilinoId);

            // Cargar usuarios de auditoría
            if (contrato.UsuarioCreacionId.HasValue && contrato.UsuarioCreacionId > 0)
                contrato.UsuarioCreacion = await _usuarioRepo.GetByIdAsync(contrato.UsuarioCreacionId.Value);
            if (contrato.UsuarioModificacionId.HasValue && contrato.UsuarioModificacionId > 0)
                contrato.UsuarioModificacion = await _usuarioRepo.GetByIdAsync(contrato.UsuarioModificacionId.Value);
            if (contrato.UsuarioTerminacionId.HasValue && contrato.UsuarioTerminacionId > 0)
                contrato.UsuarioTerminacion = await _usuarioRepo.GetByIdAsync(contrato.UsuarioTerminacionId.Value);

            return View(contrato);
        }

        // GET: Contratos/Create
        public async Task<IActionResult> Create()
        {
            await CargarListas();
            return View();
        }

        // POST: Contratos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contrato contrato)
        {
            var contratosExistentes = await _repository.FindAsync(c =>
                c.InmuebleId == contrato.InmuebleId &&
                c.Vigente &&
                ((contrato.FechaInicio >= c.FechaInicio && contrato.FechaInicio <= c.FechaFin) ||
                 (contrato.FechaFin >= c.FechaInicio && contrato.FechaFin <= c.FechaFin) ||
                 (contrato.FechaInicio <= c.FechaInicio && contrato.FechaFin >= c.FechaFin)));

            if (contratosExistentes.Any())
            {
                ModelState.AddModelError("", "El inmueble no está disponible en las fechas seleccionadas.");
            }

            if (ModelState.IsValid)
            {
                var usuarioEmail = User?.Identity?.Name;
                if (!string.IsNullOrEmpty(usuarioEmail))
                {
                    var usuarios = await _usuarioRepo.FindAsync(u => u.Email == usuarioEmail);
                    var usuario = usuarios.FirstOrDefault();
                    if (usuario != null)
                    {
                        contrato.UsuarioCreacionId = usuario.Id;
                    }
                }

                await _repository.AddAsync(contrato);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Contrato creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            await CargarListas();
            return View(contrato);
        }

        // GET: Contratos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contrato = await _repository.GetByIdAsync(id.Value);
            if (contrato == null) return NotFound();

            await CargarListas();
            return View(contrato);
        }

        // POST: Contratos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contrato contrato)
        {
            if (id != contrato.Id) return NotFound();

            var contratosExistentes = await _repository.FindAsync(c =>
                c.InmuebleId == contrato.InmuebleId &&
                c.Vigente &&
                c.Id != contrato.Id &&
                ((contrato.FechaInicio >= c.FechaInicio && contrato.FechaInicio <= c.FechaFin) ||
                 (contrato.FechaFin >= c.FechaInicio && contrato.FechaFin <= c.FechaFin) ||
                 (contrato.FechaInicio <= c.FechaInicio && contrato.FechaFin >= c.FechaFin)));

            if (contratosExistentes.Any())
            {
                ModelState.AddModelError("", "El inmueble no está disponible en las fechas seleccionadas.");
            }

            if (ModelState.IsValid)
            {
                var usuarioEmail = User?.Identity?.Name;
                var usuario = (usuarioEmail != null)
                    ? (await _usuarioRepo.FindAsync(u => u.Email == usuarioEmail)).FirstOrDefault()
                    : null;
                if (usuario != null)
                {
                    contrato.UsuarioModificacionId = usuario.Id;
                }

                contrato.FechaModificacion = DateTime.Now;
                _repository.Update(contrato);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Contrato actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            await CargarListas();
            return View(contrato);
        }

        // GET: Contratos/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var contrato = await _repository.GetByIdAsync(id.Value);
            if (contrato == null) return NotFound();

            if (contrato.InmuebleId > 0)
            {
                contrato.Inmueble = await _inmuebleRepo.GetByIdAsync(contrato.InmuebleId);
                if (contrato.Inmueble?.PropietarioId > 0)
                {
                    contrato.Inmueble.Propietario = await _propietarioRepo.GetByIdAsync(contrato.Inmueble.PropietarioId);
                }
            }
            if (contrato.InquilinoId > 0)
                contrato.Inquilino = await _inquilinoRepo.GetByIdAsync(contrato.InquilinoId);

            var pagosActivos = await _pagoRepo.FindAsync(p => p.ContratoId == id.Value && !p.Anulado);
            var pagosHistoricos = await _pagoRepo.FindAsync(p => p.ContratoId == id.Value && p.Anulado);

            ViewBag.TienePagosActivos = pagosActivos.Any();
            ViewBag.TienePagosHistoricos = pagosHistoricos.Any();

            return View(contrato);
        }

        // POST: Contratos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contrato = await _repository.GetByIdAsync(id);
            if (contrato != null)
            {
                _repository.Remove(contrato);
                await _repository.SaveAsync();
                TempData["SuccessMessage"] = "Contrato eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Contratos/Renovar/5
        public async Task<IActionResult> Renovar(int? id)
        {
            if (id == null) return NotFound();

            var contratoOriginal = await _repository.GetByIdAsync(id.Value);
            if (contratoOriginal == null) return NotFound();

            var nuevoContrato = new Contrato
            {
                InmuebleId = contratoOriginal.InmuebleId,
                InquilinoId = contratoOriginal.InquilinoId,
                FechaInicio = contratoOriginal.FechaFin.AddDays(1),
                FechaFin = contratoOriginal.FechaFin.AddYears(1),
                Monto = contratoOriginal.Monto,
                Vigente = true,
                Inmueble = await _inmuebleRepo.GetByIdAsync(contratoOriginal.InmuebleId),
                Inquilino = await _inquilinoRepo.GetByIdAsync(contratoOriginal.InquilinoId)
            };

            ViewBag.ContratoOriginalId = id.Value;
            ViewBag.MontoAnterior = contratoOriginal.Monto;
            ViewBag.FechaFinAnterior = contratoOriginal.FechaFin;

            await CargarListas();
            return View(nuevoContrato);
        }

        // POST: Contratos/Renovar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renovar(Contrato contrato)
        {
            // Validar superposición
            var contratosExistentes = await _repository.FindAsync(c =>
                c.InmuebleId == contrato.InmuebleId &&
                c.Vigente &&
                ((contrato.FechaInicio >= c.FechaInicio && contrato.FechaInicio <= c.FechaFin) ||
                 (contrato.FechaFin >= c.FechaInicio && contrato.FechaFin <= c.FechaFin) ||
                 (contrato.FechaInicio <= c.FechaInicio && contrato.FechaFin >= c.FechaFin)));

            if (contratosExistentes.Any())
            {
                ModelState.AddModelError("", "El inmueble no está disponible en las fechas seleccionadas.");
            }

            if (ModelState.IsValid)
            {
                var usuarioEmail = User?.Identity?.Name;
                var usuario = (usuarioEmail != null)
                    ? (await _usuarioRepo.FindAsync(u => u.Email == usuarioEmail)).FirstOrDefault()
                    : null;
                if (usuario != null)
                {
                    contrato.UsuarioCreacionId = usuario.Id;
                }

                await _repository.AddAsync(contrato);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Contrato renovado exitosamente.";
                return RedirectToAction(nameof(Details), new { id = contrato.Id });
            }

            await CargarListas();
            return View(contrato);
        }

        // GET: Contratos/PorInmueble/5
        public async Task<IActionResult> PorInmueble(int inmuebleId)
        {
            var inmueble = await _inmuebleRepo.GetByIdAsync(inmuebleId);
            if (inmueble == null) return NotFound();

            var contratos = await _repository.FindAsync(c => c.InmuebleId == inmuebleId);
            foreach (var c in contratos)
            {
                c.Inmueble = inmueble;
                c.Inquilino = await _inquilinoRepo.GetByIdAsync(c.InquilinoId);
            }

            ViewBag.Inmueble = inmueble;
            ViewBag.TituloEspecial = $"Contratos del inmueble: {inmueble.Direccion}";
            return View("Index", contratos.OrderByDescending(c => c.FechaInicio).ToList());
        }

        // GET: Contratos/Vigentes
        public async Task<IActionResult> Vigentes(int pagina = 1)
        {
            int elementosPorPagina = 10;
            var hoy = DateTime.Today;
            var contratos = await _repository.FindAsync(c =>
                c.Vigente && c.FechaInicio <= hoy && c.FechaFin >= hoy);

            foreach (var c in contratos)
            {
                c.Inmueble = await _inmuebleRepo.GetByIdAsync(c.InmuebleId);
                c.Inquilino = await _inquilinoRepo.GetByIdAsync(c.InquilinoId);
            }

            var lista = contratos.OrderBy(c => c.FechaFin).ToList();
            var totalPaginas = (int)Math.Ceiling(lista.Count / (double)elementosPorPagina);
            var paginaActual = lista.Skip((pagina - 1) * elementosPorPagina).Take(elementosPorPagina).ToList();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TituloEspecial = "Contratos Vigentes por Fecha";
            return View("Index", paginaActual);
        }

        // GET: Contratos/Terminar/5
        public async Task<IActionResult> Terminar(int? id)
        {
            if (id == null) return NotFound();

            var contrato = await _repository.GetByIdAsync(id.Value);
            if (contrato == null) return NotFound();

            if (!contrato.Vigente)
            {
                TempData["ErrorMessage"] = "El contrato ya está finalizado.";
                return RedirectToAction(nameof(Details), new { id });
            }

            contrato.Inmueble = await _inmuebleRepo.GetByIdAsync(contrato.InmuebleId);
            contrato.Inquilino = await _inquilinoRepo.GetByIdAsync(contrato.InquilinoId);

            var hoy = DateTime.Today;
            var duracionTotal = (contrato.FechaFin - contrato.FechaInicio).TotalDays;
            var transcurrido = (hoy - contrato.FechaInicio).TotalDays;
            var restante = duracionTotal - transcurrido;

            decimal multaSugerida;
            if (transcurrido < duracionTotal / 2)
                multaSugerida = (decimal)restante / 30 * contrato.Monto * 0.5m;
            else
                multaSugerida = (decimal)restante / 30 * contrato.Monto * 0.25m;

            ViewBag.MultaSugerida = Math.Round(multaSugerida, 2);
            ViewBag.DuracionTotal = (int)duracionTotal;
            ViewBag.Transcurrido = (int)transcurrido;
            ViewBag.Restante = (int)restante;

            return View(contrato);
        }

        // POST: Contratos/Terminar/5
        [HttpPost, ActionName("Terminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TerminarConfirmed(int id, DateTime fechaTerminacion, decimal multa, bool pagada)
        {
            var contrato = await _repository.GetByIdAsync(id);
            if (contrato == null) return NotFound();

            if (!pagada)
            {
                TempData["ErrorMessage"] = "Debe confirmar el pago de la multa antes de finalizar.";
                return RedirectToAction(nameof(Terminar), new { id });
            }

            if (fechaTerminacion < contrato.FechaInicio || fechaTerminacion > contrato.FechaFin)
            {
                TempData["ErrorMessage"] = "La fecha de terminación debe estar dentro del período.";
                return RedirectToAction(nameof(Terminar), new { id });
            }

            var usuarioEmail = User?.Identity?.Name;
            Usuario? usuario = null;
            if (!string.IsNullOrEmpty(usuarioEmail))
            {
                usuario = (await _usuarioRepo.FindAsync(u => u.Email == usuarioEmail)).FirstOrDefault();
            }

            // Terminar contrato
            contrato.FechaTerminacionAnticipada = fechaTerminacion;
            contrato.Multa = multa;
            contrato.Vigente = false;
            contrato.FechaModificacion = DateTime.Now;
            if (usuario != null) contrato.UsuarioTerminacionId = usuario.Id;

            _repository.Update(contrato);
            await _repository.SaveAsync();

            // Registrar pago de la multa
            var pagosExistentes = await _pagoRepo.FindAsync(p => p.ContratoId == id);
            var nuevoPago = new Pago
            {
                ContratoId = contrato.Id,
                NumeroPago = pagosExistentes.Any() ? pagosExistentes.Max(p => p.NumeroPago) + 1 : 1,
                FechaPago = DateTime.Today,
                Importe = multa,
                Concepto = "Multa por terminación anticipada",
                UsuarioCreacionId = usuario?.Id ?? 0,
                Anulado = false
            };

            await _pagoRepo.AddAsync(nuevoPago);
            await _pagoRepo.SaveAsync();

            TempData["SuccessMessage"] = $"Contrato terminado. Multa registrada: ${multa.ToString("0.00")}";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Contratos/BuscarLibres
        public IActionResult BuscarLibres()
        {
            return View();
        }

        // POST: Contratos/BuscarLibres
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuscarLibres(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaFin <= fechaInicio)
            {
                ModelState.AddModelError("", "La fecha de fin debe ser posterior a la de inicio.");
                return View();
            }

            var inmuebles = await _inmuebleRepo.GetAllAsync();
            var contratos = await _repository.FindAsync(c => c.Vigente);

            var inmueblesOcupados = contratos
                .Where(c => (fechaInicio >= c.FechaInicio && fechaInicio <= c.FechaFin) ||
                            (fechaFin >= c.FechaInicio && fechaFin <= c.FechaFin) ||
                            (fechaInicio <= c.FechaInicio && fechaFin >= c.FechaFin))
                .Select(c => c.InmuebleId)
                .Distinct()
                .ToList();

            var disponibles = inmuebles
                .Where(i => i.Disponible && i.Activo && !inmueblesOcupados.Contains(i.Id))
                .ToList();

            foreach (var i in disponibles)
            {
                i.Propietario = await _propietarioRepo.GetByIdAsync(i.PropietarioId);
            }

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            return View("ResultadoBusquedaLibres", disponibles);
        }

        private async Task CargarListas()
        {
            var inmuebles = await _inmuebleRepo.GetAllAsync();
            var inquilinos = await _inquilinoRepo.GetAllAsync();

            ViewBag.Inmuebles = inmuebles.Where(i => i.Disponible && i.Activo).ToList();
            ViewBag.Inquilinos = inquilinos.Where(i => i.Activo).ToList();
        }
    }
}