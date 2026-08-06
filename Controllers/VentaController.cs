using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using OpticaClaridad.Models;
using System.Diagnostics;

namespace OpticaClaridad.Controllers
{
    public class VentaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VentaController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Venta/Index
        public async Task<IActionResult> Index(string filtro = "", string tipoVenta = "", string estado = "")
        {
            if (!LoginController.EstaAutenticado(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            try
            {
                // Construir la consulta base
                var query = _context.Ventas.AsNoTracking();

                // Aplicar filtros
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    filtro = filtro.ToLower();
                    query = query.Where(v =>
                        v.NumeroVenta.ToLower().Contains(filtro) ||
                        (v.Paciente != null && v.Paciente.Nombre.ToLower().Contains(filtro)) ||
                        v.Id.ToString().Contains(filtro)
                    );
                }

                if (!string.IsNullOrWhiteSpace(tipoVenta))
                {
                    query = query.Where(v => v.TipoPago == tipoVenta);
                }

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    query = query.Where(v => v.Estado == estado);
                }

                // Cargar datos con manejo de NULLs
                var ventas = await query
                    .OrderByDescending(v => v.FechaVenta)
                    .Take(100)
                    .ToListAsync();

                Debug.WriteLine($"VENTAS ENCONTRADAS: {ventas.Count}");

                // Para cada venta, cargar detalles y paciente
                foreach (var venta in ventas)
                {
                    // Asegurar que los campos obligatorios tengan valores
                    venta.TipoPago ??= "Efectivo";
                    venta.Estado ??= "COMPLETADA";
                    venta.Observaciones ??= string.Empty;
                    venta.UsuarioId ??= "1";

                    // Cargar detalles CON Producto
                    venta.Detalles = await _context.DetallesVenta
                        .Where(d => d.VentaId == venta.Id)
                        .Include(d => d.Producto)
                        .AsNoTracking()
                        .ToListAsync();

                    // Cargar paciente si tiene PacienteId
                    if (venta.PacienteId.HasValue)
                    {
                        venta.Paciente = await _context.Pacientes
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.Id == venta.PacienteId.Value);
                    }
                }

                ViewBag.TipoVentaSeleccionado = tipoVenta;
                ViewBag.EstadoSeleccionado = estado;
                ViewBag.Filtro = filtro;
                ViewBag.TotalVentas = ventas.Count;

                var ventasCompletadas = ventas.Where(v =>
                    v.Estado != null && v.Estado.Equals("COMPLETADA", StringComparison.OrdinalIgnoreCase));
                ViewBag.TotalMonto = ventasCompletadas.Sum(v => v.Total).ToString("C");

                return View(ventas);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: {ex.Message}");
                TempData["Error"] = "Error al cargar ventas. Verifique los datos en la base de datos.";
                return View(new List<Venta>());
            }
        }

        // GET: Venta/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!LoginController.EstaAutenticado(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            Debug.WriteLine($"Buscando venta con ID: {id}");

            // Cargar venta
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            Debug.WriteLine($"Venta encontrada: {(venta != null ? "SÍ" : "NO")}");

            if (venta == null)
            {
                Debug.WriteLine($"ERROR: No se encontró venta con ID: {id}");
                TempData["Error"] = "Venta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            // Cargar paciente si tiene PacienteId
            if (venta.PacienteId.HasValue)
            {
                venta.Paciente = await _context.Pacientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == venta.PacienteId.Value);
            }

            Debug.WriteLine($"Venta cargada: {venta.NumeroVenta}, PacienteId: {venta.PacienteId}");

            return View(venta);
        }

        // GET: Venta/Ticket/5
        public async Task<IActionResult> Ticket(int id)
        {
            if (!LoginController.EstaAutenticado(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            // Cargar venta
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                TempData["Error"] = "Venta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            // Cargar paciente si tiene PacienteId
            if (venta.PacienteId.HasValue)
            {
                venta.Paciente = await _context.Pacientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == venta.PacienteId.Value);
            }

            // Obtener usuario
            ViewBag.Usuario = _httpContextAccessor.HttpContext.Session.GetString("NombreCompleto") ?? "Admin";

            // ============================================
            // OBTENER URL DE LA IMPRESORA
            // ============================================
            ViewBag.PrinterUrl = ObtenerPrinterUrl();

            return View(venta);
        }

        // NUEVA ACCIÓN: GET: Venta/ImprimirTicket/5
        // Esta acción usa la nueva vista TicketImpresion para imprimir con la app C#
        public async Task<IActionResult> ImprimirTicket(int id)
        {
            if (!LoginController.EstaAutenticado(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            // Cargar venta con todos los datos necesarios
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                TempData["Error"] = "Venta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            // Cargar paciente si tiene PacienteId
            if (venta.PacienteId.HasValue)
            {
                venta.Paciente = await _context.Pacientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == venta.PacienteId.Value);
            }

            // Configurar datos para la vista
            ViewBag.Usuario = _httpContextAccessor.HttpContext.Session.GetString("NombreCompleto") ?? "Admin";

            // Activar auto-impresión
            TempData["AutoPrint"] = true;

            // Retornar la nueva vista de impresión directa
            return View("TicketImpresion", venta);

            // Obtener IP de la impresora (puerto 3001)
            var ipServidor = HttpContext.Session.GetString("IpServidor");
            if (string.IsNullOrEmpty(ipServidor))
            {
                ipServidor = ObtenerIpAutomatica();
                HttpContext.Session.SetString("IpServidor", ipServidor);
            }
            ViewBag.PrinterUrl = $"http://{ipServidor}:3001";
        }

        // GET: Venta/Cancel/5
        public async Task<IActionResult> Cancel(int id)
        {
            if (!LoginController.EstaAutenticado(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            // Cargar venta
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                TempData["Error"] = "Venta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            // Cargar paciente si tiene PacienteId
            if (venta.PacienteId.HasValue)
            {
                venta.Paciente = await _context.Pacientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == venta.PacienteId.Value);
            }

            if (venta.Estado == "CANCELADA")
            {
                TempData["Warning"] = "La venta ya está cancelada";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(venta);
        }

        // GET: Venta/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!LoginController.EstaAutenticado(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            // Solo administradores pueden eliminar
            if (!LoginController.EsAdministrador(HttpContext.Session))
            {
                TempData["Error"] = "No tiene permisos para eliminar ventas";
                return RedirectToAction(nameof(Index));
            }

            // Cargar venta
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                TempData["Error"] = "Venta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            // Cargar paciente si tiene PacienteId
            if (venta.PacienteId.HasValue)
            {
                venta.Paciente = await _context.Pacientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == venta.PacienteId.Value);
            }

            if (venta.Estado != "CANCELADA")
            {
                TempData["Error"] = "Solo se pueden eliminar ventas canceladas";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(venta);
        }

        // GET: Venta/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!LoginController.EstaAutenticado(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            try
            {
                ViewBag.Pacientes = await _context.Pacientes
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .Select(p => new { p.Id, p.Nombre })
                    .ToListAsync();

                ViewBag.Productos = await _context.Productos
                    .Where(p => p.Activo && p.Stock > 0)
                    .OrderBy(p => p.Nombre)
                    .Select(p => new {
                        p.Id,
                        p.Codigo,
                        p.Nombre,
                        p.PrecioVenta,
                        p.Stock,
                        Display = $"{p.Codigo} - {p.Nombre} (Stock: {p.Stock}, ${p.PrecioVenta})"
                    })
                    .ToListAsync();

                var nuevaVenta = new Venta
                {
                    FechaVenta = DateTime.Today,
                    TipoPago = "Efectivo",
                    Estado = "PENDIENTE",
                    PacienteId = null
                };

                return View(nuevaVenta);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar datos para venta: {ex.Message}");
                TempData["Error"] = "Error al inicializar la venta";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Venta/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("PacienteId,FechaVenta,TipoPago,Observaciones,Total,MontoPagado,Cambio,Estado,UsuarioId")] Venta venta,
            List<int> productoIds,
            List<int> cantidades)
        {
            Console.WriteLine("=== VALORES RECIBIDOS ===");
            Console.WriteLine($"PacienteId: {venta.PacienteId}");
            Console.WriteLine($"FechaVenta: {venta.FechaVenta}");
            Console.WriteLine($"TipoPago: {venta.TipoPago}");
            Console.WriteLine($"Total: {venta.Total}");
            Console.WriteLine($"MontoPagado: {venta.MontoPagado}");
            Console.WriteLine($"Cambio: {venta.Cambio}");
            Console.WriteLine($"Observaciones: {venta.Observaciones}");
            Console.WriteLine($"Estado: {venta.Estado}");
            Console.WriteLine($"UsuarioId: {venta.UsuarioId}");
            Console.WriteLine($"ProductoIds: {(productoIds != null ? productoIds.Count : 0)}");
            Console.WriteLine($"Cantidades: {(cantidades != null ? cantidades.Count : 0)}");
            Console.WriteLine("==========================");

            if (!LoginController.EstaAutenticado(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            try
            {
                if (productoIds == null || productoIds.Count == 0)
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto a la venta");
                    await CargarDatosViewBag();
                    return View(venta);
                }

                // Validar cantidades
                for (int i = 0; i < productoIds.Count; i++)
                {
                    var producto = await _context.Productos.FindAsync(productoIds[i]);
                    if (producto == null || !producto.Activo)
                    {
                        ModelState.AddModelError("", $"Producto ID {productoIds[i]} no encontrado o inactivo");
                        await CargarDatosViewBag();
                        return View(venta);
                    }

                    if (cantidades[i] <= 0 || cantidades[i] > producto.Stock)
                    {
                        ModelState.AddModelError("", $"Cantidad no válida para {producto.Nombre}. Stock disponible: {producto.Stock}");
                        await CargarDatosViewBag();
                        return View(venta);
                    }
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // CONFIGURAR VENTA - Valores por defecto
                        venta.NumeroVenta = "V-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                        venta.UsuarioId = _httpContextAccessor.HttpContext.Session.GetString("UsuarioId") ?? "1";
                        venta.FechaCreacion = DateTime.Now;
                        venta.Estado = "COMPLETADA";

                        // Calcular total si no viene del formulario
                        if (venta.Total == 0)
                        {
                            decimal totalCalculado = 0;
                            for (int i = 0; i < productoIds.Count; i++)
                            {
                                var producto = await _context.Productos.FindAsync(productoIds[i]);
                                totalCalculado += producto.PrecioVenta * cantidades[i];
                            }
                            venta.Total = totalCalculado;
                        }

                        // CONFIGURAR MONTOS DE PAGO
                        if (venta.TipoPago == "Efectivo")
                        {
                            if (venta.MontoPagado < venta.Total)
                            {
                                ModelState.AddModelError("", "El monto pagado no puede ser menor al total");
                                await CargarDatosViewBag();
                                return View(venta);
                            }
                        }
                        else if (venta.TipoPago == "Tarjeta")
                        {
                            venta.MontoPagado = venta.Total;
                            venta.Cambio = 0;
                        }
                        else if (venta.TipoPago == "Credito")
                        {
                            // VALIDACIÓN CRÉDITO: Debe tener un paciente real (ID > 0)
                            if (!venta.PacienteId.HasValue || venta.PacienteId.Value <= 0)
                            {
                                ModelState.AddModelError("", "Debe seleccionar un paciente para venta a crédito");
                                await CargarDatosViewBag();
                                return View(venta);
                            }
                            venta.MontoPagado = venta.MontoPagado > 0 ? venta.MontoPagado : venta.Total;
                            venta.Cambio = 0;
                        }

                        // GUARDAR VENTA
                        _context.Ventas.Add(venta);
                        await _context.SaveChangesAsync();

                        // AGREGAR DETALLES Y REDUCIR STOCK
                        for (int i = 0; i < productoIds.Count; i++)
                        {
                            var producto = await _context.Productos.FindAsync(productoIds[i]);

                            var detalle = new DetalleVenta
                            {
                                VentaId = venta.Id,
                                ProductoId = productoIds[i],
                                Cantidad = cantidades[i],
                                PrecioUnitario = producto.PrecioVenta
                            };

                            // Reducir stock
                            producto.ReducirStock(cantidades[i]);
                            _context.Update(producto);

                            _context.DetallesVenta.Add(detalle);
                        }

                        // SI ES CRÉDITO, CREAR CRÉDITO
                        if (venta.TipoPago == "Credito" && venta.PacienteId.HasValue && venta.PacienteId.Value > 0)
                        {
                            // Crear nuevo crédito para el paciente
                            var nuevoCredito = new Credito
                            {
                                PacienteId = venta.PacienteId.Value,
                                VentaId = venta.Id,
                                MontoTotal = venta.Total,
                                Saldo = venta.Total - venta.MontoPagado,
                                FechaCredito = venta.FechaVenta,
                                FechaVencimiento = venta.FechaVenta.AddDays(30),
                                Estado = "PENDIENTE",
                                Observaciones = $"Crédito por venta #{venta.NumeroVenta}" +
                                               (!string.IsNullOrEmpty(venta.Observaciones) ?
                                                $" - {venta.Observaciones}" : ""),
                                DiasCredito = 30,
                                TipoPeriodo = "MESES",
                                CantidadPeriodo = 1,
                                MontoPeriodo = venta.Total,
                                FechaCreacion = DateTime.Now
                            };

                            _context.Creditos.Add(nuevoCredito);

                            // Agregar nota a la venta
                            venta.Observaciones += $" | Venta a crédito - Paciente ID: {venta.PacienteId.Value}";
                            _context.Update(venta);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["Success"] = $"Venta #{venta.NumeroVenta} registrada exitosamente";

                        if (venta.TipoPago == "Credito")
                        {
                            TempData["Info"] = "Venta a crédito registrada. Recuerde entregar el ticket al paciente.";
                        }
                        else if (venta.TipoPago == "Efectivo" && venta.Cambio > 0)
                        {
                            TempData["Info"] = $"Entregar cambio: ${venta.Cambio:N2}";
                        }

                        TempData["VentaId"] = venta.Id;

                        return RedirectToAction("Ticket", new { id = venta.Id });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Debug.WriteLine($"Error en transacción de venta: {ex.Message}");
                        ModelState.AddModelError("", $"Error al procesar la venta: {ex.Message}");
                        await CargarDatosViewBag();
                        return View(venta);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general en creación de venta: {ex.Message}");
                ModelState.AddModelError("", "Error al procesar la venta");
                await CargarDatosViewBag();
                return View(venta);
            }
        }

        // POST: Venta/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id, string motivo = "")
        {
            if (!LoginController.EstaAutenticado(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var venta = await _context.Ventas
                        .Include(v => v.Detalles)
                            .ThenInclude(d => d.Producto)
                        .FirstOrDefaultAsync(v => v.Id == id);

                    if (venta == null)
                    {
                        TempData["Error"] = "Venta no encontrada";
                        return RedirectToAction(nameof(Index));
                    }

                    // Devolver productos al stock
                    foreach (var detalle in venta.Detalles)
                    {
                        var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                        if (producto != null)
                        {
                            producto.AumentarStock(detalle.Cantidad);
                            _context.Update(producto);
                        }
                    }

                    // Si la venta era a crédito, buscar y cancelar el crédito
                    if (venta.TipoPago == "Credito")
                    {
                        var credito = await _context.Creditos
                            .FirstOrDefaultAsync(c => c.VentaId == venta.Id);
                        if (credito != null)
                        {
                            credito.Estado = "CANCELADO";
                            _context.Update(credito);
                        }
                    }

                    // Marcar como cancelada
                    venta.Estado = "CANCELADA";
                    venta.Observaciones += $" | CANCELADA: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    if (!string.IsNullOrEmpty(motivo))
                        venta.Observaciones += $" - Motivo: {motivo}";

                    _context.Update(venta);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = $"Venta #{venta.NumeroVenta} cancelada exitosamente. Stock restaurado.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Debug.WriteLine($"Error al cancelar venta: {ex.Message}");
                    TempData["Error"] = $"Error al cancelar la venta: {ex.Message}";
                    return RedirectToAction(nameof(Cancel), new { id });
                }
            }
        }

        // POST: Venta/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!LoginController.EstaAutenticado(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            if (!LoginController.EsAdministrador(HttpContext.Session))
            {
                TempData["Error"] = "No tiene permisos para eliminar ventas";
                return RedirectToAction(nameof(Index));
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var venta = await _context.Ventas
                        .Include(v => v.Detalles)
                            .ThenInclude(d => d.Producto)
                        .FirstOrDefaultAsync(v => v.Id == id);

                    if (venta == null)
                    {
                        TempData["Error"] = "Venta no encontrada";
                        return RedirectToAction(nameof(Index));
                    }

                    if (venta.Estado != "CANCELADA")
                    {
                        TempData["Error"] = "Solo se pueden eliminar ventas canceladas";
                        return RedirectToAction(nameof(Details), new { id });
                    }

                    // Si tenía crédito, eliminarlo o marcarlo
                    var credito = await _context.Creditos
                        .FirstOrDefaultAsync(c => c.VentaId == venta.Id);
                    if (credito != null)
                    {
                        _context.Creditos.Remove(credito);
                    }

                    _context.DetallesVenta.RemoveRange(venta.Detalles);
                    _context.Ventas.Remove(venta);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = $"Venta #{venta.NumeroVenta} eliminada permanentemente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Debug.WriteLine($"Error al eliminar venta: {ex.Message}");
                    TempData["Error"] = $"Error al eliminar la venta: {ex.Message}";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
        }

        // AJAX: Obtener datos del producto
        [HttpGet]
        public async Task<IActionResult> GetProducto(int id)
        {
            try
            {
                var producto = await _context.Productos
                    .Where(p => p.Id == id && p.Activo)
                    .Select(p => new
                    {
                        p.Id,
                        p.Codigo,
                        p.Nombre,
                        p.PrecioVenta,
                        p.Stock,
                        p.EstadoStock,
                        Disponible = p.Stock > 0
                    })
                    .FirstOrDefaultAsync();

                if (producto == null)
                    return Json(new { success = false, message = "Producto no encontrado" });

                return Json(new { success = true, producto });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener producto: {ex.Message}");
                return Json(new { success = false, message = "Error al obtener producto" });
            }
        }

        // NUEVO: AJAX para obtener paciente
        [HttpGet]
        public async Task<IActionResult> GetPaciente(int id)
        {
            try
            {
                var paciente = await _context.Pacientes
                    .Where(p => p.Id == id && p.Activo)
                    .Select(p => new
                    {
                        p.Id,
                        p.Nombre,
                        p.Telefono,
                        p.Email,
                        p.FechaNacimiento,
                        p.Sexo,
                        p.Observaciones
                    })
                    .FirstOrDefaultAsync();

                if (paciente == null)
                    return Json(new { success = false, message = "Paciente no encontrado" });

                return Json(new { success = true, paciente });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener paciente: {ex.Message}");
                return Json(new { success = false, message = "Error al obtener paciente" });
            }
        }

        // NUEVO: Obtener pacientes con créditos pendientes
        [HttpGet]
        public async Task<IActionResult> GetPacientesConCreditos(int page = 1, int pageSize = 20)
        {
            try
            {
                var pacientesConCreditosQuery = _context.Creditos
                    .Where(c => c.VentaId == null && c.Estado == "PENDIENTE")
                    .Select(c => c.PacienteId)
                    .Distinct();

                var totalPacientes = await pacientesConCreditosQuery.CountAsync();

                var pacientes = await _context.Pacientes
                    .Where(p => pacientesConCreditosQuery.Contains(p.Id) && p.Activo)
                    .OrderBy(p => p.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.Nombre,
                        p.Telefono,
                        p.Email,
                        CreditosPendientes = _context.Creditos
                            .Count(c => c.PacienteId == p.Id && c.VentaId == null && c.Estado == "PENDIENTE")
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    pacientes,
                    total = totalPacientes,
                    page,
                    pageSize,
                    hasMore = (page * pageSize) < totalPacientes
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar pacientes" });
            }
        }

        // GET: Venta/BuscarPacientesConCreditos
        [HttpGet]
        public async Task<IActionResult> BuscarPacientesConCreditos(string termino, int page = 1, int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino) || termino.Length < 2)
                    return Json(new { success = true, pacientes = new List<object>(), total = 0 });

                termino = termino.ToLower();

                var pacientesConCreditosIds = _context.Creditos
                    .Where(c => c.VentaId == null && c.Estado == "PENDIENTE")
                    .Select(c => c.PacienteId)
                    .Distinct();

                var query = _context.Pacientes
                    .Where(p => pacientesConCreditosIds.Contains(p.Id) && p.Activo &&
                           (p.Nombre.ToLower().Contains(termino) ||
                            (p.Telefono != null && p.Telefono.Contains(termino)) ||
                            (p.Email != null && p.Email.ToLower().Contains(termino))));

                var total = await query.CountAsync();

                var pacientes = await query
                    .OrderBy(p => p.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.Nombre,
                        p.Telefono,
                        p.Email
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    pacientes,
                    total,
                    page,
                    pageSize,
                    hasMore = (page * pageSize) < total
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error en la búsqueda" });
            }
        }

        // NUEVO: Obtener TODOS los pacientes activos
        [HttpGet]
        public async Task<IActionResult> GetPacientes(int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.Pacientes
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre);

                var totalPacientes = await query.CountAsync();

                var pacientes = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.Nombre,
                        p.Telefono,
                        p.Email
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    pacientes,
                    total = totalPacientes,
                    page,
                    pageSize,
                    hasMore = (page * pageSize) < totalPacientes
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar pacientes" });
            }
        }

        // NUEVO: Buscar pacientes (todos los activos)
        [HttpGet]
        public async Task<IActionResult> BuscarPacientes(string termino, int page = 1, int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino) || termino.Length < 2)
                    return Json(new { success = true, pacientes = new List<object>(), total = 0 });

                termino = termino.ToLower();

                var query = _context.Pacientes
                    .Where(p => p.Activo &&
                           (p.Nombre.ToLower().Contains(termino) ||
                            (p.Telefono != null && p.Telefono.Contains(termino)) ||
                            (p.Email != null && p.Email.ToLower().Contains(termino))));

                var total = await query.CountAsync();

                var pacientes = await query
                    .OrderBy(p => p.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.Nombre,
                        p.Telefono,
                        p.Email
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    pacientes,
                    total,
                    page,
                    pageSize,
                    hasMore = (page * pageSize) < total
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error en la búsqueda" });
            }
        }

        // GET: /Venta/BuscarProductos
        public IActionResult BuscarProductos(string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino) || termino.Length < 2)
                {
                    return Json(new { success = false, message = "Ingrese al menos 2 caracteres" });
                }

                var productos = _context.Productos
                    .Where(p => p.Activo && p.Stock > 0 && (
                        p.Nombre.Contains(termino) ||
                        p.Codigo.Contains(termino) ||
                        (p.Marca != null && p.Marca.Contains(termino)) ||
                        (p.Descripcion != null && p.Descripcion.Contains(termino))
                    ))
                    .OrderBy(p => p.Nombre)
                    .Take(20)
                    .Select(p => new
                    {
                        p.Id,
                        p.Nombre,
                        p.Codigo,
                        p.Marca,
                        p.PrecioVenta,
                        p.Stock
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    productos = productos,
                    count = productos.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Método auxiliar para cargar datos en ViewBag
        private async Task CargarDatosViewBag()
        {
            ViewBag.Pacientes = await _context.Pacientes
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .Select(p => new {
                    p.Id,
                    p.Nombre,
                    p.Telefono,
                    p.Email
                })
                .ToListAsync();

            ViewBag.Productos = await _context.Productos
                .Where(p => p.Activo && p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .Select(p => new {
                    p.Id,
                    p.Codigo,
                    p.Nombre,
                    p.PrecioVenta,
                    p.Stock,
                    Display = $"{p.Codigo} - {p.Nombre} (Stock: {p.Stock}, ${p.PrecioVenta})"
                })
                .ToListAsync();
        }
        // =============================================
        // MÉTODOS DE DETECCIÓN DE IP (copiados de AccesoController)
        // =============================================

        private string ObtenerIpAutomatica()
        {
            try
            {
                // 1. Intentar con socket (más confiable)
                try
                {
                    using (var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0))
                    {
                        socket.Connect("8.8.8.8", 65530);
                        if (socket.LocalEndPoint is System.Net.IPEndPoint ipEndPoint)
                        {
                            string ip = ipEndPoint.Address.ToString();
                            if (EsIpUtil(ip))
                            {
                                return ip;
                            }
                        }
                    }
                }
                catch { }

                // 2. Escanear interfaces de red
                var ipsValidas = new List<string>();

                try
                {
                    var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                        .Where(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
                            && n.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback
                            && !n.Description.ToLower().Contains("virtual")
                            && !n.Description.ToLower().Contains("vpn")
                            && !n.Description.ToLower().Contains("bluetooth"));

                    foreach (var interfaz in interfaces)
                    {
                        var propiedades = interfaz.GetIPProperties();
                        var ips = propiedades.UnicastAddresses
                            .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .Select(addr => addr.Address.ToString());

                        foreach (var ip in ips)
                        {
                            if (EsIpUtil(ip))
                            {
                                ipsValidas.Add(ip);
                                if (ip.StartsWith("192.168.") || ip.StartsWith("10."))
                                {
                                    return ip;
                                }
                            }
                        }
                    }
                }
                catch { }

                // 3. Usar primera IP válida encontrada
                if (ipsValidas.Any())
                {
                    return ipsValidas.First();
                }

                // 4. Último recurso: DNS
                try
                {
                    var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            string ipStr = ip.ToString();
                            if (EsIpUtil(ipStr))
                            {
                                return ipStr;
                            }
                        }
                    }
                }
                catch { }

                return "localhost";
            }
            catch
            {
                return "localhost";
            }
        }

        private bool EsIpUtil(string ip)
        {
            return !ip.StartsWith("127.") &&           // Loopback
                   !ip.StartsWith("169.254.") &&       // APIPA
                   !ip.StartsWith("0.") &&             // Inválida
                   ip != "0.0.0.0";
        }

        // =============================================
        // MÉTODO PARA OBTENER LA URL DE LA IMPRESORA
        // =============================================
        private string ObtenerPrinterUrl()
        {
            // 1. Intentar obtener de la sesión primero
            string printerUrl = HttpContext.Session.GetString("PrinterUrl");

            if (!string.IsNullOrEmpty(printerUrl))
            {
                return printerUrl;
            }

            // 2. Si no está en sesión, detectar IP automáticamente
            string ipServidor = ObtenerIpAutomatica();
            printerUrl = $"http://{ipServidor}:3001";

            // 3. Guardar en sesión para futuras ocasiones
            HttpContext.Session.SetString("IpServidor", ipServidor);
            HttpContext.Session.SetString("PrinterUrl", printerUrl);

            return printerUrl;
        }
    }

}