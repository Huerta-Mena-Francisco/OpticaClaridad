using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpticaClaridad.Models
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [StringLength(50)]
        [Display(Name = "Contacto")]
        public string Contacto { get; set; }

        [StringLength(500)]
        [Display(Name = "Notas")]
        public string Notas { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [DataType(DataType.Date)]
        [Display(Name = "Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Today;

        // Propiedad calculada (no se guarda)
        [NotMapped]
        [Display(Name = "Estado")]
        public string Estado => Activo ? "Activo" : "Inactivo";
    }
}