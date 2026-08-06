using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using OpticaClaridad.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpticaClaridad.Controllers
{
    public class HistorialClinicoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistorialClinicoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: HistorialClinico/Index?pacienteId=5
        public async Task<IActionResult> Index(int? pacienteId, string searchDate)
        {
            ViewBag.PacienteId = pacienteId;

            var historiales = _context.HistorialesClinicos
                .Include(h => h.Paciente)
                .AsQueryable();

            if (pacienteId.HasValue)
            {
                historiales = historiales.Where(h => h.PacienteId == pacienteId.Value);
                var paciente = await _context.Pacientes.FindAsync(pacienteId.Value);
                ViewBag.PacienteNombre = paciente?.Nombre ?? "Paciente";
            }

            if (!string.IsNullOrEmpty(searchDate) && DateTime.TryParse(searchDate, out DateTime fecha))
            {
                historiales = historiales.Where(h => h.FechaExamen.Date == fecha.Date);
            }

            historiales = historiales.OrderByDescending(h => h.FechaExamen);

            return View(await historiales.ToListAsync());
        }

        // GET: HistorialClinico/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historialClinico = await _context.HistorialesClinicos
                .Include(h => h.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (historialClinico == null)
            {
                return NotFound();
            }

            return View(historialClinico);
        }

        // GET: HistorialClinico/Create?pacienteId=5
        public async Task<IActionResult> Create(int? pacienteId)
        {
            // Obtener el nombre del usuario desde la sesión
            var nombreUsuario = HttpContext.Session.GetString("NombreCompleto") ?? "Opt. Principal";
            ViewBag.NombreUsuario = nombreUsuario;

            if (pacienteId.HasValue)
            {
                var paciente = await _context.Pacientes.FindAsync(pacienteId.Value);
                if (paciente != null)
                {
                    ViewBag.PacienteId = pacienteId.Value;
                    ViewBag.PacienteNombre = paciente.Nombre;

                    // Crear un nuevo historial con valores predeterminados
                    var nuevoHistorial = new HistorialClinico
                    {
                        PacienteId = pacienteId.Value,
                        FechaExamen = DateTime.Today,
                        ExamenPagado = true,
                        Optometrista = nombreUsuario, // Aquí ya usamos el nombre real
                        GraduacionAnterior = false // Por defecto no tiene graduación anterior
                    };

                    return View(nuevoHistorial);
                }
            }

            // Si no hay pacienteId, mostrar lista de pacientes para seleccionar
            ViewBag.PacienteId = new SelectList(await _context.Pacientes.Where(p => p.Activo).ToListAsync(), "Id", "Nombre");

            var historialVacio = new HistorialClinico
            {
                Optometrista = nombreUsuario // Asignar el nombre también aquí
            };

            return View(historialVacio);
        }

        // POST: HistorialClinico/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PacienteId,FechaExamen,HoraExamen,MotivoExamen,TipoExamen,CostoExamen,ExamenPagado,TieneDiabetes,DiabetesTratamiento,TieneHipertension,HipertensionTratamiento,TieneAlergias,OtrasPatologias,UsaLentes,TipoLentesActuales,AntecedentesOculares,DetalleAntecedentes,EsferaOD,CilindroOD,EjeOD,EsPlanoOD,CilindroPlanoOD,EsferaOI,CilindroOI,EjeOI,EsPlanoOI,CilindroPlanoOI,Adicion,DistanciaPupilar,AgudezaVisualODLejos,AgudezaVisualOILejos,AgudezaVisualODCerca,AgudezaVisualOICerca,ObservacionesExamen,Recomendaciones,NecesitaSeguimiento,FechaProximoControl,InteresEnCompra,ObservacionesInteres,Optometrista,GraduacionAnterior,ODEsfAnterior,ODCilAnterior,ODEjeAnterior,ODAddAnterior,OIEsfAnterior,OICilAnterior,OIEjeAnterior,OIAddAnterior")] HistorialClinico historialClinico)
        {
            // Validación personalizada: Si es Plano, la esfera debe ser null o 0
            if (historialClinico.EsPlanoOD)
            {
                historialClinico.EsferaOD = null;
            }

            if (historialClinico.EsPlanoOI)
            {
                historialClinico.EsferaOI = null;
            }

            if (historialClinico.CilindroPlanoOD)
            {
                historialClinico.CilindroOD = null;
            }

            if (historialClinico.CilindroPlanoOI)
            {
                historialClinico.CilindroOI = null;
            }

            // Si no tiene graduación anterior, limpiar los campos
            if (!historialClinico.GraduacionAnterior)
            {
                historialClinico.ODEsfAnterior = null;
                historialClinico.ODCilAnterior = null;
                historialClinico.ODEjeAnterior = null;
                historialClinico.ODAddAnterior = null;
                historialClinico.OIEsfAnterior = null;
                historialClinico.OICilAnterior = null;
                historialClinico.OIEjeAnterior = null;
                historialClinico.OIAddAnterior = null;
            }

            // Si el campo Optometrista está vacío, asignar el nombre de la sesión
            if (string.IsNullOrWhiteSpace(historialClinico.Optometrista))
            {
                historialClinico.Optometrista = HttpContext.Session.GetString("NombreCompleto") ?? "Opt. Principal";
            }

            if (ModelState.IsValid)
            {
                historialClinico.FechaRegistro = DateTime.Now;

                _context.Add(historialClinico);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Historial clínico creado exitosamente.";
                return RedirectToAction(nameof(Index), new { pacienteId = historialClinico.PacienteId });
            }

            // Recargar datos del paciente si hay error
            if (historialClinico.PacienteId > 0)
            {
                var paciente = await _context.Pacientes.FindAsync(historialClinico.PacienteId);
                ViewBag.PacienteNombre = paciente?.Nombre;
            }

            // Volver a asignar el nombre para la vista en caso de error
            ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreCompleto") ?? "Opt. Principal";

            return View(historialClinico);
        }

        // GET: HistorialClinico/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historialClinico = await _context.HistorialesClinicos
                .Include(h => h.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (historialClinico == null)
            {
                return NotFound();
            }

            ViewBag.PacienteNombre = historialClinico.Paciente?.Nombre;
            return View(historialClinico);
        }

        // POST: HistorialClinico/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PacienteId,FechaExamen,HoraExamen,MotivoExamen,TipoExamen,CostoExamen,ExamenPagado,TieneDiabetes,DiabetesTratamiento,TieneHipertension,HipertensionTratamiento,TieneAlergias,OtrasPatologias,UsaLentes,TipoLentesActuales,AntecedentesOculares,DetalleAntecedentes,EsferaOD,CilindroOD,EjeOD,EsPlanoOD,CilindroPlanoOD,EsferaOI,CilindroOI,EjeOI,EsPlanoOI,CilindroPlanoOI,Adicion,DistanciaPupilar,AgudezaVisualODLejos,AgudezaVisualOILejos,AgudezaVisualODCerca,AgudezaVisualOICerca,ObservacionesExamen,Recomendaciones,NecesitaSeguimiento,FechaProximoControl,InteresEnCompra,ObservacionesInteres,Optometrista,FechaRegistro,GraduacionAnterior,ODEsfAnterior,ODCilAnterior,ODEjeAnterior,ODAddAnterior,OIEsfAnterior,OICilAnterior,OIEjeAnterior,OIAddAnterior")] HistorialClinico historialClinico)
        {
            if (id != historialClinico.Id)
            {
                return NotFound();
            }

            // Validación personalizada: Si es Plano, la esfera debe ser null o 0
            if (historialClinico.EsPlanoOD)
            {
                historialClinico.EsferaOD = null;
            }

            if (historialClinico.EsPlanoOI)
            {
                historialClinico.EsferaOI = null;
            }

            if (historialClinico.CilindroPlanoOD)
            {
                historialClinico.CilindroOD = null;
            }

            if (historialClinico.CilindroPlanoOI)
            {
                historialClinico.CilindroOI = null;
            }

            // Si no tiene graduación anterior, limpiar los campos
            if (!historialClinico.GraduacionAnterior)
            {
                historialClinico.ODEsfAnterior = null;
                historialClinico.ODCilAnterior = null;
                historialClinico.ODEjeAnterior = null;
                historialClinico.ODAddAnterior = null;
                historialClinico.OIEsfAnterior = null;
                historialClinico.OICilAnterior = null;
                historialClinico.OIEjeAnterior = null;
                historialClinico.OIAddAnterior = null;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    historialClinico.FechaModificacion = DateTime.Now;
                    _context.Update(historialClinico);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Historial clínico actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HistorialClinicoExists(historialClinico.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { pacienteId = historialClinico.PacienteId });
            }

            // Recargar datos del paciente si hay error
            var paciente = await _context.Pacientes.FindAsync(historialClinico.PacienteId);
            ViewBag.PacienteNombre = paciente?.Nombre;

            return View(historialClinico);
        }

        // GET: HistorialClinico/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historialClinico = await _context.HistorialesClinicos
                .Include(h => h.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (historialClinico == null)
            {
                return NotFound();
            }

            return View(historialClinico);
        }

        // POST: HistorialClinico/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var historialClinico = await _context.HistorialesClinicos.FindAsync(id);
            if (historialClinico != null)
            {
                var pacienteId = historialClinico.PacienteId;
                _context.HistorialesClinicos.Remove(historialClinico);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Historial clínico eliminado exitosamente.";
                return RedirectToAction(nameof(Index), new { pacienteId });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool HistorialClinicoExists(int id)
        {
            return _context.HistorialesClinicos.Any(e => e.Id == id);
        }

        // GET: HistorialClinico/Reporte/5
        public async Task<IActionResult> Reporte(int id)
        {
            var historialClinico = await _context.HistorialesClinicos
                .Include(h => h.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (historialClinico == null)
            {
                return NotFound();
            }

            return View(historialClinico);
        }
    }
}