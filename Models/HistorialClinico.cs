using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpticaClaridad.Models
{
    public class HistorialClinico
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Paciente")]
        public int PacienteId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Examen")]
        public DateTime FechaExamen { get; set; } = DateTime.Today;

        [Display(Name = "Hora de Examen")]
        [DataType(DataType.Time)]
        public TimeSpan? HoraExamen { get; set; }

        [StringLength(500)]
        [Display(Name = "Motivo del Examen")]
        public string? MotivoExamen { get; set; }

        // Tipo de examen (gratis/pago)
        [Required]
        [Display(Name = "Tipo de Examen")]
        public string TipoExamen { get; set; } = "Gratis"; // Gratis, Básico, Completo, Especializado

        [Display(Name = "Costo del Examen")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? CostoExamen { get; set; }

        [Display(Name = "¿Examen Pagado?")]
        public bool ExamenPagado { get; set; } = true;

        // ============================================
        // CAMPOS MÉDICOS QUE FALTABAN
        // ============================================

        [Display(Name = "¿Tiene Diabetes?")]
        public bool TieneDiabetes { get; set; } = false;

        [StringLength(200)]
        [Display(Name = "Tratamiento Diabetes")]
        public string? DiabetesTratamiento { get; set; }

        [Display(Name = "¿Tiene Hipertensión?")]
        public bool TieneHipertension { get; set; } = false;

        [StringLength(200)]
        [Display(Name = "Tratamiento Hipertensión")]
        public string? HipertensionTratamiento { get; set; }

        [Display(Name = "¿Tiene Alergias?")]
        public bool TieneAlergias { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Otras Patologías")]
        public string? OtrasPatologias { get; set; }

        // ============================================
        // FIN CAMPOS MÉDICOS
        // ============================================

        // Datos médicos relevantes
        [Display(Name = "¿Usa Lentes Actualmente?")]
        public bool UsaLentes { get; set; } = false;

        [StringLength(100)]
        [Display(Name = "Tipo de Lentes Actuales")]
        public string? TipoLentesActuales { get; set; }

        [Display(Name = "¿Tiene Antecedentes Oculares?")]
        public bool AntecedentesOculares { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Detalle Antecedentes")]
        public string? DetalleAntecedentes { get; set; }

        // Resultados del examen - Ojo Derecho (OD)
        [Display(Name = "Esfera O.D.")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(-20.00, 20.00, ErrorMessage = "La esfera debe estar entre -20.00 y +20.00")]
        public decimal? EsferaOD { get; set; }

        [Display(Name = "Cilindro O.D.")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(-10.00, 10.00, ErrorMessage = "El cilindro debe estar entre -10.00 y +10.00")]
        public decimal? CilindroOD { get; set; }

        [Display(Name = "Eje O.D.")]
        [Range(0, 180, ErrorMessage = "El eje debe estar entre 0 y 180")]
        public int? EjeOD { get; set; }

        // Resultados del examen - Ojo Izquierdo (OI)
        [Display(Name = "Esfera O.I.")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(-20.00, 20.00, ErrorMessage = "La esfera debe estar entre -20.00 y +20.00")]
        public decimal? EsferaOI { get; set; }

        [Display(Name = "Cilindro O.I.")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(-10.00, 10.00, ErrorMessage = "El cilindro debe estar entre -10.00 y +10.00")]
        public decimal? CilindroOI { get; set; }

        [Display(Name = "Eje O.I.")]
        [Range(0, 180, ErrorMessage = "El eje debe estar entre 0 y 180")]
        public int? EjeOI { get; set; }

        // ============================================
        // NUEVOS CAMPOS PARA INDICAR "PLANO" (PL)
        // ============================================

        [Display(Name = "O.D. es Plano?")]
        public bool EsPlanoOD { get; set; } = false;

        [Display(Name = "O.I. es Plano?")]
        public bool EsPlanoOI { get; set; } = false;

        [Display(Name = "Cilindro O.D. es Plano?")]
        public bool CilindroPlanoOD { get; set; } = false;

        [Display(Name = "Cilindro O.I. es Plano?")]
        public bool CilindroPlanoOI { get; set; } = false;

        // ============================================
        // FIN NUEVOS CAMPOS
        // ============================================

        // ============================================
        // NUEVOS CAMPOS PARA GRADUACIÓN ANTERIOR
        // ============================================
        [Display(Name = "¿Tiene Graduación Anterior?")]
        public bool GraduacionAnterior { get; set; } = false;

        // Ojo Derecho (O.D) Anterior
        [Display(Name = "Esfera O.D. Anterior")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? ODEsfAnterior { get; set; }

        [Display(Name = "Cilindro O.D. Anterior")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? ODCilAnterior { get; set; }

        [Display(Name = "Eje O.D. Anterior")]
        public int? ODEjeAnterior { get; set; }

        [Display(Name = "Adición O.D. Anterior")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? ODAddAnterior { get; set; }

        // Ojo Izquierdo (O.I) Anterior
        [Display(Name = "Esfera O.I. Anterior")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? OIEsfAnterior { get; set; }

        [Display(Name = "Cilindro O.I. Anterior")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? OICilAnterior { get; set; }

        [Display(Name = "Eje O.I. Anterior")]
        public int? OIEjeAnterior { get; set; }

        [Display(Name = "Adición O.I. Anterior")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? OIAddAnterior { get; set; }
        // ============================================
        // FIN NUEVOS CAMPOS GRADUACIÓN ANTERIOR
        // ============================================

        // Para lentes multifocales
        [Display(Name = "Adición")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0.00, 4.00, ErrorMessage = "La adición debe estar entre 0.00 y 4.00")]
        public decimal? Adicion { get; set; }

        // Distancia pupilar
        [Display(Name = "Distancia Pupilar (mm)")]
        [Column(TypeName = "decimal(5,1)")]
        [Range(50.0, 80.0, ErrorMessage = "La DP debe estar entre 50 y 80 mm")]
        public decimal? DistanciaPupilar { get; set; }

        // Agudeza visual
        [StringLength(10)]
        [Display(Name = "A.V. O.D. (Lejos)")]
        public string? AgudezaVisualODLejos { get; set; }

        [StringLength(10)]
        [Display(Name = "A.V. O.I. (Lejos)")]
        public string? AgudezaVisualOILejos { get; set; }

        [StringLength(10)]
        [Display(Name = "A.V. O.D. (Cerca)")]
        public string? AgudezaVisualODCerca { get; set; }

        [StringLength(10)]
        [Display(Name = "A.V. O.I. (Cerca)")]
        public string? AgudezaVisualOICerca { get; set; }

        // Observaciones y recomendaciones
        [StringLength(1000)]
        [Display(Name = "Observaciones del Examen")]
        public string? ObservacionesExamen { get; set; }

        [StringLength(1000)]
        [Display(Name = "Recomendaciones")]
        public string? Recomendaciones { get; set; }

        // Para seguimiento
        [Display(Name = "¿Necesita Seguimiento?")]
        public bool NecesitaSeguimiento { get; set; } = false;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha Próximo Control")]
        public DateTime? FechaProximoControl { get; set; }

        // Interés en compra (sin compromiso)
        [Display(Name = "¿Mostró Interés en Compra?")]
        public bool InteresEnCompra { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Observaciones Interés")]
        public string? ObservacionesInteres { get; set; }

        // Auditoría
        [Required]
        [StringLength(100)]
        [Display(Name = "Optometrista")]
        public string Optometrista { get; set; } = "Opt. Principal";

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        // Navegación
        [ForeignKey("PacienteId")]
        public virtual Paciente? Paciente { get; set; }

        // ============================================
        // PROPIEDADES CALCULADAS ACTUALIZADAS
        // ============================================

        [NotMapped]
        [Display(Name = "Graduación O.D.")]
        public string GraduacionODFormateada => FormatearGraduacion(
            EsferaOD,
            EsPlanoOD,
            CilindroOD,
            CilindroPlanoOD,
            EjeOD,
            Adicion);

        [NotMapped]
        [Display(Name = "Graduación O.I.")]
        public string GraduacionOIFormateada => FormatearGraduacion(
            EsferaOI,
            EsPlanoOI,
            CilindroOI,
            CilindroPlanoOI,
            EjeOI,
            Adicion);

        [NotMapped]
        [Display(Name = "Graduación O.D. Anterior")]
        public string GraduacionODAnteriorFormateada => FormatearGraduacionAnterior(
            ODEsfAnterior,
            ODCilAnterior,
            ODEjeAnterior,
            ODAddAnterior);

        [NotMapped]
        [Display(Name = "Graduación O.I. Anterior")]
        public string GraduacionOIAnteriorFormateada => FormatearGraduacionAnterior(
            OIEsfAnterior,
            OICilAnterior,
            OIEjeAnterior,
            OIAddAnterior);

        [NotMapped]
        [Display(Name = "Tipo de Lente Sugerido")]
        public string TipoLenteSugerido
        {
            get
            {
                if (Adicion.HasValue && Adicion.Value > 0)
                    return "Lente Progresivo";

                if ((EsferaOD.HasValue && Math.Abs(EsferaOD.Value) >= 3.00m) ||
                    (EsferaOI.HasValue && Math.Abs(EsferaOI.Value) >= 3.00m))
                    return "Lente Alto Índice";

                return "Lente Monofocal";
            }
        }

        [NotMapped]
        [Display(Name = "Costo Formateado")]
        public string CostoFormateado => CostoExamen.HasValue ? $"${CostoExamen.Value:N2}" : "Gratis";

        // Método privado para formatear graduación (ACTUALIZADO CON PL)
        private string FormatearGraduacion(
            decimal? esfera,
            bool esPlano,
            decimal? cilindro,
            bool cilindroPlano,
            int? eje,
            decimal? adicion)
        {
            string resultado;

            // Manejar esfera (PL)
            if (esPlano)
            {
                resultado = "PL";
            }
            else if (esfera.HasValue)
            {
                resultado = $"{esfera.Value:+0.00;-0.00}";
            }
            else
            {
                return "---";
            }

            // Manejar cilindro (puede ser plano también)
            if (cilindroPlano)
            {
                resultado += " / Cil: PL";
            }
            else if (cilindro.HasValue && cilindro.Value != 0)
            {
                resultado += $" / {cilindro.Value:+0.00;-0.00} x {eje?.ToString() ?? "0"}";
            }

            // Manejar adición
            if (adicion.HasValue && adicion.Value > 0)
            {
                resultado += $" Add {adicion.Value:0.00}";
            }

            return resultado;
        }

        // Nuevo método para formatear graduación anterior (sin PL)
        private string FormatearGraduacionAnterior(
            decimal? esfera,
            decimal? cilindro,
            int? eje,
            decimal? adicion)
        {
            if (!esfera.HasValue && !cilindro.HasValue && !adicion.HasValue)
                return "---";

            string resultado = esfera.HasValue ? $"{esfera.Value:+0.00;-0.00}" : "---";

            if (cilindro.HasValue && cilindro.Value != 0)
            {
                resultado += $" / {cilindro.Value:+0.00;-0.00} x {eje?.ToString() ?? "0"}";
            }

            if (adicion.HasValue && adicion.Value > 0)
            {
                resultado += $" Add {adicion.Value:0.00}";
            }

            return resultado;
        }
    }
}