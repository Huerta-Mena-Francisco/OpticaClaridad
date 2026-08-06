using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using OpticaClaridad.Models;
using System.Diagnostics;

namespace OpticaClaridad.Controllers
{
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Citas
        public async Task<IActionResult> Index(string estado = null, DateTime? fecha = null)
        {
            IQueryable<Cita> query = _context.Citas.Where(c => !c.Eliminada); // Solo citas no eliminadas

            // Filtrar por estado si se especifica
            if (!string.IsNullOrEmpty(estado) && estado != "Todos")
            {
                query = query.Where(c => c.Estado == estado);
            }

            // Filtrar por fecha si se especifica
            if (fecha.HasValue)
            {
                query = query.Where(c => c.FechaHora.Date == fecha.Value.Date);
            }
            else
            {
                // Por defecto mostrar citas de hoy en adelante
                query = query.Where(c => c.FechaHora.Date >= DateTime.Today);
            }

            // Ordenar por fecha más próxima
            var citas = await query.OrderBy(c => c.FechaHora).ToListAsync();

            ViewBag.EstadoSeleccionado = estado ?? "Todos";
            ViewBag.FechaSeleccionada = fecha;
            ViewBag.Hoy = DateTime.Today;

            return View(citas);
        }

        // GET: Citas/Hoy
        public async Task<IActionResult> Hoy()
        {
            var citasHoy = await _context.Citas
                .Where(c => c.FechaHora.Date == DateTime.Today &&
                          c.Estado != "Cancelada" &&
                          !c.Eliminada)
                .OrderBy(c => c.FechaHora)
                .ToListAsync();

            ViewBag.Titulo = "Citas para Hoy";
            return View("Index", citasHoy);
        }

        // GET: Citas/Calendario
        public async Task<IActionResult> Calendario(DateTime? mes = null)
        {
            var fecha = mes ?? DateTime.Today;
            var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var citasMes = await _context.Citas
                .Where(c => c.FechaHora.Date >= inicioMes &&
                          c.FechaHora.Date <= finMes &&
                          !c.Eliminada)
                .OrderBy(c => c.FechaHora)
                .ToListAsync();

            ViewBag.MesActual = fecha;
            ViewBag.MesAnterior = fecha.AddMonths(-1);
            ViewBag.MesSiguiente = fecha.AddMonths(1);

            return View(citasMes);
        }

        // GET: Citas/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Citas.FindAsync(id);
            if (cita == null || cita.Eliminada)
            {
                return NotFound();
            }

