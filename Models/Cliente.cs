using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpticaClaridad.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

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
        public DateTime? FechaRegistro { get; set; } = DateTime.Today;

        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();

        // Método útil
        public string NombreCompleto => Nombre;
    }
}