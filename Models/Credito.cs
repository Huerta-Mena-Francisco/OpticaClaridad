using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpticaClaridad.Models
{
    public class Credito
    {
        [Key]
        public int Id { get; set; }

        [Required]
        // 🔴 CAMBIO IMPORTANTE: De ClienteId a PacienteId
        public int? PacienteId { get; set; } // Antes era ClienteId

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto Total")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoTotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Saldo Pendiente")]
        public decimal Saldo { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Crédito")]
        public DateTime FechaCredito { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "PENDIENTE";

        [StringLength(500)]
        public string Observaciones { get; set; }

        [Display(Name = "Días de Crédito")]
        [Range(1, 365, ErrorMessage = "Los días deben estar entre 1 y 365")]
        public int DiasCredito { get; set; } = 30;

        [StringLength(10)]
        [Display(Name = "Tipo de Período")]
        public string TipoPeriodo { get; set; }

        [Display(Name = "Cantidad de Período")]
        [Range(1, 52, ErrorMessage = "La cantidad debe estar entre 1 y 52")]
        public int CantidadPeriodo { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto por Período")]
        [Range(0, 1000000, ErrorMessage = "El monto debe ser mayor o igual a 0")]
        public decimal? MontoPeriodo { get; set; }

        public int? VentaId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relaciones
        public virtual Venta Venta { get; set; }

        // 🔴 CAMBIO IMPORTANTE: De Cliente a Paciente
        public virtual Paciente Paciente { get; set; } // Antes era Cliente

        public virtual ICollection<AbonoCredito> Abonos { get; set; } = new List<AbonoCredito>();

        // Propiedades calculadas...
        [Display(Name = "Abonado")]
        public decimal Abonado => MontoTotal - Saldo;

        [Display(Name = "Porcentaje Pagado")]
        public decimal PorcentajePagado => MontoTotal > 0 ? (Abonado / MontoTotal) * 100 : 0;

        [Display(Name = "Días Restantes")]
        public int? DiasRestantes => FechaVencimiento.HasValue ?
            (FechaVencimiento.Value - DateTime.Today).Days : null;

        [Display(Name = "Días Calculados")]
        public int DiasCreditoCalculado
        {
            get
            {
                if (!string.IsNullOrEmpty(TipoPeriodo) && CantidadPeriodo > 0)
                {
                    if (TipoPeriodo == "SEMANAS")
                        return CantidadPeriodo * 7;
                    else if (TipoPeriodo == "MESES")
                        return CantidadPeriodo * 30;
                }
                return DiasCredito;
            }
        }

        [Display(Name = "Total Estimado")]
        public decimal? TotalEstimado
        {
            get
            {
                if (!string.IsNullOrEmpty(TipoPeriodo) && MontoPeriodo.HasValue && CantidadPeriodo > 0)
                {
                    return MontoPeriodo.Value * CantidadPeriodo;
                }
                return null;
            }
        }

        // Métodos
        public void RegistrarAbono(decimal monto, string observaciones = "")
        {
            if (monto <= 0)
                throw new ArgumentException("El monto debe ser mayor a 0");

            if (monto > Saldo)
                throw new InvalidOperationException($"El abono no puede exceder el saldo pendiente (${Saldo})");

            Saldo -= monto;

            if (Saldo <= 0)
                Estado = "PAGADO";

            Abonos.Add(new AbonoCredito
            {
                Monto = monto,
                Observaciones = observaciones,
                FechaAbono = DateTime.Today
            });
        }

        public DateTime? CalcularFechaVencimiento()
        {
            if (!string.IsNullOrEmpty(TipoPeriodo) && CantidadPeriodo > 0)
            {
                if (TipoPeriodo == "SEMANAS")
                    return FechaCredito.AddDays(CantidadPeriodo * 7);
                else if (TipoPeriodo == "MESES")
                    return FechaCredito.AddMonths(CantidadPeriodo);
            }
            else if (DiasCredito > 0)
            {
                return FechaCredito.AddDays(DiasCredito);
            }

            return null;
        }
    }
}