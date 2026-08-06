using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using OpticaClaridad.Models;

namespace OpticaClaridad.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AccesoConfig _config;
        private readonly ILogger<AccesoController> _logger;

        public AccesoController(IOptions<AccesoConfig> config, ILogger<AccesoController> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        // =============================================
        // OBTENER IP LOCAL DE LA MÁQUINA (SIMPLE)
        // =============================================
        private string ObtenerIpLocal()
        {
            try
            {
                // Método 1: Usar socket para obtener IP real
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    if (endPoint != null)
                    {
                        return endPoint.Address.ToString();
                    }
                }
            }
            catch
            {
                // Método 2: Buscar primera IP que no sea loopback
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }

            return "localhost";
        }

        // =============================================
        // OBTENER URL COMPLETA
        // =============================================
        private string ObtenerUrlAcceso()
        {
            string ip = ObtenerIpLocal();
            return $"http://{ip}:{_config.Puerto}/Login";
        }

        // =============================================
        // ACCIONES
        // =============================================

        // GET: /Acceso/QR
        public IActionResult QR()
        {
            string ip = ObtenerIpLocal();
            string url = $"http://{ip}:{_config.Puerto}/Login";

            ViewBag.AppUrl = url;
            ViewBag.IpServidor = ip;
            ViewBag.Puerto = _config.Puerto;
            ViewBag.AppName = _config.NombreAplicacion;
            ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            return View();
        }

        // GET: /Acceso/QRCodeImage
        public IActionResult QRCodeImage()
        {
            string ip = ObtenerIpLocal();
            string url = $"http://{ip}:{_config.Puerto}/Login";

            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

                using (var qrCode = new QRCode(qrCodeData))
                using (var bitmap = qrCode.GetGraphic(20))
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    return File(stream.ToArray(), "image/png");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generando QR: {ex.Message}");
                return BadRequest("Error generando código QR");
            }
        }

        // GET: /Acceso/Info
        public IActionResult Info()
        {
            string ip = ObtenerIpLocal();
            string url = $"http://{ip}:{_config.Puerto}/Login";

            ViewBag.AppUrl = url;
            ViewBag.AppName = _config.NombreAplicacion;
            ViewBag.IpServidor = ip;
            ViewBag.Puerto = _config.Puerto;

            return View();
        }

        // GET: /Acceso/Dispositivos
        public IActionResult Dispositivos()
        {
            string ip = ObtenerIpLocal();
            string url = $"http://{ip}:{_config.Puerto}/Login";

            ViewBag.IpServidor = ip;
            ViewBag.Puerto = _config.Puerto;
            ViewBag.UrlCompleta = url;
            ViewBag.FechaDeteccion = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            return View();
        }

        // GET: /Acceso/Test
        public IActionResult Test()
        {
            string ip = ObtenerIpLocal();
            string url = $"http://{ip}:{_config.Puerto}/Login";

            return Content($"URL generada: {url}\nIP: {ip}\nPuerto: {_config.Puerto}");
        }
    }
}