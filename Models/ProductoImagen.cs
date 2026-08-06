using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpticaClaridad.Models
{
    public class ProductoImagen
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "URL de la Imagen")]
        public string ImagenUrl { get; set; }

        [StringLength(200)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Orden de Visualización")]
        public int Orden { get; set; } = 0;

        [Display(Name = "Es Principal")]
        public bool EsPrincipal { get; set; } = false;

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relación
        public virtual Producto Producto { get; set; }

        // Propiedades útiles
        [NotMapped]
        [Display(Name = "Miniatura")]
        public string MiniaturaUrl => ImagenUrl.Replace("/images/", "/thumbnails/");
    }
}