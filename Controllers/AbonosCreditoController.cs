using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using OpticaClaridad.Models;
using QRCoder;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace OpticaClaridad.Controllers
{
    public class AbonosCreditoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AbonosCreditoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AbonosCredito (para un crédito específico)
        public async Task<IActionResult> Index(int? creditoId)
        {
            if (!creditoId.HasValue)
            {
                return RedirectToAction("Index", "Creditos");
            }

            var credito = await _context.Creditos
                .Include(c => c.Paciente)
                .FirstOrDefaultAsync(c => c.Id == creditoId.Value);

            if (credito == null)
            {
                return NotFound();
            }

            var abonos = await _context.AbonosCredito
                .Where(a => a.CreditoId == creditoId.Value)
                .OrderByDescending(a => a.FechaAbono)
                .ToListAsync();

            ViewBag.Credito = credito;
            ViewBag.CreditoId = creditoId.Value;

            return View(abonos);
        }

        // GET: AbonosCredito/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var abonoCredito = await _context.AbonosCredito
                .Include(a => a.Credito)
                .ThenInclude(c => c.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (abonoCredito == null)
            {
                return NotFound();
            }

            return View(abonoCredito);
        }

        // GET: AbonosCredito/Create
        public async Task<IActionResult> Create(int? creditoId)
        {
            if (!creditoId.HasValue)
            {
                return NotFound();
            }

            var credito = await _context.Creditos
                .Include(c => c.Paciente)
                .Include(c => c.Abonos)
                .FirstOrDefaultAsync(c => c.Id == creditoId.Value);

            if (credito == null)
            {
                return NotFound();
            }

            if (credito.Estado == "PAGADO")
            {
                TempData["ErrorMessage"] = "No se pueden registrar abonos a un crédito ya pagado.";
                return RedirectToAction("Details", "Creditos", new { id = creditoId.Value });
            }

            decimal totalAbonado = credito.Abonos?.Sum(a => a.Monto) ?? 0;
            decimal saldoPendiente = credito.MontoTotal - totalAbonado;

            var abono = new AbonoCredito
            {
                CreditoId = creditoId.Value,
                FechaAbono = DateTime.Today
            };

            ViewBag.Credito = credito;
            ViewBag.SaldoPendiente = saldoPendiente;

            return View(abono);
        }

        // POST: AbonosCredito/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CreditoId,Monto,FechaAbono,Observaciones")] AbonoCredito abonoCredito)
        {
            Console.WriteLine("=== CREANDO NUEVO ABONO ===");
            Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"User.Identity.Name: {User.Identity?.Name}");
            Console.WriteLine($"User.Identity.AuthenticationType: {User.Identity?.AuthenticationType}");

            if (abonoCredito.CreditoId <= 0)
            {
                ModelState.AddModelError("CreditoId", "El crédito es requerido.");
            }

            if (abonoCredito.Monto <= 0)
            {
                ModelState.AddModelError("Monto", "El monto debe ser mayor a 0.");
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ Errores de validación detectados");

                var credito = await _context.Creditos
                    .Include(c => c.Paciente)
                    .FirstOrDefaultAsync(c => c.Id == abonoCredito.CreditoId);

                ViewBag.Credito = credito;
                decimal totalAbonado = credito?.Abonos?.Sum(a => a.Monto) ?? 0;
                ViewBag.SaldoPendiente = (credito?.MontoTotal ?? 0) - totalAbonado;

                return View(abonoCredito);
            }

            try
            {
                Console.WriteLine("🔄 Procesando abono...");

                var credito = await _context.Creditos
                    .Include(c => c.Abonos)
                    .FirstOrDefaultAsync(c => c.Id == abonoCredito.CreditoId);

                if (credito == null)
                {
                    Console.WriteLine("❌ Crédito no encontrado");
                    ModelState.AddModelError("", "El crédito no existe.");

                    var creditoInfo = await _context.Creditos
                        .Include(c => c.Paciente)
                        .FirstOrDefaultAsync(c => c.Id == abonoCredito.CreditoId);

                    ViewBag.Credito = creditoInfo;
                    decimal totalAbonadoInfo = creditoInfo?.Abonos?.Sum(a => a.Monto) ?? 0;
                    ViewBag.SaldoPendiente = (creditoInfo?.MontoTotal ?? 0) - totalAbonadoInfo;

                    return View(abonoCredito);
                }

                decimal totalAbonado = credito.Abonos?.Sum(a => a.Monto) ?? 0;
                decimal saldoPendienteReal = credito.MontoTotal - totalAbonado;

                Console.WriteLine($"✅ Crédito encontrado: #{credito.Id}");
                Console.WriteLine($"   MontoTotal: ${credito.MontoTotal}");
                Console.WriteLine($"   Total Abonado: ${totalAbonado}");
                Console.WriteLine($"   Saldo Pendiente REAL: ${saldoPendienteReal}");
                Console.WriteLine($"   Saldo en BD: ${credito.Saldo}");

                if (credito.Estado == "PAGADO")
                {
                    Console.WriteLine("❌ Crédito ya está pagado");
                    TempData["ErrorMessage"] = "No se pueden registrar abonos a un crédito ya pagado.";
                    return RedirectToAction("Details", "Creditos", new { id = credito.Id });
                }

                if (abonoCredito.Monto > saldoPendienteReal)
                {
                    Console.WriteLine($"❌ Monto excede saldo: ${abonoCredito.Monto} > ${saldoPendienteReal}");
                    ModelState.AddModelError("Monto",
                        $"El monto no puede exceder el saldo pendiente (${saldoPendienteReal:N2})");

                    ViewBag.Credito = credito;
                    ViewBag.SaldoPendiente = saldoPendienteReal;

                    return View(abonoCredito);
                }

                credito.Saldo = saldoPendienteReal - abonoCredito.Monto;

                if (credito.Saldo <= 0)
                {
                    credito.Estado = "PAGADO";
                    credito.Saldo = 0;
                }

                _context.Update(credito);

                Console.WriteLine($"✅ Saldo actualizado: ${credito.Saldo}, Estado: {credito.Estado}");

                string usuarioActual = "SISTEMA";
                var nombreCompleto = HttpContext.Session.GetString("NombreCompleto");
                var esAdmin = HttpContext.Session.GetString("EsAdmin");

                if (!string.IsNullOrEmpty(nombreCompleto))
                {
                    usuarioActual = nombreCompleto;
                }
                else if (esAdmin == "True")
                {
                    usuarioActual = "ADMIN";
                }

                var nuevoAbono = new AbonoCredito
                {
                    CreditoId = abonoCredito.CreditoId,
                    Monto = abonoCredito.Monto,
                    FechaAbono = abonoCredito.FechaAbono,
                    Observaciones = abonoCredito.Observaciones,
                    UsuarioId = usuarioActual,
                    FechaCreacion = DateTime.Now
                };

                Console.WriteLine($"✅ Nuevo abono creado: ${nuevoAbono.Monto}, Usuario: {nuevoAbono.UsuarioId}");

                _context.AbonosCredito.Add(nuevoAbono);
                var cambios = await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Guardado exitoso. Cambios: {cambios}, Abono ID: {nuevoAbono.Id}");

                TempData["SuccessMessage"] = $"Abono de ${nuevoAbono.Monto:N2} registrado exitosamente. Nuevo saldo: ${credito.Saldo:N2}";
                return RedirectToAction("Details", "Creditos", new { id = credito.Id });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"❌ ERROR DE BD: {dbEx.Message}");
                if (dbEx.InnerException != null)
                {
                    Console.WriteLine($"❌ ERROR INTERNO: {dbEx.InnerException.Message}");
                }

                ModelState.AddModelError("", $"Error de base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");

                ModelState.AddModelError("", $"Error al registrar el abono: {ex.Message}");
            }

            var creditoError = await _context.Creditos
                .Include(c => c.Paciente)
                .Include(c => c.Abonos)
                .FirstOrDefaultAsync(c => c.Id == abonoCredito.CreditoId);

            ViewBag.Credito = creditoError;
            decimal totalAbonadoError = creditoError?.Abonos?.Sum(a => a.Monto) ?? 0;
            ViewBag.SaldoPendiente = (creditoError?.MontoTotal ?? 0) - totalAbonadoError;

            return View(abonoCredito);
        }

        // GET: AbonosCredito/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var abonoCredito = await _context.AbonosCredito
                .Include(a => a.Credito)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (abonoCredito == null)
            {
                return NotFound();
            }

            if (abonoCredito.Credito?.Estado == "PAGADO")
            {
                TempData["ErrorMessage"] = "No se puede editar un abono de un crédito ya pagado.";
                return RedirectToAction("Details", "Creditos", new { id = abonoCredito.CreditoId });
            }

            ViewBag.Credito = abonoCredito.Credito;
            return View(abonoCredito);
        }

        // POST: AbonosCredito/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CreditoId,Monto,FechaAbono,Observaciones,UsuarioId,FechaCreacion")] AbonoCredito abonoCredito)
        {
            if (id != abonoCredito.Id)
            {
                return NotFound();
            }

            var credito = await _context.Creditos.FindAsync(abonoCredito.CreditoId);
            if (credito?.Estado == "PAGADO")
            {
                TempData["ErrorMessage"] = "No se puede editar un abono de un crédito ya pagado.";
                return RedirectToAction("Details", "Creditos", new { id = abonoCredito.CreditoId });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var abonoOriginal = await _context.AbonosCredito
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Id == id);

                    if (abonoOriginal != null)
                    {
                        decimal diferencia = abonoCredito.Monto - abonoOriginal.Monto;

                        if (credito.Saldo - diferencia < 0)
                        {
                            ModelState.AddModelError("Monto",
                                $"El ajuste dejaría un saldo negativo en el crédito. Saldo actual: ${credito.Saldo:N2}");
                            ViewBag.Credito = credito;
                            return View(abonoCredito);
                        }

                        credito.Saldo -= diferencia;

                        if (credito.Saldo <= 0)
                        {
                            credito.Estado = "PAGADO";
                        }
                    }

                    _context.Update(abonoCredito);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Abono actualizado exitosamente.";
                    return RedirectToAction("Details", "Creditos", new { id = abonoCredito.CreditoId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AbonoCreditoExists(abonoCredito.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            credito = await _context.Creditos.FindAsync(abonoCredito.CreditoId);
            ViewBag.Credito = credito;
            return View(abonoCredito);
        }

        // GET: AbonosCredito/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var abonoCredito = await _context.AbonosCredito
                .Include(a => a.Credito)
                .ThenInclude(c => c.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (abonoCredito == null)
            {
                return NotFound();
            }

            return View(abonoCredito);
        }

        // POST: AbonosCredito/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var abonoCredito = await _context.AbonosCredito
                .Include(a => a.Credito)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (abonoCredito == null)
            {
                return NotFound();
            }

            var credito = abonoCredito.Credito;
            if (credito == null)
            {
                return NotFound();
            }

            try
            {
                credito.Saldo += abonoCredito.Monto;

                if (credito.Estado == "PAGADO" && credito.Saldo > 0)
                {
                    credito.Estado = "PENDIENTE";
                }

                _context.AbonosCredito.Remove(abonoCredito);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Abono de ${abonoCredito.Monto:N2} eliminado exitosamente. Saldo restaurado.";
                return RedirectToAction("Details", "Creditos", new { id = credito.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar el abono: {ex.Message}";
                return RedirectToAction("Delete", new { id });
            }
        }

        // GET: AbonosCredito/ReportePorFecha
        public async Task<IActionResult> ReportePorFecha(DateTime? fechaInicio, DateTime? fechaFin)
        {
            fechaInicio ??= DateTime.Today.AddMonths(-1);
            fechaFin ??= DateTime.Today;

            var abonos = await _context.AbonosCredito
                .Include(a => a.Credito)
                .ThenInclude(c => c.Paciente)
                .Where(a => a.FechaAbono >= fechaInicio && a.FechaAbono <= fechaFin)
                .OrderByDescending(a => a.FechaAbono)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio.Value.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin.Value.ToString("yyyy-MM-dd");
            ViewBag.TotalAbonos = abonos.Sum(a => a.Monto);
            ViewBag.CantidadAbonos = abonos.Count;

            return View(abonos);
        }

        // GET: AbonosCredito/TicketHistorico/5
        public async Task<IActionResult> TicketHistorico(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credito = await _context.Creditos
                .Include(c => c.Paciente)
                .Include(c => c.Abonos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (credito == null)
            {
                return NotFound();
            }

            var abonos = credito.Abonos
                .OrderByDescending(a => a.FechaAbono)
                .ThenByDescending(a => a.Id)
                .ToList();

            var reporte = new ReporteTicket
            {
                Credito = credito,
                Abonos = abonos,
                TotalAbonado = abonos.Sum(a => a.Monto)
            };

            ViewBag.TituloReporte = "HISTORIAL DE ABONOS";
            return View(reporte);
        }

        // GET: Creditos/ImprimirHistorial/5
        public async Task<IActionResult> ImprimirHistorial(int id)
        {
            var credito = await _context.Creditos
                .Include(c => c.Paciente)
                .Include(c => c.Abonos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (credito == null)
            {
                return NotFound();
            }

            var reporte = new ReporteTicket
            {
                Credito = credito,
                Abonos = credito.Abonos.OrderBy(a => a.FechaAbono).ToList(),
                TotalAbonado = credito.Abonos.Sum(a => a.Monto)
            };

            // ============================================
            // OBTENER URL DE LA IMPRESORA
            // ============================================
            ViewBag.PrinterUrl = ObtenerPrinterUrl();

            TempData["AutoPrint"] = true;

            return View("TicketHistoricoImpresion", reporte);
        }

        public IActionResult QRCodeImage(int id)
        {
            try
            {
                var credito = _context.Creditos
                    .Include(c => c.Paciente)
                    .FirstOrDefault(c => c.Id == id);

                if (credito == null)
                    return NotFound();

                string qrText = $@"
ÓPTICA CLARIDAD
Historial de Abonos
Crédito: #{credito.Id}
Paciente: {credito.Paciente?.Nombre ?? "N/A"}
Fecha: {DateTime.Now:dd/MM/yyyy}
Monto Total: ${credito.MontoTotal:N2}
Abonado: ${(credito.MontoTotal - credito.Saldo):N2}
Saldo: ${credito.Saldo:N2}
Tel: 656 556 3770
Email: claridadoptica1@gmail.com
Instagram: opticaclaridadjrz".Trim();

                using (var qrGenerator = new QRCodeGenerator())
                {
                    using (var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q))
                    {
                        using (var qrCode = new QRCode(qrCodeData))
                        {
                            using (var bitmap = qrCode.GetGraphic(20))
                            {
                                using (var stream = new MemoryStream())
                                {
                                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                    return File(stream.ToArray(), "image/png");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generando QR: {ex.Message}");
                return NotFound();
            }
        }

        private bool AbonoCreditoExists(int id)
        {
            return _context.AbonosCredito.Any(e => e.Id == id);
        }

        // =============================================
        // MÉTODOS DE DETECCIÓN DE IP
        // =============================================

        private string ObtenerIpAutomatica()
        {
            try
            {
                try
                {
                    using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect("8.8.8.8", 65530);
                        if (socket.LocalEndPoint is IPEndPoint ipEndPoint)
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

                var ipsValidas = new List<string>();

                try
                {
                    var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                        .Where(n => n.OperationalStatus == OperationalStatus.Up
                            && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                            && !n.Description.ToLower().Contains("virtual")
                            && !n.Description.ToLower().Contains("vpn")
                            && !n.Description.ToLower().Contains("bluetooth"));

                    foreach (var interfaz in interfaces)
                    {
                        var propiedades = interfaz.GetIPProperties();
                        var ips = propiedades.UnicastAddresses
                            .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
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

                if (ipsValidas.Any())
                {
                    return ipsValidas.First();
                }

                try
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
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
            return !ip.StartsWith("127.") &&
                   !ip.StartsWith("169.254.") &&
                   !ip.StartsWith("0.") &&
                   ip != "0.0.0.0";
        }

        // =============================================
        // MÉTODO PARA OBTENER LA URL DE LA IMPRESORA
        // =============================================
        private string ObtenerPrinterUrl()
        {
            string printerUrl = HttpContext.Session.GetString("PrinterUrl");

            if (!string.IsNullOrEmpty(printerUrl))
            {
                return printerUrl;
            }

            string ipServidor = ObtenerIpAutomatica();
            printerUrl = $"http://{ipServidor}:3001";

            HttpContext.Session.SetString("IpServidor", ipServidor);
            HttpContext.Session.SetString("PrinterUrl", printerUrl);

            return printerUrl;
        }
    }
}