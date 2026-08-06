using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpticaClaridad.Models
{
    public class ReporteVentaDetallada
    {
        [Display(Name = "Fecha Inicio")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; } = DateTime.Today.AddDays(-30);

        [Display(Name = "Fecha Fin")]
        [DataType(DataType.Date)]
        public DateTime? FechaFin { get; set; } = DateTime.Today;

        [Display(Name = "Tipo de Pago")]
        public string TipoPago { get; set; } = "TODOS";

        [Display(Name = "Categoría")]
        public int? CategoriaId { get; set; }

        [Display(Name = "Producto")]
        public string ProductoNombre { get; set; }

        [Display(Name = "Ordenar por")]
        public string OrdenarPor { get; set; } = "FechaDesc";

        // Resultados
        public List<VentaDetalleViewModel> Resultados { get; set; } = new List<VentaDetalleViewModel>();
        public decimal TotalGeneral { get; set; }
        public int TotalVentas { get; set; }
        public int TotalProductosVendidos { get; set; }
    }

    public class VentaDetalleViewModel
    {
        public int VentaId { get; set; }
        public string NumeroVenta { get; set; }
        public DateTime FechaVenta { get; set; }
        public string ClienteNombre { get; set; }
        public string TipoPago { get; set; }
        public string ProductoNombre { get; set; }
        public string CodigoProducto { get; set; }
        public string Categoria { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalVenta { get; set; }
        public string Observaciones { get; set; }
        public string Estado { get; set; }
    }
}