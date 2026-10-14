using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Inmobiliaria.Models;
using Inmobiliaria.Repository;

namespace Inmobiliaria.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRepository<Usuario> _repository;
        private readonly IConfiguration _config;

        public AuthController(IRepository<Usuario> repository, IConfiguration config)
        {
            _repository = repository;
            _config = config;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email y contraseña son obligatorios" });

            var usuarios = await _repository.FindAsync(u => u.Email == request.Email && u.Activo);
            var usuario = usuarios.FirstOrDefault();

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.Password))
                return Unauthorized(new { message = "Credenciales inválidas" });

            var token = GenerarToken(usuario);
            return Ok(new
            {
                token,
                usuario = new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Email,
                    usuario.Rol,
                    usuario.Avatar
                }
            });
        }

        private string GenerarToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Jwt:Key"]!));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("FullName", usuario.NombreCompleto),
                new Claim("Avatar", usuario.Avatar ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}