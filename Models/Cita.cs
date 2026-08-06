using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpticaClaridad.Models
{
    public class Cita
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio")]
        [Display(Name = "Cliente/Paciente")]
        public string ClienteNombre { get; set; }

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha y Hora")]
        public DateTime FechaHora { get; set; }

        [StringLength(50)]
        [Display(Name = "Tipo de Cita")]
        public string TipoCita { get; set; } // "Examen", "Ajuste", "Entrega", "Consulta"

        [StringLength(500)]
        [Display(Name = "Motivo")]
        public string Motivo { get; set; }

        [StringLength(50)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente"; // "Pendiente", "Confirmada", "Atendida", "Cancelada"

        [StringLength(500)]
        [Display(Name = "Notas")]
        public string Notas { get; set; }

        [Display(Name = "Recordatorio Enviado")]
        public bool RecordatorioEnviado { get; set; } = false;

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // NUEVAS PROPIEDADES PARA ELIMINACIÓN
        [Display(Name = "Eliminada")]
        public bool Eliminada { get; set; } = false;

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha Eliminación")]
        public DateTime? FechaEliminacion { get; set; }

        // Propiedades calculadas (no se guardan)
        [NotMapped]
        [Display(Name = "Días Restantes")]
        public int DiasRestantes => (FechaHora.Date - DateTime.Today).Days;

        [NotMapped]
        [Display(Name = "Es Hoy")]
        public bool EsHoy => FechaHora.Date == DateTime.Today;

        [NotMapped]
        [Display(Name = "Es Pasada")]
        public bool EsPasada => FechaHora < DateTime.Now;

        [NotMapped]
        [Display(Name = "Es Futura")]
        public bool EsFutura => FechaHora > DateTime.Now;

        [NotMapped]
        [Display(Name = "Visible")]
        public bool EsVisible => !Eliminada;

        [NotMapped]
        [Display(Name = "Tiempo Eliminada")]
        public string TiempoEliminada
        {
            get
            {
                if (!FechaEliminacion.HasValue) return "No eliminada";

                var diferencia = DateTime.Now - FechaEliminacion.Value;

                if (diferencia.TotalDays >= 365)
                    return $"Hace {Math.Floor(diferencia.TotalDays / 365)} años";
                if (diferencia.TotalDays >= 30)
                    return $"Hace {Math.Floor(diferencia.TotalDays / 30)} meses";
                if (diferencia.TotalDays >= 7)
                    return $"Hace {Math.Floor(diferencia.TotalDays / 7)} semanas";
                if (diferencia.TotalDays >= 1)
                    return $"Hace {Math.Floor(diferencia.TotalDays)} días";
                if (diferencia.TotalHours >= 1)
                    return $"Hace {Math.Floor(diferencia.TotalHours)} horas";

                return "Hace poco tiempo";
            }
        }
    }
}