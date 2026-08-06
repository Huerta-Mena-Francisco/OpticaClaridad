using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OpticaClaridad.Controllers
{
    public class TunelController : Controller
    {
        private readonly string _cloudflarePath = @"C:\cloudflare";
        private readonly string _batFile = @"C:\cloudflare\bd_tunel_optica.bat";
        private readonly string _urlFile = @"C:\ArchivosOptica\tunel_url_optica.txt";
        private readonly string _logFile = @"C:\ArchivosOptica\tunel_log_optica.txt";

        // GET: Tunel
        public IActionResult Index()
        {
            bool isActive = System.IO.File.Exists(_urlFile);
            string? url = null;

            if (isActive)
            {
                url = System.IO.File.ReadAllText(_urlFile).Trim();
            }

            ViewBag.IsActive = isActive;
            ViewBag.Url = url;

            return View();
        }

        // POST: Tunel/Iniciar
        [HttpPost]
        public async Task<IActionResult> Iniciar()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INICIANDO TÚNEL ÓPTICA CLARIDAD ===");

                // Limpiar archivo anterior
                if (System.IO.File.Exists(_urlFile))
                {
                    System.IO.File.Delete(_urlFile);
                }

                // Ejecutar el script en segundo plano
                Task.Run(() =>
                {
                    try
                    {
                        var processInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c \"{_batFile}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };
                        using (var process = Process.Start(processInfo))
                        {
                            process?.WaitForExit();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al ejecutar túnel: {ex.Message}");
                    }
                });

                // Esperar a que se cree el archivo (hasta 30 segundos)
                int espera = 0;
                while (!System.IO.File.Exists(_urlFile) && espera < 30)
                {
                    await Task.Delay(1000);
                    espera++;
                }

                if (System.IO.File.Exists(_urlFile))
                {
                    string url = System.IO.File.ReadAllText(_urlFile).Trim();

                    // Limpiar la URL
                    var regex = new Regex(@"https:\/\/[a-zA-Z0-9\-]+\.trycloudflare\.com");
                    var match = regex.Match(url);
                    if (match.Success)
                    {
                        url = match.Value;
                        System.IO.File.WriteAllText(_urlFile, url);
                    }

                    if (!string.IsNullOrEmpty(url) && url.StartsWith("https://"))
                    {
                        return Ok(new { success = true, url = url });
                    }
                }

                return BadRequest(new { success = false, message = "No se pudo crear el túnel" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST: Tunel/Detener
        [HttpPost]
        public IActionResult Detener()
        {
            try
            {
                // Matar procesos de cloudflared
                var processes = Process.GetProcessesByName("cloudflared");
                foreach (var proc in processes)
                {
                    proc.Kill();
                    proc.WaitForExit();
                }

                // Limpiar archivo de URL
                if (System.IO.File.Exists(_urlFile))
                {
                    System.IO.File.Delete(_urlFile);
                }

                return Ok(new { success = true, message = "Túnel detenido" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: Tunel/Estado
        [HttpGet]
        public IActionResult Estado()
        {
            bool isActive = System.IO.File.Exists(_urlFile);
            string? url = null;

            if (isActive)
            {
                url = System.IO.File.ReadAllText(_urlFile).Trim();
            }

            return Ok(new { active = isActive, url = url });
        }
    }
}