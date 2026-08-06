using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpticaClaridad.Models
{
    public class DetalleVenta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VentaId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio Unitario")]
        [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal => Cantidad * PrecioUnitario;

        // Relaciones
        public virtual Venta Venta { get; set; }

        [Display(Name = "Producto")]
        public virtual Producto Producto { get; set; }
    }
}