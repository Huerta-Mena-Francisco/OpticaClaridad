using System;
using System.ComponentModel.DataAnnotations;

namespace OpticaClaridad.Models
{
    public class ExamenVisual
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PacienteId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha del Examen")]
        public DateTime FechaExamen { get; set; } = DateTime.Today;

        [StringLength(50)]
        [Display(Name = "OD Esférico")]
        public string ODEsferico { get; set; }

        [StringLength(50)]
        [Display(Name = "OD Cilíndrico")]
        public string ODCilindrico { get; set; }

        [StringLength(50)]
        [Display(Name = "OD Eje")]
        public string ODEje { get; set; }

        [StringLength(50)]
        [Display(Name = "OD Adición")]
        public string ODAdicion { get; set; }

        [StringLength(50)]
        [Display(Name = "OI Esférico")]
        public string OIEsferico { get; set; }

        [StringLength(50)]
        [Display(Name = "OI Cilíndrico")]
        public string OICilindrico { get; set; }

        [StringLength(50)]
        [Display(Name = "OI Eje")]
        public string OIEje { get; set; }

        [StringLength(50)]
        [Display(Name = "OI Adición")]
        public string OIAdicion { get; set; }

        [StringLength(50)]
        [Display(Name = "Distancia Pupilar")]
        public string DistanciaPupilar { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }

        // Propiedades de auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        // Relación
        public virtual Paciente Paciente { get; set; }
    }
}