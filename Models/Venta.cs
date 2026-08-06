using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpticaClaridad.Models
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Número de Venta")]
        [StringLength(20)]
        public string NumeroVenta { get; set; } = "V-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");

        // 🔴 CAMBIO: Solo PacienteId, eliminamos ClienteId
        [Display(Name = "Paciente")]
        public int? PacienteId { get; set; } // Antes teníamos ClienteId y PacienteId

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Venta")]
        public DateTime FechaVenta { get; set; } = DateTime.Today;

        [Required]
        [StringLength(20)]
        [Display(Name = "Tipo de Pago")]
        public string TipoPago { get; set; } = "Efectivo";

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000, ErrorMessage = "El total debe ser mayor a 0")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto Pagado")]
        public decimal MontoPagado { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cambio")]
        public decimal Cambio { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public string? Estado { get; set; } = "COMPLETADA";

        [Required]
        public string UsuarioId { get; set; } = "1";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relaciones
        // 🔴 CAMBIO: Solo Paciente, eliminamos Cliente
        [Display(Name = "Paciente")]
        public virtual Paciente? Paciente { get; set; } // Antes teníamos Cliente y Paciente

        public virtual ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();

        public virtual Credito? Credito { get; set; }

        // ============================================
        // PROPIEDADES CALCULADAS - ACTUALIZADAS
        // ============================================

        [NotMapped]
        [Display(Name = "Tipo de Venta")]
        public string TipoVenta => TipoPago == "Credito" ? "CRÉDITO" : "CONTADO";

        [NotMapped]
        [Display(Name = "Pendiente de Pago")]
        public decimal PendientePago => TipoPago == "Credito" ? Total - MontoPagado : 0;

        // ✅ ACTUALIZADO: Solo Paciente
        [NotMapped]
        [Display(Name = "Paciente")]
        public string? NombrePacienteDisplay
        {
            get
            {
                if (!PacienteId.HasValue)
                    return "Paciente General";
                else if (Paciente != null)
                    return Paciente.Nombre ?? $"Paciente ID: {PacienteId}";
                else
                    return $"Paciente ID: {PacienteId}";
            }
        }

        // ✅ ACTUALIZADO: Teléfono del paciente
        [NotMapped]
        public string TelefonoPacienteDisplay =>
            !PacienteId.HasValue ? string.Empty :
            Paciente?.Telefono ?? string.Empty;

        // ✅ ACTUALIZADO: Comprador display simplificado
        [NotMapped]
        public string CompradorDisplay
        {
            get
            {
                if (PacienteId.HasValue)
                    return $"Paciente: {Paciente?.Nombre ?? "N/A"}";
                else
                    return "Paciente General";
            }
        }
    }
}