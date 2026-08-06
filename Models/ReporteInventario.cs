using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpticaClaridad.Models
{
    public class ReporteInventario
    {
        [Display(Name = "Categoría")]
        public int? CategoriaId { get; set; }

        [Display(Name = "Estado de Stock")]
        public string EstadoStock { get; set; } = "TODOS";

        [Display(Name = "Ordenar por")]
        public string OrdenarPor { get; set; } = "Nombre";

        [Display(Name = "Mostrar solo activos")]
        public bool SoloActivos { get; set; } = true;

        // Resultados
        public List<ProductoInventarioViewModel> Resultados { get; set; } = new List<ProductoInventarioViewModel>();
        public int TotalProductos { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public int ProductosBajoStock { get; set; }
        public int ProductosAgotados { get; set; }
    }

    public class ProductoInventarioViewModel
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string EstadoStock { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal ValorInventario { get; set; }
        public decimal GananciaUnidad { get; set; }
        public decimal MargenPorcentaje { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaUltimaVenta { get; set; }
        public int DiasSinMovimiento { get; set; }
    }
}