            return View(cita);
        }

        // GET: Citas/Crear
        public IActionResult Crear()
        {
            // Establecer fecha por defecto (próximos 30 minutos desde ahora)
            var ahora = DateTime.Now;
            var minutos = ahora.Minute;
            var minutosRedondeados = Math.Ceiling(minutos / 30.0) * 30;

            // Si estamos cerca del final de la hora, redondear a la siguiente media hora
            if (minutosRedondeados == 60)
            {
                ahora = ahora.AddHours(1);
                minutosRedondeados = 0;
            }

            var fechaHoraDefecto = new DateTime(ahora.Year, ahora.Month, ahora.Day, ahora.Hour, (int)minutosRedondeados, 0);

            // Si la fecha calculada es anterior a ahora (por redondeo hacia abajo), sumar 30 minutos
            if (fechaHoraDefecto < DateTime.Now)
            {
                fechaHoraDefecto = fechaHoraDefecto.AddMinutes(30);
            }

            var cita = new Cita
            {
                FechaHora = fechaHoraDefecto
            };

            return View(cita);
        }

        // POST: Citas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Cita cita)
        {
            // Validar que la fecha no sea anterior a hoy
            if (cita.FechaHora.Date < DateTime.Today)
            {
                ModelState.AddModelError("FechaHora", "No se pueden crear citas para fechas pasadas");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si ya existe cita a la misma hora (solo no eliminadas)
                    var existeCita = await _context.Citas
                        .AnyAsync(c => c.FechaHora == cita.FechaHora &&
                                     c.Estado != "Cancelada" &&
                                     !c.Eliminada);

                    if (existeCita)
                    {
                        ModelState.AddModelError("FechaHora", "Ya existe una cita programada para esta fecha y hora");
                        return View(cita);
                    }

                    cita.FechaCreacion = DateTime.Now;
                    cita.Eliminada = false;
                    _context.Add(cita);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cita creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al crear cita: {ex.Message}");
                    ModelState.AddModelError("", "Error al crear la cita");
                }
            }
            return View(cita);
        }

        // GET: Citas/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Citas.FindAsync(id);
            if (cita == null || cita.Eliminada)
            {
                return NotFound();
            }
            return View(cita);
        }

        // POST: Citas/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Cita cita)
        {
            if (id != cita.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar conflicto de horario (excluyendo la cita actual y las eliminadas)
                    var existeCita = await _context.Citas
                        .AnyAsync(c => c.FechaHora == cita.FechaHora
                                     && c.Id != id
                                     && c.Estado != "Cancelada"
                                     && !c.Eliminada);

                    if (existeCita)
                    {
                        ModelState.AddModelError("FechaHora", "Ya existe otra cita programada para esta fecha y hora");
                        return View(cita);
                    }

                    _context.Update(cita);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cita actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CitaExists(cita.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al editar cita: {ex.Message}");
                    ModelState.AddModelError("", "Error al actualizar la cita");
                }
            }
            return View(cita);
        }

        // GET: Citas/Cancelar/5
        public async Task<IActionResult> Cancelar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Citas.FindAsync(id);
            if (cita == null || cita.Eliminada)
            {
                return NotFound();
            }

            return View(cita);
        }

        // POST: Citas/Cancelar/5
        [HttpPost, ActionName("Cancelar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarConfirmado(int id)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita != null && !cita.Eliminada)
                {
                    cita.Estado = "Cancelada";
                    _context.Update(cita);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cita cancelada exitosamente";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cancelar cita: {ex.Message}");
                TempData["ErrorMessage"] = "Error al cancelar la cita";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Citas/Confirmar/5
        public async Task<IActionResult> Confirmar(int id)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita != null && !cita.Eliminada)
                {
                    cita.Estado = "Confirmada";
                    _context.Update(cita);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cita confirmada exitosamente";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al confirmar cita: {ex.Message}");
                TempData["ErrorMessage"] = "Error al confirmar la cita";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Citas/Completar/5
        public async Task<IActionResult> Completar(int id)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita != null && !cita.Eliminada)
                {
                    cita.Estado = "Atendida";
                    _context.Update(cita);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cita marcada como atendida";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al completar cita: {ex.Message}");
                TempData["ErrorMessage"] = "Error al completar la cita";
            }

            return RedirectToAction(nameof(Index));
        }

        // ================================================
        // MÉTODOS PARA GESTIÓN DE ELIMINACIÓN
        // ================================================

        // GET: Citas/EliminarLogicamente/5 (Marcar como eliminada - mover a papelera)
        public async Task<IActionResult> EliminarLogicamente(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Citas.FindAsync(id);
            if (cita == null || cita.Eliminada)
            {
                return NotFound();
            }

            return View(cita);
        }

        // POST: Citas/EliminarLogicamente/5
        [HttpPost, ActionName("EliminarLogicamente")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarLogicamenteConfirmado(int id)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita != null && !cita.Eliminada)
                {
                    cita.Eliminada = true;
                    cita.FechaEliminacion = DateTime.Now;
                    _context.Update(cita);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cita movida a la papelera de reciclaje";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al marcar cita como eliminada: {ex.Message}");
                TempData["ErrorMessage"] = "Error al mover la cita a la papelera";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Citas/CitasEliminadas (Ver papelera de reciclaje)
        public async Task<IActionResult> CitasEliminadas()
        {
            var citas = await _context.Citas
                .Where(c => c.Eliminada)
                .OrderByDescending(c => c.FechaEliminacion)
                .ToListAsync();

            ViewBag.TotalCitasEliminadas = citas.Count;

            return View(citas);
        }

        // GET: Citas/Restaurar/5
        public async Task<IActionResult> Restaurar(int id)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita != null && cita.Eliminada)
                {
                    cita.Eliminada = false;
                    cita.FechaEliminacion = null;
                    _context.Update(cita);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cita restaurada exitosamente";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al restaurar cita: {ex.Message}");
                TempData["ErrorMessage"] = "Error al restaurar la cita";
            }

            return RedirectToAction(nameof(CitasEliminadas));
        }

        // GET: Citas/EliminarPermanente/5 (Eliminación física)
        public async Task<IActionResult> EliminarPermanente(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Citas.FindAsync(id);
            if (cita == null || !cita.Eliminada)
            {
                return NotFound();
            }

            return View(cita);
        }

        // POST: Citas/EliminarPermanente/5
        [HttpPost, ActionName("EliminarPermanente")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPermanenteConfirmado(int id)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita != null && cita.Eliminada)
                {
                    _context.Citas.Remove(cita); // Eliminación física
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cita eliminada permanentemente de la base de datos";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al eliminar cita permanentemente: {ex.Message}");
                TempData["ErrorMessage"] = "Error al eliminar la cita permanentemente";
            }

            return RedirectToAction(nameof(CitasEliminadas));
        }

        // GET: Citas/EliminarTodasPermanente (Eliminar todas las citas eliminadas)
        public async Task<IActionResult> EliminarTodasPermanente()
        {
            try
            {
                var citasEliminadas = await _context.Citas
                    .Where(c => c.Eliminada)
                    .ToListAsync();

                int total = citasEliminadas.Count;

                if (total > 0)
                {
                    _context.Citas.RemoveRange(citasEliminadas);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Se eliminaron permanentemente {total} citas de la base de datos";
                }
                else
                {
                    TempData["InfoMessage"] = "No hay citas en la papelera para eliminar";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al eliminar todas las citas: {ex.Message}");
                TempData["ErrorMessage"] = "Error al eliminar las citas";
            }

            return RedirectToAction(nameof(CitasEliminadas));
        }

        // GET: Citas/EliminarAntiguas (Automático - citas con más de X días)
        public async Task<IActionResult> EliminarAntiguas(int dias = 365) // Por defecto 1 año
        {
            try
            {
                var fechaLimite = DateTime.Now.AddDays(-dias);

                var citasAntiguas = await _context.Citas
                    .Where(c => c.FechaHora < fechaLimite &&
                              (c.Estado == "Cancelada" || c.Estado == "Atendida") &&
                              !c.Eliminada)
                    .ToListAsync();

                int contador = 0;
                foreach (var cita in citasAntiguas)
                {
                    cita.Eliminada = true;
                    cita.FechaEliminacion = DateTime.Now;
                    contador++;
                }

                if (contador > 0)
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Se movieron {contador} citas antiguas a la papelera";
                }
                else
                {
                    TempData["InfoMessage"] = "No se encontraron citas antiguas para mover a la papelera";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al procesar citas antiguas: {ex.Message}");
                TempData["ErrorMessage"] = "Error al procesar las citas antiguas";
            }

            return RedirectToAction(nameof(CitasEliminadas));
        }

        // GET: Citas/CitasPorFecha?fecha=2024-01-15 (Para el modal del calendario)
        public async Task<IActionResult> CitasPorFecha(DateTime fecha)
        {
            var citas = await _context.Citas
                .Where(c => c.FechaHora.Date == fecha.Date && !c.Eliminada)
                .OrderBy(c => c.FechaHora)
                .ToListAsync();

            ViewBag.Fecha = fecha;
            return PartialView("_CitasPorFecha", citas);
        }

        private bool CitaExists(int id)
        {
            return _context.Citas.Any(e => e.Id == id);
        }
    }
}