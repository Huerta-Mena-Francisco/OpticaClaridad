using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using OpticaClaridad.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpticaClaridad.Controllers
{
    public class PacientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PacientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Pacientes
        // GET: Pacientes/Index
        public async Task<IActionResult> Index(string searchString, string sexoFilter, string estadoFilter)
        {
            var query = _context.Pacientes
                .Include(p => p.ExamenesVisuales)
                .Include(p => p.Creditos)  // 🔴 NUEVO: Incluir créditos
                .AsQueryable();

            // Filtros
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(p =>
                    p.Nombre.ToLower().Contains(searchString) ||
                    p.Telefono.Contains(searchString) ||
                    p.Email.ToLower().Contains(searchString));
            }

            if (!string.IsNullOrEmpty(sexoFilter))
            {
                query = query.Where(p => p.Sexo == sexoFilter);
            }

            if (!string.IsNullOrEmpty(estadoFilter))
            {
                bool activo = estadoFilter == "Activo";
                query = query.Where(p => p.Activo == activo);
            }

            var pacientes = await query
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            // ✅ DEBUG: Verificar que los créditos se cargaron
            foreach (var p in pacientes)
            {
                Console.WriteLine($"Paciente: {p.Nombre}, Créditos: {p.Creditos?.Count ?? 0}");
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["SexoFilter"] = sexoFilter;
            ViewData["EstadoFilter"] = estadoFilter;

            return View(pacientes);
        }

        // GET: Pacientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paciente = await _context.Pacientes
                .Include(p => p.Ventas)
                .Include(p => p.ExamenesVisuales)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (paciente == null)
            {
                return NotFound();
            }

            // Obtener el último historial clínico
            var ultimoHistorial = await _context.HistorialesClinicos
                .Where(h => h.PacienteId == id)
                .OrderByDescending(h => h.FechaExamen)
                .FirstOrDefaultAsync();

            ViewBag.UltimoHistorial = ultimoHistorial;

            return View(paciente);
        }

        // GET: Pacientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pacientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Telefono,Email,Direccion,FechaNacimiento,Sexo,Observaciones,Activo")] Paciente paciente)
        {
            if (ModelState.IsValid)
            {
                paciente.FechaRegistro = DateTime.Today;

                _context.Add(paciente);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Paciente {paciente.Nombre} creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(paciente);
        }

        // GET: Pacientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente == null)
            {
                return NotFound();
            }
            return View(paciente);
        }

        // POST: Pacientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Telefono,Email,Direccion,FechaNacimiento,Sexo,Observaciones,Activo,FechaRegistro")] Paciente paciente)
        {
            if (id != paciente.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paciente);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Paciente {paciente.Nombre} actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PacienteExists(paciente.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(paciente);
        }

        // GET: Pacientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (paciente == null)
            {
                return NotFound();
            }

            // Verificar si tiene historiales clínicos
            var tieneHistoriales = await _context.HistorialesClinicos
                .AnyAsync(h => h.PacienteId == id);

            ViewBag.TieneHistoriales = tieneHistoriales;

            return View(paciente);
        }

        // POST: Pacientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);

            if (paciente != null)
            {
                // Verificar si tiene historiales clínicos antes de eliminar
                var tieneHistoriales = await _context.HistorialesClinicos
                    .AnyAsync(h => h.PacienteId == id);

                if (tieneHistoriales)
                {
                    // En lugar de eliminar, marcamos como inactivo
                    paciente.Activo = false;
                    _context.Update(paciente);
                    TempData["SuccessMessage"] = $"Paciente {paciente.Nombre} marcado como inactivo (tiene historiales clínicos).";
                }
                else
                {
                    // Si no tiene historiales, eliminamos
                    _context.Pacientes.Remove(paciente);
                    TempData["SuccessMessage"] = $"Paciente {paciente.Nombre} eliminado exitosamente.";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PacienteExists(int id)
        {
            return _context.Pacientes.Any(e => e.Id == id);
        }

        // GET: Pacientes/Activar/5
        public async Task<IActionResult> Activar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente == null)
            {
                return NotFound();
            }

            paciente.Activo = true;
            _context.Update(paciente);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Paciente {paciente.Nombre} activado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}