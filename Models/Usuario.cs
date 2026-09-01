using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public class Usuario : BaseEntity
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede tener más de 50 caracteres")]
        public string Apellido { get; set; } = "";

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = "";

        public string Avatar { get; set; } = "";

        [Required]
        public string Rol { get; set; } = "Empleado";

        public string NombreCompleto => $"{Apellido}, {Nombre}";

        // ✅ AGREGAR ESTA PROPIEDAD FALTANTE
        public virtual ICollection<Contrato> ContratosTerminados { get; set; } = new List<Contrato>();

        // Otras propiedades de navegación
        public virtual ICollection<Contrato> ContratosCreados { get; set; } = new List<Contrato>();
        public virtual ICollection<Contrato> ContratosModificados { get; set; } = new List<Contrato>();
        public virtual ICollection<Pago> PagosCreados { get; set; } = new List<Pago>();
        public virtual ICollection<Pago> PagosAnulados { get; set; } = new List<Pago>();
    }
}