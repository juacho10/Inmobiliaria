using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Inmobiliaria.Models;
using Inmobiliaria.Repository;

namespace Inmobiliaria.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class InmueblesApiController : ControllerBase
    {
        private readonly IRepository<Inmueble> _repository;

        public InmueblesApiController(IRepository<Inmueble> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
        {
            var (items, total) = await _repository.GetPagedAsync(
                page, pageSize, null,
                q => q.OrderBy(i => i.Direccion));

            return Ok(new
            {
                items = items.Select(i => new
                {
                    i.Id,
                    i.Direccion,
                    i.Tipo,
                    i.Uso,
                    i.Ambientes,
                    i.Precio,
                    i.Disponible,
                    i.ImagenPortada
                }),
                total,
                page,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var inmueble = await _repository.GetByIdAsync(id);
            if (inmueble == null) return NotFound(new { message = "Inmueble no encontrado" });
            return Ok(inmueble);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([FromBody] Inmueble inmueble)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _repository.AddAsync(inmueble);
            await _repository.SaveAsync();
            return CreatedAtAction(nameof(GetById), new { id = inmueble.Id }, inmueble);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, [FromBody] Inmueble inmueble)
        {
            if (id != inmueble.Id) return BadRequest(new { message = "ID no coincide" });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            inmueble.FechaModificacion = DateTime.Now;
            _repository.Update(inmueble);
            await _repository.SaveAsync();
            return Ok(new { message = "Actualizado", inmueble });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var inmueble = await _repository.GetByIdAsync(id);
            if (inmueble == null) return NotFound();
            _repository.Remove(inmueble);
            await _repository.SaveAsync();
            return Ok(new { message = "Eliminado" });
        }
    }
}