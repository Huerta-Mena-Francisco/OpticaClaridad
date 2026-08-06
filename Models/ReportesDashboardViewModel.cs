using System;
using System.Collections.Generic;

namespace OpticaClaridad.Models
{
    public class ReportesDashboardViewModel
    {
        // ===== MÉTRICAS PRINCIPALES (KPI Cards) =====
        public decimal VentasHoy { get; set; }
        public decimal VentasEsteMes { get; set; }
        public decimal VentasMesAnterior { get; set; }
        public decimal VariacionVentas { get; set; } // % vs mes anterior
        public int TransaccionesHoy { get; set; }
        public decimal TicketPromedio { get; set; }
        public decimal MargenGananciaPromedio { get; set; }

        // ===== INVENTARIO =====
        public int ProductosStockBajo { get; set; }
        public int ProductosStockCritico { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public int ProductosSinMovimiento { get; set; } // Últimos 30 días

        // ===== VENTAS POR CATEGORÍA =====
        public List<VentasPorCategoria> VentasPorCategoria { get; set; } = new List<VentasPorCategoria>();
        public List<VentasPorProducto> TopProductosVendidos { get; set; } = new List<VentasPorProducto>();

        // ===== VENTAS POR MÉTODO DE PAGO =====
        public List<VentasPorPago> VentasPorMetodoPago { get; set; } = new List<VentasPorPago>();

        // ===== TENDENCIA VENTAS (ÚLTIMOS 7 DÍAS) =====
        public List<VentaDiaria> VentasUltimaSemana { get; set; } = new List<VentaDiaria>();

        // ===== PROPIEDADES CALCULADAS =====
        public string VariacionVentasFormateada =>
            VariacionVentas >= 0 ? $"+{VariacionVentas:0.0}%" : $"{VariacionVentas:0.0}%";

        public string ColorVariacion =>
            VariacionVentas >= 0 ? "text-success" : "text-danger";
    }

    // Clases auxiliares para las gráficas
    public class VentasPorCategoria
    {
        public string Categoria { get; set; }
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class VentasPorProducto
    {
        public string Producto { get; set; }
        public string Codigo { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
    }

    public class VentasPorPago
    {
        public string MetodoPago { get; set; }
        public decimal Total { get; set; }
        public int Transacciones { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class VentaDiaria
    {
        public string Dia { get; set; } // "Lun", "Mar", etc.
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public int Transacciones { get; set; }
    }
}