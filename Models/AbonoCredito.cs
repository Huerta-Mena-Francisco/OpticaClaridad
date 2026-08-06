using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // AGREGAR ESTE USING

namespace OpticaClaridad.Models
{
    public class AbonoCredito
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CreditoId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Abono")]
        public DateTime FechaAbono { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string Observaciones { get; set; }

        // CAMBIO IMPORTANTE: Agregar valor por defecto y hacerlo requerido
        [Required]
        [StringLength(100)]
        public string UsuarioId { get; set; } = "SISTEMA"; // VALOR POR DEFECTO

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relación - AGREGAR [ValidateNever]
        [ValidateNever] // ← ESTA LÍNEA ES IMPORTANTE
        public virtual Credito Credito { get; set; }

        // Propiedad calculada
        [Display(Name = "Formato Fecha")]
        public string FechaFormateada => FechaAbono.ToString("dd/MM/yyyy");
    }
}