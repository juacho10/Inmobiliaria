using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria.Models
{
    public class Pago : BaseEntity
    {
        [Required(ErrorMessage = "El número de pago es obligatorio")]
        [Display(Name = "Número de Pago")]
        public int NumeroPago { get; set; }

        [Required(ErrorMessage = "La fecha de pago es obligatoria")]
        [Display(Name = "Fecha de Pago")]
        public DateTime FechaPago { get; set; }

        [Required(ErrorMessage = "El importe es obligatorio")]
        [Range(1, double.MaxValue, ErrorMessage = "El importe debe ser mayor a 0")]
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio")]
        [StringLength(100, ErrorMessage = "El concepto no puede tener más de 100 caracteres")]
        [Display(Name = "Concepto")]
        public string Concepto { get; set; } = "";

        [Display(Name = "Anulado")]
        public bool Anulado { get; set; } = false;

        // Relación con Contrato
        [Display(Name = "Contrato")]
        public int ContratoId { get; set; }

        [ForeignKey("ContratoId")]
        public Contrato? Contrato { get; set; }

        // Auditoría
        [Display(Name = "Usuario Creación")]
        public int UsuarioCreacionId { get; set; }

        [Display(Name = "Usuario Anulación")]
        public int? UsuarioAnulacionId { get; set; }

        [ForeignKey("UsuarioCreacionId")]
        public Usuario? UsuarioCreacion { get; set; }

        [ForeignKey("UsuarioAnulacionId")]
        public Usuario? UsuarioAnulacion { get; set; }

        // Propiedad calculada
        [Display(Name = "Estado")]
        public string Estado => Anulado ? "Anulado" : "Activo";
    }
}