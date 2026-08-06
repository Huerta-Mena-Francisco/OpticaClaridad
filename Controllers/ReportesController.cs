using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using OpticaClaridad.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpticaClaridad.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reportes/Dashboard
        public IActionResult Dashboard()
        {
            var fechaActual = DateTime.Today;
            var fechaInicioMes = new DateTime(fechaActual.Year, fechaActual.Month, 1);
            var fechaInicioMesAnterior = fechaInicioMes.AddMonths(-1);
            var fechaFinMesAnterior = fechaInicioMes.AddDays(-1);

            var viewModel = new ReportesDashboardViewModel();

            try
            {
                // ===== VENTAS HOY =====
                var ventasHoy = _context.Ventas
                    .Where(v => v.FechaVenta.Date == fechaActual.Date && v.Estado == "COMPLETADA")
                    .ToList();

                viewModel.VentasHoy = ventasHoy.Sum(v => v.Total);
                viewModel.TransaccionesHoy = ventasHoy.Count;

                // ===== VENTAS ESTE MES =====
                var ventasEsteMes = _context.Ventas
                    .Where(v => v.FechaVenta >= fechaInicioMes &&
                                v.FechaVenta <= fechaActual &&
                                v.Estado == "COMPLETADA")
                    .ToList();

                viewModel.VentasEsteMes = ventasEsteMes.Sum(v => v.Total);

                // ===== VENTAS MES ANTERIOR =====
                var ventasMesAnterior = _context.Ventas
                    .Where(v => v.FechaVenta >= fechaInicioMesAnterior &&
                                v.FechaVenta <= fechaFinMesAnterior &&
                                v.Estado == "COMPLETADA")
                    .ToList();

                viewModel.VentasMesAnterior = ventasMesAnterior.Sum(v => v.Total);

                // ===== VARIACIÓN =====
                viewModel.VariacionVentas = viewModel.VentasMesAnterior > 0 ?
                    ((viewModel.VentasEsteMes - viewModel.VentasMesAnterior) / viewModel.VentasMesAnterior) * 100 : 0;

                // ===== TICKET PROMEDIO =====
                viewModel.TicketPromedio = ventasEsteMes.Any() ?
                    ventasEsteMes.Average(v => v.Total) : 0;

                // ===== INVENTARIO =====
                var productos = _context.Productos.ToList();
                viewModel.ProductosStockBajo = productos.Count(p => p.Stock <= p.StockMinimo && p.Stock > 0);
                viewModel.ProductosStockCritico = productos.Count(p => p.Stock == 0);
                viewModel.ValorTotalInventario = productos.Sum(p => p.Stock * p.PrecioCompra);

                // ===== PRODUCTOS SIN MOVIMIENTO (ÚLTIMOS 30 DÍAS) =====
                var fechaLimite = fechaActual.AddDays(-30);
                var productosIdsConVentas = _context.DetallesVenta
                    .Include(d => d.Venta)
                    .Where(d => d.Venta.FechaVenta >= fechaLimite)
                    .Select(d => d.ProductoId)
                    .Distinct()
                    .ToList();

                viewModel.ProductosSinMovimiento = productos
                    .Count(p => !productosIdsConVentas.Contains(p.Id));

                // ===== VENTAS POR CATEGORÍA =====
                var detallesVentaMes = _context.DetallesVenta
                    .Include(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                    .Include(d => d.Venta)
                    .Where(d => d.Venta.FechaVenta >= fechaInicioMes && d.Venta.Estado == "COMPLETADA")
                    .ToList()
                    .GroupBy(d => d.Producto.Categoria.Nombre)
                    .Select(g => new VentasPorCategoria
                    {
                        Categoria = g.Key,
                        Total = g.Sum(d => d.Subtotal),
                        Cantidad = g.Sum(d => d.Cantidad)
                    })
                    .ToList();

                var totalCategorias = detallesVentaMes.Sum(v => v.Total);
                foreach (var item in detallesVentaMes)
                {
                    item.Porcentaje = totalCategorias > 0 ? (item.Total / totalCategorias) * 100 : 0;
                }

                viewModel.VentasPorCategoria = detallesVentaMes;

                // ===== TOP PRODUCTOS VENDIDOS =====
                viewModel.TopProductosVendidos = _context.DetallesVenta
                    .Include(d => d.Producto)
                    .Include(d => d.Venta)
                    .Where(d => d.Venta.FechaVenta >= fechaInicioMes && d.Venta.Estado == "COMPLETADA")
                    .ToList()
                    .GroupBy(d => new { d.ProductoId, d.Producto.Nombre, d.Producto.Codigo })
                    .Select(g => new VentasPorProducto
                    {
                        Producto = g.Key.Nombre,
                        Codigo = g.Key.Codigo,
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        TotalVentas = g.Sum(d => d.Subtotal)
                    })
                    .OrderByDescending(x => x.CantidadVendida)
                    .Take(10)
                    .ToList();

                // ===== VENTAS POR MÉTODO DE PAGO =====
                var ventasPorPagoList = _context.Ventas
                    .Where(v => v.FechaVenta >= fechaInicioMes && v.Estado == "COMPLETADA")
                    .ToList()
                    .GroupBy(v => v.TipoPago)
                    .Select(g => new VentasPorPago
                    {
                        MetodoPago = g.Key,
                        Total = g.Sum(v => v.Total),
                        Transacciones = g.Count()
                    })
                    .ToList();

                var totalPagos = ventasPorPagoList.Sum(v => v.Total);
                foreach (var item in ventasPorPagoList)
                {
                    item.Porcentaje = totalPagos > 0 ? (item.Total / totalPagos) * 100 : 0;
                }

                viewModel.VentasPorMetodoPago = ventasPorPagoList;

                // ===== TENDENCIA ÚLTIMA SEMANA =====
                var ventasSemana = new List<VentaDiaria>();
                for (int i = 6; i >= 0; i--)
                {
                    var fecha = fechaActual.AddDays(-i);
                    var dia = fecha.ToString("ddd");

                    var ventasDelDia = _context.Ventas
                        .Where(v => v.FechaVenta.Date == fecha.Date && v.Estado == "COMPLETADA")
                        .ToList();

                    ventasSemana.Add(new VentaDiaria
                    {
                        Dia = dia,
                        Fecha = fecha,
                        Total = ventasDelDia.Sum(v => v.Total),
                        Transacciones = ventasDelDia.Count
                    });
                }

                viewModel.VentasUltimaSemana = ventasSemana;

                // ===== MARGEN DE GANANCIA PROMEDIO =====
                var detalleVentasMes = _context.DetallesVenta
                    .Include(d => d.Venta)
                    .Include(d => d.Producto)
                    .Where(d => d.Venta.FechaVenta >= fechaInicioMes && d.Venta.Estado == "COMPLETADA")
                    .ToList();

                if (detalleVentasMes.Any())
                {
                    var gananciaTotal = detalleVentasMes.Sum(d => d.Cantidad * (d.PrecioUnitario - d.Producto.PrecioCompra));
                    var ventasTotales = detalleVentasMes.Sum(d => d.Subtotal);
                    viewModel.MargenGananciaPromedio = ventasTotales > 0 ? (gananciaTotal / ventasTotales) * 100 : 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Dashboard: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al generar el reporte. Intente nuevamente.";
            }

            return View(viewModel);
        }

        // GET: Reportes/VentasDetallada - ACTUALIZADO (solo Paciente)
        public IActionResult VentasDetallada(ReporteVentaDetallada filtro)
        {
            // Valores por defecto
            if (!filtro.FechaInicio.HasValue)
                filtro.FechaInicio = DateTime.Today.AddDays(-30);

            if (!filtro.FechaFin.HasValue)
                filtro.FechaFin = DateTime.Today;

            // Cargar ventas con detalles
            var ventas = _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                .Include(v => v.Paciente)  // 🔴 SOLO Paciente, eliminado Cliente
                .Where(v => v.Estado == "COMPLETADA" &&
                           v.FechaVenta.Date >= filtro.FechaInicio.Value.Date &&
                           v.FechaVenta.Date <= filtro.FechaFin.Value.Date)
                .AsEnumerable()
                .ToList();

            var resultados = new List<VentaDetalleViewModel>();

            // Procesar cada venta y sus detalles
            foreach (var venta in ventas)
            {
                // 🔴 ACTUALIZADO: Solo paciente
                string nombreComprador = venta.Paciente != null ?
                    $"Paciente: {venta.Paciente.Nombre}" :
                    "Cliente General";

                // Crear un detalle por cada producto
                foreach (var detalle in venta.Detalles)
                {
                    resultados.Add(new VentaDetalleViewModel
                    {
                        VentaId = venta.Id,
                        NumeroVenta = venta.NumeroVenta,
                        FechaVenta = venta.FechaVenta,
                        ClienteNombre = nombreComprador,  // 🔴 Mantenemos el nombre de la propiedad por compatibilidad
                        TipoPago = venta.TipoPago,
                        ProductoNombre = detalle.Producto?.Nombre ?? "N/A",
                        CodigoProducto = detalle.Producto?.Codigo ?? "N/A",
                        Categoria = detalle.Producto?.Categoria?.Nombre ?? "Sin categoría",
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario,
                        Subtotal = detalle.Subtotal,
                        TotalVenta = venta.Total,
                        Observaciones = venta.Observaciones ?? "",
                        Estado = venta.Estado ?? "COMPLETADA"
                    });
                }
            }

            // Aplicar filtros adicionales
            if (filtro.CategoriaId.HasValue && filtro.CategoriaId.Value > 0)
            {
                var categoria = _context.CategoriasProducto
                    .FirstOrDefault(c => c.Id == filtro.CategoriaId.Value);

                if (categoria != null)
                    resultados = resultados.Where(d => d.Categoria == categoria.Nombre).ToList();
            }

            // Aplicar ordenamiento
            switch (filtro.OrdenarPor)
            {
                case "FechaAsc":
                    resultados = resultados.OrderBy(d => d.FechaVenta).ToList();
                    break;
                case "TotalDesc":
                    resultados = resultados.OrderByDescending(d => d.Subtotal).ToList();
                    break;
                case "TotalAsc":
                    resultados = resultados.OrderBy(d => d.Subtotal).ToList();
                    break;
                case "CantidadDesc":
                    resultados = resultados.OrderByDescending(d => d.Cantidad).ToList();
                    break;
                default: // "FechaDesc"
                    resultados = resultados.OrderByDescending(d => d.FechaVenta).ToList();
                    break;
            }

            // Calcular totales
            filtro.Resultados = resultados;
            filtro.TotalGeneral = resultados.Sum(d => d.Subtotal);
            filtro.TotalVentas = resultados.Select(d => d.VentaId).Distinct().Count();
            filtro.TotalProductosVendidos = resultados.Sum(d => d.Cantidad);

            // Pasar categorías para el dropdown
            ViewBag.Categorias = _context.CategoriasProducto
                .OrderBy(c => c.Nombre)
                .ToList();

            return View(filtro);
        }

        // GET: Reportes/Inventario - SIN CAMBIOS
        public IActionResult Inventario(ReporteInventario filtro)
        {
            // Cargar productos
            var productos = _context.Productos
                .Include(p => p.Categoria)
                .AsEnumerable()
                .ToList();

            // Aplicar filtros
            if (filtro.CategoriaId.HasValue)
                productos = productos.Where(p => p.CategoriaId == filtro.CategoriaId.Value).ToList();

            if (filtro.EstadoStock != "TODOS")
                productos = productos.Where(p => p.EstadoStock == filtro.EstadoStock).ToList();

            if (filtro.SoloActivos)
                productos = productos.Where(p => p.Activo).ToList();

            // Obtener fechas de última venta
            var productosIds = productos.Select(p => p.Id).ToList();
            var ventasPorProducto = _context.DetallesVenta
                .Include(d => d.Venta)
                .Where(d => productosIds.Contains(d.ProductoId) && d.Venta.Estado == "COMPLETADA")
                .ToList()
                .GroupBy(d => d.ProductoId)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    UltimaVenta = g.Max(d => d.Venta.FechaVenta)
                })
                .ToList();

            var fechaActual = DateTime.Today;

            // Mapear a ViewModel
            var resultados = productos.Select(p => new ProductoInventarioViewModel
            {
                ProductoId = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Categoria = p.Categoria?.Nombre ?? "Sin categoría",
                Marca = p.Marca,
                Modelo = p.Modelo,
                Stock = p.Stock,
                StockMinimo = p.StockMinimo,
                EstadoStock = p.EstadoStock,
                PrecioCompra = p.PrecioCompra,
                PrecioVenta = p.PrecioVenta,
                ValorInventario = p.Stock * p.PrecioCompra,
                GananciaUnidad = p.GananciaUnidad,
                MargenPorcentaje = p.MargenPorcentaje,
                Activo = p.Activo,
                FechaUltimaVenta = ventasPorProducto.FirstOrDefault(u => u.ProductoId == p.Id)?.UltimaVenta,
                DiasSinMovimiento = ventasPorProducto.Any(u => u.ProductoId == p.Id) ?
                    (fechaActual - ventasPorProducto.First(u => u.ProductoId == p.Id).UltimaVenta).Days :
                    (fechaActual - p.FechaCreacion).Days
            }).ToList();

            // Aplicar ordenamiento
            switch (filtro.OrdenarPor)
            {
                case "StockAsc":
                    resultados = resultados.OrderBy(r => r.Stock).ToList();
                    break;
                case "StockDesc":
                    resultados = resultados.OrderByDescending(r => r.Stock).ToList();
                    break;
                case "ValorDesc":
                    resultados = resultados.OrderByDescending(r => r.ValorInventario).ToList();
                    break;
                case "DiasSinMovimientoDesc":
                    resultados = resultados.OrderByDescending(r => r.DiasSinMovimiento).ToList();
                    break;
                case "Nombre":
                default:
                    resultados = resultados.OrderBy(r => r.Nombre).ToList();
                    break;
            }

            // Calcular estadísticas
            filtro.Resultados = resultados;
            filtro.TotalProductos = resultados.Count;
            filtro.ValorTotalInventario = resultados.Sum(r => r.ValorInventario);
            filtro.ProductosBajoStock = resultados.Count(r => r.EstadoStock == "BAJO");
            filtro.ProductosAgotados = resultados.Count(r => r.EstadoStock == "AGOTADO");

            // Pasar categorías para el dropdown
            ViewBag.Categorias = _context.CategoriasProducto
                .OrderBy(c => c.Nombre)
                .ToList();

            return View(filtro);
        }

        // GET: Reportes/GetDashboardStats
        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var hoy = DateTime.Today;
                var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

                // Ventas hoy
                var ventasHoy = await _context.Ventas
                    .Where(v => v.FechaVenta.Date == hoy && v.Estado == "COMPLETADA")
                    .CountAsync();

                // Total productos activos
                var totalProductos = await _context.Productos
                    .Where(p => p.Activo)
                    .CountAsync();

                // Total pacientes
                var totalPacientes = await _context.Pacientes
                    .CountAsync();

                // Ingresos del mes
                var ingresosMes = await _context.Ventas
                    .Where(v => v.FechaVenta >= inicioMes && v.Estado == "COMPLETADA")
                    .SumAsync(v => v.Total);

                return Ok(new
                {
                    ventasHoy,
                    totalProductos,
                    totalClientes = totalPacientes,
                    ingresosMes
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    ventasHoy = 0,
                    totalProductos = 0,
                    totalClientes = 0,
                    ingresosMes = 0
                });
            }
        }

        // GET: Reportes/ExportarDashboardCSV
        public IActionResult ExportarDashboardCSV()
        {
            var fechaActual = DateTime.Today;
            var fechaInicioMes = new DateTime(fechaActual.Year, fechaActual.Month, 1);

            try
            {
                // ===== 1. VENTAS POR CATEGORÍA =====
                var ventasCategoria = _context.DetallesVenta
                    .Include(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                    .Include(d => d.Venta)
                    .Where(d => d.Venta.FechaVenta >= fechaInicioMes && d.Venta.Estado == "COMPLETADA")
                    .ToList()
                    .GroupBy(d => d.Producto.Categoria.Nombre)
                    .Select(g => new
                    {
                        Categoria = g.Key,
                        TotalVentas = g.Sum(d => d.Subtotal),
                        Cantidad = g.Sum(d => d.Cantidad)
                    })
                    .OrderByDescending(x => x.TotalVentas)
                    .ToList();

                // ===== 2. TOP PRODUCTOS =====
                var topProductos = _context.DetallesVenta
                    .Include(d => d.Producto)
                    .Include(d => d.Venta)
                    .Where(d => d.Venta.FechaVenta >= fechaInicioMes && d.Venta.Estado == "COMPLETADA")
                    .ToList()
                    .GroupBy(d => new { d.ProductoId, d.Producto.Nombre, d.Producto.Codigo })
                    .Select(g => new
                    {
                        Producto = g.Key.Nombre,
                        Codigo = g.Key.Codigo,
                        Cantidad = g.Sum(d => d.Cantidad),
                        Total = g.Sum(d => d.Subtotal)
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .Take(10)
                    .ToList();

                // ===== 3. MÉTODOS DE PAGO =====
                var metodosPago = _context.Ventas
                    .Where(v => v.FechaVenta >= fechaInicioMes && v.Estado == "COMPLETADA")
                    .ToList()
                    .GroupBy(v => v.TipoPago)
                    .Select(g => new
                    {
                        Metodo = g.Key,
                        Total = g.Sum(v => v.Total),
                        Transacciones = g.Count()
                    })
                    .ToList();

                // ===== 4. TENDENCIA SEMANAL =====
                var tendenciaSemanal = new List<object>();
                for (int i = 6; i >= 0; i--)
                {
                    var fecha = fechaActual.AddDays(-i);
                    var ventasDelDia = _context.Ventas
                        .Where(v => v.FechaVenta.Date == fecha.Date && v.Estado == "COMPLETADA")
                        .ToList();

                    tendenciaSemanal.Add(new
                    {
                        Dia = fecha.ToString("dd/MM"),
                        DiaSemana = fecha.ToString("dddd"),
                        Total = ventasDelDia.Sum(v => v.Total),
                        Transacciones = ventasDelDia.Count
                    });
                }

                // ===== 5. INVENTARIO =====
                var inventario = _context.Productos
                    .Select(p => new
                    {
                        p.Nombre,
                        p.Codigo,
                        p.Stock,
                        p.StockMinimo,
                        Estado = p.Stock <= 0 ? "AGOTADO" : (p.Stock <= p.StockMinimo ? "BAJO" : "NORMAL"),
                        p.PrecioCompra,
                        ValorInventario = p.Stock * p.PrecioCompra
                    })
                    .OrderBy(p => p.Nombre)
                    .ToList();

                // ===== CONSTRUIR EL CSV =====
                var csv = new System.Text.StringBuilder();

                // HEADER: Fecha de generación
                csv.AppendLine($"Reporte generado:,{DateTime.Now:dd/MM/yyyy HH:mm}");
                csv.AppendLine();

                // SECCIÓN 1: Ventas por Categoría
                csv.AppendLine("--- VENTAS POR CATEGORÍA ---");
                csv.AppendLine("Categoría,Total Ventas,Cantidad Vendida");
                foreach (var item in ventasCategoria)
                {
                    csv.AppendLine($"\"{item.Categoria}\",{item.TotalVentas.ToString("F2").Replace(",", "")},{item.Cantidad}");
                }
                csv.AppendLine();

                // SECCIÓN 2: Top 10 Productos
                csv.AppendLine("--- TOP 10 PRODUCTOS MÁS VENDIDOS ---");
                csv.AppendLine("Producto,Código,Cantidad Vendida,Total Ventas");
                foreach (var item in topProductos)
                {
                    csv.AppendLine($"\"{item.Producto}\",\"{item.Codigo}\",{item.Cantidad},{item.Total.ToString("F2").Replace(",", "")}");
                }
                csv.AppendLine();

                // SECCIÓN 3: Métodos de Pago
                csv.AppendLine("--- VENTAS POR MÉTODO DE PAGO ---");
                csv.AppendLine("Método de Pago,Total,Transacciones");
                foreach (var item in metodosPago)
                {
                    csv.AppendLine($"\"{item.Metodo}\",{item.Total.ToString("F2").Replace(",", "")},{item.Transacciones}");
                }
                csv.AppendLine();

                // SECCIÓN 4: Tendencia Semanal
                csv.AppendLine("--- TENDENCIA DE VENTAS (ÚLTIMOS 7 DÍAS) ---");
                csv.AppendLine("Fecha,Día,Total Ventas,Transacciones");
                foreach (var item in tendenciaSemanal)
                {
                    var dia = item.GetType().GetProperty("Dia")?.GetValue(item, null);
                    var diaSemana = item.GetType().GetProperty("DiaSemana")?.GetValue(item, null);
                    var total = item.GetType().GetProperty("Total")?.GetValue(item, null);
                    var transacciones = item.GetType().GetProperty("Transacciones")?.GetValue(item, null);

                    csv.AppendLine($"{dia},\"{diaSemana}\",{Convert.ToDouble(total).ToString("F2").Replace(",", "")},{transacciones}");
                }
                csv.AppendLine();

                // SECCIÓN 5: Inventario
                csv.AppendLine("--- ESTADO DE INVENTARIO ---");
                csv.AppendLine("Producto,Código,Stock,Stock Mínimo,Estado,Precio Compra,Valor Inventario");
                foreach (var item in inventario)
                {
                    csv.AppendLine($"\"{item.Nombre}\",\"{item.Codigo}\",{item.Stock},{item.StockMinimo},\"{item.Estado}\",{item.PrecioCompra.ToString("F2").Replace(",", "")},{item.ValorInventario.ToString("F2").Replace(",", "")}");
                }

                // ===== DEVOLVER EL ARCHIVO =====
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                string fileName = $"Dashboard_OpticaClaridad_{DateTime.Now:yyyyMMdd_HHmm}.csv";

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al exportar: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }
    }
}