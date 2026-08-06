using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using System.Runtime.InteropServices;

namespace OpticaClaridad.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var usuarioId = HttpContext.Session.GetString("UsuarioId");
        if (string.IsNullOrEmpty(usuarioId))
            return RedirectToAction("Index", "Login");

        ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreCompleto");
        ViewBag.EsAdmin = HttpContext.Session.GetString("EsAdmin") == "True";
        ViewBag.FechaActual = DateTime.Now.ToString("dddd, dd MMMM yyyy");

        if (ViewBag.EsAdmin)
        {
            await CargarDatosAdmin();
            return View("IndexAdmin");
        }

        await CargarDatosVendedor();
        return View("IndexVendedor");
    }

    private async Task CargarDatosAdmin()
    {
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var finMes = inicioMes.AddMonths(1);

        // Ventas hoy
        ViewBag.VentasHoy = await _context.Ventas
            .Where(v => v.FechaVenta >= hoy && v.FechaVenta < manana)
            .SumAsync(v => (double?)v.Total) ?? 0;

        // 🔴 ELIMINADO: Total de clientes - ya no existe

        // 🔴 NUEVO: Total de pacientes activos
        ViewBag.TotalPacientes = await _context.Pacientes
            .CountAsync(p => p.Activo);

        // Total productos activos
        ViewBag.TotalProductos = await _context.Productos
            .CountAsync(p => p.Activo);

        // Productos bajo stock
        ViewBag.ProductosBajoStock = await _context.Productos
            .CountAsync(p => p.Stock <= p.StockMinimo && p.Stock > 0 && p.Activo);

        // Ventas del mes
        ViewBag.VentasMes = await _context.Ventas
            .Where(v => v.FechaVenta >= inicioMes && v.FechaVenta < finMes)
            .SumAsync(v => (double?)v.Total) ?? 0;

        // Productos agotados
        ViewBag.ProductosAgotados = await _context.Productos
            .CountAsync(p => p.Stock == 0 && p.Activo);

        // 🔴 ACTUALIZADO: Últimas ventas (usando Paciente en lugar de Cliente)
        ViewBag.UltimasVentas = await _context.Ventas
            .Include(v => v.Paciente) // 🔴 Cambio: Paciente en lugar de Cliente
            .Where(v => v.Total > 0)
            .OrderByDescending(v => v.FechaVenta)
            .Take(5)
            .Select(v => new
            {
                v.NumeroVenta,
                Paciente = v.Paciente != null ? v.Paciente.Nombre : "Cliente General", // 🔴 Cambio
                v.FechaVenta,
                v.Total
            })
            .ToListAsync();
    }

    private async Task CargarDatosVendedor()
    {
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);
        var usuarioId = HttpContext.Session.GetString("UsuarioId");

        ViewBag.VentasHoy = await _context.Ventas
            .Where(v => v.FechaVenta >= hoy && v.FechaVenta < manana
                && v.Estado == "COMPLETADA" && v.UsuarioId == usuarioId)
            .SumAsync(v => (double?)v.Total) ?? 0;

        ViewBag.CantidadVentasHoy = await _context.Ventas
            .CountAsync(v => v.FechaVenta >= hoy && v.FechaVenta < manana
                && v.Estado == "COMPLETADA" && v.UsuarioId == usuarioId);
    }

    // ============================================
    // NUEVO MÉTODO: Crear acceso directo en menú de programas
    // ============================================
    [HttpPost]
    public IActionResult CrearAccesoProgramas()
    {
        try
        {
            // ===== USAR CARPETA DE USUARIO EN VEZ DE CARPETA DEL PROYECTO =====
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OpticaClaridad");

            // Crear carpeta en AppData si no existe
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            // Ruta del menú de programas del usuario actual
            string programasPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                "Programas", "Óptica Claridad");

            if (!Directory.Exists(programasPath))
                Directory.CreateDirectory(programasPath);

            // ===== 1. COPIAR EL ICONO A LA CARPETA DE PROGRAMAS =====
            string iconSourcePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, // En producción: carpeta de publicación
                "wwwroot", "favicon.ico");

            string iconDestPath = Path.Combine(programasPath, "icono.ico");

            if (System.IO.File.Exists(iconSourcePath))
            {
                System.IO.File.Copy(iconSourcePath, iconDestPath, true);
            }

            // ===== 2. CREAR EL ARCHIVO .BAT EN AppData =====
            string batPath = Path.Combine(appDataPath, "Iniciar Optica Claridad.bat");

            // En producción, necesitamos la URL completa
            string url = $"{Request.Scheme}://{Request.Host}";

            // Contenido del .bat adaptado para producción
            string batContent = $@"@echo off
title Óptica Claridad
color 0A
echo ========================================
echo   ÓPTICA CLARIDAD - SISTEMA WEB
echo ========================================
echo.

:: Abrir directamente la URL en Chrome
echo 🌐 Abriendo la aplicación...
start chrome --app={url} --window-size=1300,800

echo.
echo ✅ Aplicación abierta correctamente
echo.
echo Presiona cualquier tecla para cerrar...
pause >nul";

            System.IO.File.WriteAllText(batPath, batContent, System.Text.Encoding.Default);

            // ===== 3. CREAR EL ACCESO DIRECTO QUE APUNTA AL .BAT =====
            string shortcutPath = Path.Combine(programasPath, "Óptica Claridad.lnk");

            dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
            dynamic shortcut = shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = batPath;
            shortcut.Description = "Óptica Claridad - Sistema de Gestión";

            if (System.IO.File.Exists(iconDestPath))
            {
                shortcut.IconLocation = $"{iconDestPath},0";
            }
            else
            {
                shortcut.IconLocation = "chrome.exe,0";
            }

            shortcut.WorkingDirectory = appDataPath;
            shortcut.Save();

            Marshal.ReleaseComObject(shortcut);
            Marshal.ReleaseComObject(shell);

            return Json(new
            {
                success = true,
                message = "✅ Instalación completa en modo producción:\n\n" +
                         $"El acceso directo se creó en:\nInicio > Programas > Óptica Claridad\n\n" +
                         $"Archivos instalados en:\n{appDataPath}"
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    // ============================================
    // MÉTODO ADICIONAL: Crear acceso directo en escritorio
    // ============================================
    [HttpPost]
    public IActionResult CrearAccesoEscritorio()
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string programasPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                "Programas", "Óptica Claridad");

            // Verificar si ya existe la instalación en programas
            string batPath = Path.Combine(programasPath, "Iniciar Optica Claridad.bat");

            // Si no existe, primero crear la instalación en programas
            if (!System.IO.File.Exists(batPath))
            {
                var result = CrearAccesoProgramas();
                // Esperar un momento para que se complete
                System.Threading.Thread.Sleep(1000);
            }

            // Usar el mismo .bat de programas
            string shortcutPath = Path.Combine(desktopPath, "Óptica Claridad.lnk");
            string iconDestPath = Path.Combine(programasPath, "icono.ico");

            dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
            dynamic shortcut = shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = batPath;
            shortcut.Description = "Óptica Claridad - Sistema de Gestión";

            if (System.IO.File.Exists(iconDestPath))
            {
                shortcut.IconLocation = $"{iconDestPath},0";
            }
            else
            {
                shortcut.IconLocation = "chrome.exe,0";
            }

            shortcut.WorkingDirectory = programasPath;
            shortcut.Save();

            Marshal.ReleaseComObject(shortcut);
            Marshal.ReleaseComObject(shell);

            return Json(new
            {
                success = true,
                message = "✅ Acceso directo creado en el escritorio"
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    public IActionResult Error() => View();
}