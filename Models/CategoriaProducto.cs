using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpticaClaridad.Models
{
    public class CategoriaProducto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Nombre de Categoría")]
        public string Nombre { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        // Relación con productos
        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

        // Método útil
        public override string ToString() => Nombre;
    }
}