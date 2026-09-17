using System;
using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public abstract class BaseEntity
    {
        private DateTime fechaCreacion = DateTime.Now;

        [Key]
        public int Id { get; set; }

        public DateTime FechaCreacion { get => fechaCreacion; set => fechaCreacion = value; }
        public DateTime? FechaModificacion { get; set; }
        public bool Activo { get; set; } = true;
    }
}