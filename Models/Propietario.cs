using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public class Propietario : BaseEntity
    {
        [Required(ErrorMessage = "El DNI es obligatorio")]
        [StringLength(10, ErrorMessage = "El DNI no puede tener más de 10 caracteres")]
        public required string Dni { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
        public required string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede tener más de 50 caracteres")]
        public required string Apellido { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public required string Telefono { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public required string Email { get; set; }

        // Propiedad de navegación para Inmuebles
        public virtual ICollection<Inmueble> Inmuebles { get; set; } = new List<Inmueble>();

        public string NombreCompleto => $"{Apellido}, {Nombre}";
    }
}