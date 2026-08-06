// Models/CancelacionVenta.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace OpticaClaridad.Models
{
    public class CancelacionVenta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VentaId { get; set; }

        [Required]
        public DateTime FechaCancelacion { get; set; } = DateTime.Now;

        [Required]
        [StringLength(500)]
        public string Motivo { get; set; }

        // Relación
        public virtual Venta Venta { get; set; }
    }
}