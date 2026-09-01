using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public class Inmueble : BaseEntity
    {
        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(200, ErrorMessage = "La dirección no puede tener más de 200 caracteres.")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = "";

        [Required(ErrorMessage = "El propietario es obligatorio.")]
        [Display(Name = "Propietario")]
        public int PropietarioId { get; set; }

        [ForeignKey("PropietarioId")]
        public Propietario? Propietario { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio.")]
        [StringLength(50, ErrorMessage = "El tipo no puede tener más de 50 caracteres.")]
        [Display(Name = "Tipo")]
        public string Tipo { get; set; } = "";

        [Required(ErrorMessage = "El uso es obligatorio.")]
        [StringLength(20, ErrorMessage = "El uso no puede tener más de 20 caracteres.")]
        [Display(Name = "Uso")]
        public string Uso { get; set; } = "";

        [Required(ErrorMessage = "La cantidad de ambientes es obligatoria.")]
        [Range(1, 50, ErrorMessage = "La cantidad de ambientes debe estar entre 1 y 50.")]
        [Display(Name = "Ambientes")]
        public int Ambientes { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(1, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        [Display(Name = "Precio Mensual")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [StringLength(100, ErrorMessage = "Las coordenadas no pueden tener más de 100 caracteres.")]
        [Display(Name = "Coordenadas")]
        public string? Coordenadas { get; set; }

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; } = true;

        // Propiedad de navegación para Contratos
        public virtual ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    }
}