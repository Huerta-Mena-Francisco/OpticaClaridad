using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpticaClaridad.Models
{
    public class Paciente
    {
        [Key]
        public int Id { get; set; }

        // Datos personales básicos
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string Direccion { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Registro")]
        public DateTime? FechaRegistro { get; set; } = DateTime.Today;

        public bool Activo { get; set; } = true;

        // Campos específicos de paciente
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [StringLength(10)]
        public string Sexo { get; set; } // "Masculino", "Femenino"

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string Observaciones { get; set; }

        // Relación con exámenes visuales
        public virtual ICollection<HistorialClinico> ExamenesVisuales { get; set; } = new List<HistorialClinico>();

        // Relación con ventas (si un paciente también compra)
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();

        // 🔴 NUEVA RELACIÓN: Créditos del paciente
        public virtual ICollection<Credito> Creditos { get; set; } = new List<Credito>();

        // Propiedades calculadas
        public string NombreCompleto => Nombre;

        public int Edad => FechaNacimiento.HasValue ?
            DateTime.Today.Year - FechaNacimiento.Value.Year -
            (DateTime.Today < FechaNacimiento.Value.AddYears(DateTime.Today.Year - FechaNacimiento.Value.Year) ? 1 : 0) : 0;
    }
}