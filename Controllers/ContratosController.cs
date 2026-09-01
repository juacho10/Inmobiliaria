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
    public class ContratoController : Controller
    {
        private readonly IRepository<Contrato> _repository;
        private readonly IRepository<Inmueble> _inmuebleRepo;
        private readonly IRepository<Inquilino> _inquilinoRepo;
        private readonly IRepository<Usuario> _usuarioRepo;
        private readonly IRepository<Propietario> _propietarioRepo;

        public ContratoController(
            IRepository<Contrato> repository,
            IRepository<Inmueble> inmuebleRepo,
            IRepository<Inquilino> inquilinoRepo,
            IRepository<Usuario> usuarioRepo,
            IRepository<Propietario> propietarioRepo)
        {
            _repository = repository;
            _inmuebleRepo = inmuebleRepo;
            _inquilinoRepo = inquilinoRepo;
            _usuarioRepo = usuarioRepo;
            _propietarioRepo = propietarioRepo;
        }

        // GET: Contratos con paginación y filtros
        public async Task<IActionResult> Index(int pagina = 1, string search = "", bool? vigente = null)
        {
            int elementosPorPagina = 10;
            var contratos = await _repository.GetAllAsync();

            // Cargar datos relacionados
            foreach (var contrato in contratos)
            {
                if (contrato.InmuebleId > 0)
                    contrato.Inmueble = await _inmuebleRepo.GetByIdAsync(contrato.InmuebleId);
                if (contrato.InquilinoId > 0)
                    contrato.Inquilino = await _inquilinoRepo.GetByIdAsync(contrato.InquilinoId);
            }

            // Filtrar por búsqueda
            if (!string.IsNullOrEmpty(search))
            {
                contratos = contratos.Where(c =>
                    (c.Inmueble != null && c.Inmueble.Direccion.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (c.Inquilino != null && c.Inquilino.NombreCompleto.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            // Filtrar por vigencia
            if (vigente.HasValue)
            {
                contratos = contratos.Where(c => c.Vigente == vigente.Value);
            }

            // Paginación
            var totalElementos = contratos.Count();
            var totalPaginas = (int)Math.Ceiling(totalElementos / (double)elementosPorPagina);
            var contratosPagina = contratos.Skip((pagina - 1) * elementosPorPagina).Take(elementosPorPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.Search = search;
            ViewBag.Vigente = vigente;

            return View(contratosPagina);
        }

        // GET: Contratos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _repository.GetByIdAsync(id.Value);
            if (contrato == null)
            {
                return NotFound();
            }

            // Cargar datos relacionados
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
            // Validar que el inmueble no esté ocupado en las fechas seleccionadas
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
                // Asignar usuario de creación
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
                return RedirectToAction(nameof(Index));
            }

            await CargarListas();
            return View(contrato);
        }

        // GET: Contratos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _repository.GetByIdAsync(id.Value);
            if (contrato == null)
            {
                return NotFound();
            }

            await CargarListas();
            return View(contrato);
        }

        // POST: Contratos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contrato contrato)
        {
            if (id != contrato.Id)
            {
                return NotFound();
            }

            // Validar disponibilidad del inmueble (excluyendo el contrato actual)
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
                // Asignar usuario de modificación
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
                return RedirectToAction(nameof(Index));
            }

            await CargarListas();
            return View(contrato);
        }

        // GET: Contratos/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _repository.GetByIdAsync(id.Value);
            if (contrato == null)
            {
                return NotFound();
            }

            // Cargar datos relacionados
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
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Contratos/Renovar/5
        public async Task<IActionResult> Renovar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contratoOriginal = await _repository.GetByIdAsync(id.Value);
            if (contratoOriginal == null)
            {
                return NotFound();
            }

            // Crear nuevo contrato basado en el original
            var nuevoContrato = new Contrato
            {
                InmuebleId = contratoOriginal.InmuebleId,
                InquilinoId = contratoOriginal.InquilinoId,
                FechaInicio = contratoOriginal.FechaFin.AddDays(1),
                FechaFin = contratoOriginal.FechaFin.AddYears(1),
                Monto = contratoOriginal.Monto,
                Vigente = true
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
            if (ModelState.IsValid)
            {
                // Asignar usuario de creación
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

        private async Task CargarListas()
        {
            var inmuebles = await _inmuebleRepo.GetAllAsync();
            var inquilinos = await _inquilinoRepo.GetAllAsync();

            ViewBag.Inmuebles = inmuebles.Where(i => i.Disponible && i.Activo).ToList();
            ViewBag.Inquilinos = inquilinos.Where(i => i.Activo).ToList();
        }
    }
}