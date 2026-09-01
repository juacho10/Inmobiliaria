using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public class Contrato : BaseEntity // ✅ HEREDA CORRECTAMENTE
    {
        [Required(ErrorMessage = "El inmueble es obligatorio.")]
        [Display(Name = "Inmueble")]
        public int InmuebleId { get; set; }

        [ForeignKey("InmuebleId")]
        public Inmueble? Inmueble { get; set; }

        [Required(ErrorMessage = "El inquilino es obligatorio.")]
        [Display(Name = "Inquilino")]
        public int InquilinoId { get; set; }

        [ForeignKey("InquilinoId")]
        public Inquilino? Inquilino { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [Display(Name = "Fecha de Fin")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.")]
        [Display(Name = "Monto Mensual")]
        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }

        [Display(Name = "Vigente")]
        public bool Vigente { get; set; } = true;

        [Display(Name = "Fecha de Terminación Anticipada")]
        [DataType(DataType.Date)]
        public DateTime? FechaTerminacionAnticipada { get; set; }

        [Display(Name = "Multa")]
        [DataType(DataType.Currency)]
        public decimal? Multa { get; set; }

        [Display(Name = "Usuario de Creación")]
        public int? UsuarioCreacionId { get; set; }

        [Display(Name = "Usuario de Modificación")]
        public int? UsuarioModificacionId { get; set; }

        [Display(Name = "Usuario de Terminación")]
        public int? UsuarioTerminacionId { get; set; }

        // ✅ AGREGAR estas propiedades como opcionales
        [ForeignKey("UsuarioCreacionId")]
        public Usuario? UsuarioCreacion { get; set; }

        [ForeignKey("UsuarioModificacionId")]
        public Usuario? UsuarioModificacion { get; set; }

        [ForeignKey("UsuarioTerminacionId")]
        public Usuario? UsuarioTerminacion { get; set; }

        // ✅ AGREGAR COLECCIÓN DE PAGOS
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        // Propiedad calculada para verificar si está vigente por fecha
        [NotMapped]
        public bool EstaVigentePorFecha => Vigente && DateTime.Today >= FechaInicio && DateTime.Today <= FechaFin;
    }
}