using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpticaClaridad.Controllers
{
    // ==============================================
    // MODELOS (deben estar FUERA de la clase del controller)
    // ==============================================
    public class BackupInfo
    {
        public string NombreArchivo { get; set; }
        public string RutaCompleta { get; set; }
        public double TamanoKB { get; set; }
        public double TamanoMB { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public int DiasDesdeCreacion { get; set; }
    }

    public class BackupEstadisticas
    {
        public int TotalBackups { get; set; }
        public double TamanoTotalMB { get; set; }
        public DateTime BackupMasAntiguo { get; set; }
        public DateTime BackupMasReciente { get; set; }
        public int BackupsHoy { get; set; }
        public string RutaBackups { get; set; }
        public int MantenerDias { get; set; }
        public double EspacioDiscoGB { get; set; }
    }

    // ==============================================
    // CONTROLADOR
    // ==============================================
    public class BackupController : Controller
    {
        // RUTA CONFIGURABLE
        private readonly string _rutaBaseDatos;
        private readonly string _rutaBackups;
        private readonly int _mantenerDias;

        public BackupController()
        {
            _rutaBaseDatos = Path.Combine(Directory.GetCurrentDirectory(), "opticaclaridad.db");

            // ✅ CAMBIA ESTA RUTA A DONDE QUIERAS
            _rutaBackups = @"C:\BackupOptica";  // Cambia aquí la ubicación
            _mantenerDias = 30; // Mantener backups por 30 días

            // Crear carpeta de backups si no existe
            if (!Directory.Exists(_rutaBackups))
                Directory.CreateDirectory(_rutaBackups);
        }

        // ==============================================
        // MÉTODOS AUXILIARES
        // ==============================================
        private bool EstaAutenticado()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));
        }

        private bool EsAdministrador()
        {
            return HttpContext.Session.GetString("EsAdmin") == "True";
        }

        private List<BackupInfo> ObtenerBackups()
        {
            var backups = new List<BackupInfo>();

            if (Directory.Exists(_rutaBackups))
            {
                var archivos = Directory.GetFiles(_rutaBackups, "opticaclaridad_backup_*.db")
                    .OrderByDescending(f => new FileInfo(f).CreationTime);

                foreach (var archivo in archivos)
                {
                    var info = new FileInfo(archivo);
                    backups.Add(new BackupInfo
                    {
                        NombreArchivo = Path.GetFileName(archivo),
                        RutaCompleta = archivo,
                        TamanoKB = Math.Round(info.Length / 1024.0, 2),
                        TamanoMB = Math.Round(info.Length / (1024.0 * 1024.0), 2),
                        FechaCreacion = info.CreationTime,
                        FechaModificacion = info.LastWriteTime,
                        DiasDesdeCreacion = (DateTime.Now - info.CreationTime).Days
                    });
                }
            }

            return backups;
        }

        private BackupEstadisticas CalcularEstadisticas(List<BackupInfo> backups)
        {
            var estadisticas = new BackupEstadisticas();

            if (backups.Any())
            {
                estadisticas.TotalBackups = backups.Count;
                estadisticas.TamanoTotalMB = backups.Sum(b => b.TamanoMB);
                estadisticas.BackupMasAntiguo = backups.Min(b => b.FechaCreacion);
                estadisticas.BackupMasReciente = backups.Max(b => b.FechaCreacion);
                estadisticas.BackupsHoy = backups.Count(b => b.FechaCreacion.Date == DateTime.Today);
            }

            estadisticas.RutaBackups = _rutaBackups;
            estadisticas.MantenerDias = _mantenerDias;
            estadisticas.EspacioDiscoGB = GetEspacioDiscoDisponible();

            return estadisticas;
        }

        private double GetEspacioDiscoDisponible()
        {
            try
            {
                var rutaRaiz = Path.GetPathRoot(_rutaBackups) ?? Directory.GetCurrentDirectory();
                var drive = new DriveInfo(rutaRaiz);
                return Math.Round(drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0), 2);
            }
            catch
            {
                return 0;
            }
        }

        // ==============================================
        // ACCIONES DEL CONTROLADOR
        // ==============================================

        // GET: Backup/Index
        public IActionResult Index()
        {
            if (!EstaAutenticado())
                return RedirectToAction("Index", "Login");

            var backups = ObtenerBackups();
            var estadisticas = CalcularEstadisticas(backups);

            ViewBag.RutaBaseDatos = _rutaBaseDatos;
            ViewBag.ExisteBaseDatos = System.IO.File.Exists(_rutaBaseDatos);
            ViewBag.RutaBackups = _rutaBackups;
            ViewBag.EsAdmin = EsAdministrador();
            ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreCompleto");
            ViewBag.Estadisticas = estadisticas;

            return View(backups);
        }

        // POST: Backup/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear()
        {
            if (!EstaAutenticado())
                return RedirectToAction("Index", "Login");

            try
            {
                if (!System.IO.File.Exists(_rutaBaseDatos))
                {
                    TempData["Error"] = "No se encontró la base de datos principal.";
                    return RedirectToAction("Index");
                }

                var fecha = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreBackup = $"opticaclaridad_backup_{fecha}.db";
                var rutaDestino = Path.Combine(_rutaBackups, nombreBackup);

                // Copiar la base de datos
                System.IO.File.Copy(_rutaBaseDatos, rutaDestino, false);

                TempData["Success"] = $"✅ Backup creado exitosamente<br>Ubicación: {rutaDestino}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Error al crear backup: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: Backup/Descargar/{nombreArchivo}
        public IActionResult Descargar(string nombreArchivo)
        {
            if (!EstaAutenticado())
                return RedirectToAction("Index", "Login");

            try
            {
                var rutaArchivo = Path.Combine(_rutaBackups, nombreArchivo);

                if (!System.IO.File.Exists(rutaArchivo))
                {
                    TempData["Error"] = "El archivo de backup no existe.";
                    return RedirectToAction("Index");
                }

                var bytes = System.IO.File.ReadAllBytes(rutaArchivo);
                return File(bytes, "application/octet-stream", nombreArchivo);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al descargar: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Backup/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(string nombreArchivo)
        {
            if (!EstaAutenticado() || !EsAdministrador())
            {
                TempData["Error"] = "No tiene permisos para realizar esta acción.";
                return RedirectToAction("Index");
            }

            try
            {
                var rutaArchivo = Path.Combine(_rutaBackups, nombreArchivo);

                if (!System.IO.File.Exists(rutaArchivo))
                {
                    TempData["Error"] = "El archivo de backup no existe.";
                    return RedirectToAction("Index");
                }

                System.IO.File.Delete(rutaArchivo);
                TempData["Success"] = $"Backup eliminado: {nombreArchivo}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: Backup/AbrirCarpeta
        public IActionResult AbrirCarpeta()
        {
            if (!EstaAutenticado())
                return RedirectToAction("Index", "Login");

            try
            {
                if (Directory.Exists(_rutaBackups))
                {
                    // Abrir carpeta en el explorador de Windows
                    System.Diagnostics.Process.Start("explorer.exe", _rutaBackups);
                    TempData["Success"] = $"Carpeta abierta: {_rutaBackups}";
                }
                else
                {
                    TempData["Error"] = $"La carpeta no existe: {_rutaBackups}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al abrir carpeta: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: Backup/Estadisticas
        public IActionResult Estadisticas()
        {
            if (!EstaAutenticado())
                return RedirectToAction("Index", "Login");

            var backups = ObtenerBackups();
            var estadisticas = CalcularEstadisticas(backups);

            ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreCompleto");
            ViewBag.EsAdmin = EsAdministrador();

            return View(estadisticas);
        }

        // POST: Backup/LimpiarAntiguos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LimpiarAntiguos()
        {
            if (!EstaAutenticado() || !EsAdministrador())
            {
                TempData["Error"] = "No tiene permisos para realizar esta acción.";
                return RedirectToAction("Index");
            }

            try
            {
                int eliminados = 0;
                var fechaLimite = DateTime.Now.AddDays(-_mantenerDias);

                if (Directory.Exists(_rutaBackups))
                {
                    var archivos = Directory.GetFiles(_rutaBackups, "opticaclaridad_backup_*.db");

                    foreach (var archivo in archivos)
                    {
                        var info = new FileInfo(archivo);
                        if (info.CreationTime < fechaLimite)
                        {
                            System.IO.File.Delete(archivo);
                            eliminados++;
                        }
                    }
                }

                TempData["Success"] = $"Se eliminaron {eliminados} backups antiguos (mayores a {_mantenerDias} días).";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al limpiar backups antiguos: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}