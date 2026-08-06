using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OpticaClaridad.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
        [Display(Name = "Código")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio")]
        [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio de Venta")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }

        [Range(0, 1000000, ErrorMessage = "El precio de compra no puede ser negativo")]
        [Display(Name = "Precio de Compra")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioCompra { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio")]
        [Range(0, 10000, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Range(0, 10000, ErrorMessage = "Stock mínimo no puede ser negativo")]
        [Display(Name = "Stock Mínimo")]
        public int StockMinimo { get; set; } = 5;

        [Display(Name = "Imagen Principal")]
        [StringLength(500)]
        public string ImagenPrincipalUrl { get; set; } = string.Empty;  // ← VALOR POR DEFECTO

        [Display(Name = "Marca")]
        [StringLength(50)]
        public string Marca { get; set; }

        [Display(Name = "Modelo")]
        [StringLength(50)]
        public string Modelo { get; set; }

        [Display(Name = "Color")]
        [StringLength(30)]
        public string Color { get; set; }

        [Display(Name = "Material")]
        [StringLength(50)]
        public string Material { get; set; }

        [Display(Name = "Para")]
        [StringLength(20)]
        public string Genero { get; set; } // "Hombre", "Mujer", "Unisex", "Niño", "Niña"

        [Display(Name = "Tipo de Lente")]
        [StringLength(50)]
        public string TipoLente { get; set; } // "Graduado", "Sol", "Protección", "Contacto"

        [Display(Name = "Protección UV")]
        public bool TieneUV { get; set; } = false;

        [Display(Name = "Antireflejante")]
        public bool TieneAntireflejante { get; set; } = false;

        [Display(Name = "Fotocromático")]
        public bool EsFotocromatico { get; set; } = false;

        public bool Activo { get; set; } = true;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Today;

        [Display(Name = "Fecha de Última Actualización")]
        public DateTime? FechaActualizacion { get; set; }

        // Relaciones
        [Display(Name = "Categoría")]
        public virtual CategoriaProducto Categoria { get; set; }

        // Verifica que esta línea esté correcta (sin ";" al final de la propiedad)
        public virtual ICollection<ProductoImagen> Imagenes { get; set; } = new List<ProductoImagen>();

        // Propiedades calculadas
        [Display(Name = "Estado Stock")]
        public string EstadoStock
        {
            get
            {
                if (Stock == 0) return "AGOTADO";
                if (Stock <= StockMinimo) return "BAJO";
                return "NORMAL";
            }
        }

        [Display(Name = "Disponible")]
        public bool Disponible => Stock > 0 && Activo;

        [NotMapped]
        public string ImagenUrl => ImagenPrincipalUrl ?? (Imagenes.FirstOrDefault(i => i.EsPrincipal)?.ImagenUrl ?? "/images/productos/default.png");

        [NotMapped]
        [Display(Name = "Imágenes")]
        public IEnumerable<ProductoImagen> ImagenesActivas => Imagenes.Where(i => i.Activa).OrderBy(i => i.Orden);

        [NotMapped]
        [Display(Name = "Ganancia por Unidad")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GananciaUnidad => PrecioVenta - PrecioCompra;

        [NotMapped]
        [Display(Name = "Margen %")]
        public decimal MargenPorcentaje => PrecioCompra > 0 ? ((PrecioVenta - PrecioCompra) / PrecioCompra) * 100 : 0;

        // Métodos útiles
        public void ReducirStock(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0");

            if (cantidad > Stock)
                throw new InvalidOperationException($"Stock insuficiente. Disponible: {Stock}, Solicitado: {cantidad}");

            Stock -= cantidad;
            FechaActualizacion = DateTime.Now;
        }

        public void AumentarStock(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0");

            Stock += cantidad;
            FechaActualizacion = DateTime.Now;
        }

        public bool TieneStockSuficiente(int cantidad)
        {
            return Stock >= cantidad;
        }

        // Método para agregar imagen
        public void AgregarImagen(string imagenUrl, string descripcion = "", bool esPrincipal = false, int orden = 0)
        {
            // Si esta imagen es principal, quitar principal de otras
            if (esPrincipal)
            {
                foreach (var img in Imagenes)
                {
                    img.EsPrincipal = false;
                }
            }

            var imagen = new ProductoImagen
            {
                ProductoId = Id,
                ImagenUrl = imagenUrl,
                Descripcion = descripcion,
                EsPrincipal = esPrincipal,
                Orden = orden,
                Activa = true,
                FechaCreacion = DateTime.Now
            };

            Imagenes.Add(imagen);

            // Si es la primera imagen y no hay principal, hacerla principal
            if (!Imagenes.Any(i => i.EsPrincipal) && string.IsNullOrEmpty(ImagenPrincipalUrl))
            {
                imagen.EsPrincipal = true;
                ImagenPrincipalUrl = imagenUrl;
            }
        }

        // Método para establecer imagen principal
        public void EstablecerImagenPrincipal(int imagenId)
        {
            var imagen = Imagenes.FirstOrDefault(i => i.Id == imagenId);
            if (imagen != null)
            {
                foreach (var img in Imagenes)
                {
                    img.EsPrincipal = false;
                }
                imagen.EsPrincipal = true;
                ImagenPrincipalUrl = imagen.ImagenUrl;
                FechaActualizacion = DateTime.Now;
            }
        }
    }
}