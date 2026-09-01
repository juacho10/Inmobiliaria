using System;
using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow; // ✅ Cambiado a UTC
        public DateTime? FechaModificacion { get; set; }
        public bool Activo { get; set; } = true;
    }
}