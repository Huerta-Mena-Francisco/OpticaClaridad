using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using OpticaClaridad.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpticaClaridad.Controllers
{
    public class CreditosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CreditosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Creditos
        public async Task<IActionResult> Index(int? pacienteId, string estado, string searchString)
        {
            var creditos = _context.Creditos
                .Include(c => c.Paciente)  // 🔴 CAMBIO: Paciente en lugar de Cliente
                .Include(c => c.Abonos)
                .AsQueryable();

            // Filtrar por paciente si se especifica
            if (pacienteId.HasValue)
            {
                creditos = creditos.Where(c => c.PacienteId == pacienteId.Value);
                ViewBag.PacienteId = pacienteId.Value;
                var paciente = await _context.Pacientes.FindAsync(pacienteId.Value);
                ViewBag.PacienteNombre = paciente?.Nombre;
            }

            // Filtrar por estado
            if (!string.IsNullOrEmpty(estado) && estado != "TODOS")
            {
                creditos = creditos.Where(c => c.Estado == estado);
            }

            // Filtrar por búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                creditos = creditos.Where(c =>
                    c.Paciente.Nombre.Contains(searchString) ||
                    c.Observaciones.Contains(searchString) ||
                    c.Id.ToString() == searchString);
            }

            // Ordenar por fecha de vencimiento (los más próximos primero)
            creditos = creditos.OrderBy(c => c.FechaVencimiento ?? DateTime.MaxValue);

            ViewBag.Estados = new[] { "PENDIENTE", "PAGADO", "VENCIDO", "MOROSO" };
            ViewBag.EstadoSeleccionado = estado ?? "TODOS";

            return View(await creditos.ToListAsync());
        }

        // GET: Creditos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credito = await _context.Creditos
                .Include(c => c.Paciente)  // 🔴 CAMBIO: Paciente en lugar de Cliente
                .Include(c => c.Abonos)
                .Include(c => c.Venta)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (credito == null)
            {
                return NotFound();
            }

            return View(credito);
        }

        // GET: Creditos/Create
        public async Task<IActionResult> Create(int? pacienteId)
        {
            var pacientes = await _context.Pacientes  // 🔴 CAMBIO: Pacientes en lugar de Clientes
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Pacientes = pacientes;

            var credito = new Credito
            {
                PacienteId = pacienteId ?? 0,  // 🔴 CAMBIO: PacienteId
                FechaCredito = DateTime.Today,
                Estado = "PENDIENTE"
            };

            if (pacienteId.HasValue)
            {
                credito.PacienteId = pacienteId.Value;
            }

            return View(credito);
        }

        // POST: Creditos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Credito credito)
        {
            Console.WriteLine("=== INICIANDO CREACIÓN DE CRÉDITO ===");
            Console.WriteLine($"PacienteId recibido: {credito.PacienteId}");  // 🔴 CAMBIO
            Console.WriteLine($"MontoTotal recibido: {credito.MontoTotal}");

            // IMPORTANTE: Remover campos de navegación del ModelState
            ModelState.Remove("Venta");
            ModelState.Remove("Paciente");  // 🔴 CAMBIO
            ModelState.Remove("VentaId");
            ModelState.Remove("Abonos");

            // 1. Validaciones manuales
            bool tieneErrores = false;

            if (credito.PacienteId <= 0)  // 🔴 CAMBIO
            {
                Console.WriteLine("❌ ERROR: PacienteId <= 0");
                TempData["ErrorMessage"] = "Debe seleccionar un paciente válido.";
                ModelState.AddModelError("PacienteId", "Debe seleccionar un paciente válido.");  // 🔴 CAMBIO
                tieneErrores = true;
            }

            if (credito.MontoTotal <= 0)
            {
                Console.WriteLine("❌ ERROR: MontoTotal <= 0");
                TempData["ErrorMessage"] = "El monto debe ser mayor a $0.00.";
                ModelState.AddModelError("MontoTotal", "El monto debe ser mayor a $0.00.");
                tieneErrores = true;
            }

            // 2. Verificar si el paciente existe y está activo
            var pacienteExiste = await _context.Pacientes  // 🔴 CAMBIO
                .AnyAsync(p => p.Id == credito.PacienteId && p.Activo);

            if (!pacienteExiste)
            {
                Console.WriteLine("❌ ERROR: Paciente no existe o no está activo");
                TempData["ErrorMessage"] = "El paciente seleccionado no existe o no está activo.";
                ModelState.AddModelError("PacienteId", "Paciente no válido.");  // 🔴 CAMBIO
                tieneErrores = true;
            }

            // 3. Si hay errores, volver a la vista
            if (tieneErrores)
            {
                Console.WriteLine("❌ MODELO INVALIDO - Mostrando errores:");

                ViewBag.Pacientes = await _context.Pacientes  // 🔴 CAMBIO
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return View(credito);
            }

            // 4. CREAR EL CRÉDITO
            try
            {
                // Crear objeto nuevo para evitar problemas de validación
                var creditoNuevo = new Credito
                {
                    PacienteId = credito.PacienteId,  // 🔴 CAMBIO
                    MontoTotal = credito.MontoTotal,
                    Saldo = credito.MontoTotal,
                    FechaCredito = credito.FechaCredito,
                    Observaciones = credito.Observaciones,
                    DiasCredito = credito.DiasCredito,
                    TipoPeriodo = credito.TipoPeriodo,
                    CantidadPeriodo = credito.CantidadPeriodo,
                    MontoPeriodo = credito.MontoPeriodo,
                    Estado = "PENDIENTE",
                    FechaCreacion = DateTime.Now,
                    VentaId = null
                };

                // Calcular fecha de vencimiento
                creditoNuevo.FechaVencimiento = creditoNuevo.CalcularFechaVencimiento();

                // Si no se calculó, usar valor por defecto
                if (!creditoNuevo.FechaVencimiento.HasValue)
                {
                    creditoNuevo.FechaVencimiento = creditoNuevo.FechaCredito.AddDays(30);
                }

                // Guardar en la base de datos
                _context.Creditos.Add(creditoNuevo);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ CRÉDITO CREADO EXITOSAMENTE: ID={creditoNuevo.Id}");

                TempData["SuccessMessage"] = $"Crédito #{creditoNuevo.Id} creado exitosamente.";
                return RedirectToAction(nameof(Details), new { id = creditoNuevo.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EXCEPCIÓN: {ex.Message}");

                TempData["ErrorMessage"] = $"Error al guardar el crédito: {ex.Message}";

                ViewBag.Pacientes = await _context.Pacientes  // 🔴 CAMBIO
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return View(credito);
            }
        }

        // GET: Creditos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credito = await _context.Creditos.FindAsync(id);
            if (credito == null)
            {
                return NotFound();
            }

            ViewBag.Pacientes = await _context.Pacientes  // 🔴 CAMBIO
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(credito);
        }

        // POST: Creditos/Edit/5
        // POST: Creditos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,PacienteId,MontoTotal,Saldo,FechaCredito,FechaVencimiento,Estado,Observaciones,DiasCredito,TipoPeriodo,CantidadPeriodo,MontoPeriodo,VentaId")]
    Credito credito)
        {
            if (id != credito.Id)
            {
                return NotFound();
            }

            // 🔴 1. Cargar el crédito original con todas sus relaciones
            var creditoOriginal = await _context.Creditos
                .Include(c => c.Paciente)
                .Include(c => c.Abonos)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (creditoOriginal == null)
            {
                return NotFound();
            }

            // 🔴 2. Verificar si está pagado
            if (creditoOriginal.Estado == "PAGADO")
            {
                TempData["ErrorMessage"] = "No se puede editar un crédito que ya ha sido pagado completamente.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // 🔴 3. Si tiene venta asociada, mantener el PacienteId original
            if (creditoOriginal.VentaId.HasValue)
            {
                // Forzar el PacienteId original (no permitir cambios)
                credito.PacienteId = creditoOriginal.PacienteId;
                ModelState.Remove("PacienteId"); // Remover del ModelState para evitar validación
            }

            // 🔴 4. Remover validaciones innecesarias
            ModelState.Remove("Venta");
            ModelState.Remove("Paciente");
            ModelState.Remove("Abonos");

            // 🔴 5. Validar el monto si cambió
            if (credito.MontoTotal != creditoOriginal.MontoTotal)
            {
                // Si hay abonos, no permitir cambiar el monto total
                if (creditoOriginal.Abonos != null && creditoOriginal.Abonos.Any())
                {
                    ModelState.AddModelError("MontoTotal", "No se puede cambiar el monto total porque ya tiene abonos registrados.");
                }
                // Si el nuevo monto es menor al saldo actual
                else if (credito.MontoTotal < creditoOriginal.Saldo)
                {
                    ModelState.AddModelError("MontoTotal",
                        $"El nuevo monto total no puede ser menor al saldo actual (${creditoOriginal.Saldo:N2})");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mantener la fecha de creación original
                    credito.FechaCreacion = creditoOriginal.FechaCreacion;

                    // Si no hay abonos, recalcular saldo si cambió el monto total
                    if ((creditoOriginal.Abonos == null || !creditoOriginal.Abonos.Any()) &&
                        credito.MontoTotal != creditoOriginal.MontoTotal)
                    {
                        credito.Saldo = credito.MontoTotal;
                    }
                    else
                    {
                        // Mantener el saldo actual
                        credito.Saldo = creditoOriginal.Saldo;
                    }

                    // Recalcular fecha de vencimiento si cambió
                    if (!credito.FechaVencimiento.HasValue ||
                        credito.TipoPeriodo != creditoOriginal.TipoPeriodo ||
                        credito.CantidadPeriodo != creditoOriginal.CantidadPeriodo ||
                        credito.DiasCredito != creditoOriginal.DiasCredito)
                    {
                        credito.FechaVencimiento = credito.CalcularFechaVencimiento();
                    }

                    _context.Update(credito);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Crédito actualizado exitosamente.";
                    return RedirectToAction(nameof(Details), new { id = credito.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CreditoExists(credito.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // 🔴 6. Si hay errores, recargar los datos necesarios para la vista
            ViewBag.Pacientes = await _context.Pacientes
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            // 🔴 7. CRÍTICO: Volver a cargar el paciente para mostrarlo en la vista
            // Esto asegura que aunque haya errores, el nombre del paciente se muestre
            if (credito.PacienteId > 0)
            {
                credito.Paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.Id == credito.PacienteId);
            }

            return View(credito);
        }

        // GET: Creditos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credito = await _context.Creditos
                .Include(c => c.Paciente)  // 🔴 CAMBIO
                .Include(c => c.Abonos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (credito == null)
            {
                return NotFound();
            }

            return View(credito);
        }

        // POST: Creditos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var credito = await _context.Creditos
                .Include(c => c.Abonos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (credito == null)
            {
                return NotFound();
            }

            // Verificar si tiene abonos registrados
            if (credito.Abonos.Any())
            {
                TempData["ErrorMessage"] = "No se puede eliminar el crédito porque tiene abonos registrados.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            // Verificar si está pagado
            if (credito.Saldo <= 0 || credito.Estado == "PAGADO")
            {
                TempData["ErrorMessage"] = "No se puede eliminar un crédito que ya ha sido pagado.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            try
            {
                _context.Creditos.Remove(credito);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Crédito eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar el crédito: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Creditos/RegistrarAbono/5
        public async Task<IActionResult> RegistrarAbono(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credito = await _context.Creditos
                .Include(c => c.Paciente)  // 🔴 CAMBIO
                .FirstOrDefaultAsync(c => c.Id == id);

            if (credito == null)
            {
                return NotFound();
            }

            if (credito.Estado == "PAGADO")
            {
                TempData["ErrorMessage"] = "No se pueden registrar abonos a un crédito ya pagado.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.Credito = credito;
            ViewBag.SaldoPendiente = credito.Saldo;
            ViewBag.MaxAbono = credito.Saldo;

            var abono = new AbonoCredito
            {
                CreditoId = credito.Id,
                FechaAbono = DateTime.Today
            };

            return View(abono);
        }

        // POST: Creditos/RegistrarAbono/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarAbono(int id, [Bind("CreditoId,Monto,Observaciones,FechaAbono")] AbonoCredito abono)
        {
            var credito = await _context.Creditos.FindAsync(id);
            if (credito == null)
            {
                return NotFound();
            }

            if (credito.Estado == "PAGADO")
            {
                TempData["ErrorMessage"] = "No se pueden registrar abonos a un crédito ya pagado.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (abono.Monto > credito.Saldo)
            {
                ModelState.AddModelError("Monto",
                    $"El monto no puede exceder el saldo pendiente (${credito.Saldo:N2})");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Usar el método del modelo para registrar el abono
                    credito.RegistrarAbono(abono.Monto, abono.Observaciones);

                    // Asignar usuario actual
                    abono.UsuarioId = User.Identity?.Name ?? "SISTEMA";

                    _context.AbonosCredito.Add(abono);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Abono de ${abono.Monto:N2} registrado exitosamente.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al registrar el abono: {ex.Message}");
                }
            }

            ViewBag.Credito = credito;
            ViewBag.SaldoPendiente = credito.Saldo;
            ViewBag.MaxAbono = credito.Saldo;
            return View(abono);
        }

        // GET: Creditos/HistorialAbonos/5
        public async Task<IActionResult> HistorialAbonos(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credito = await _context.Creditos
                .Include(c => c.Paciente)  // 🔴 CAMBIO
                .Include(c => c.Abonos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (credito == null)
            {
                return NotFound();
            }

            return View(credito);
        }

        // POST: Creditos/CambiarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var credito = await _context.Creditos.FindAsync(id);
            if (credito == null)
            {
                return NotFound();
            }

            var estadosValidos = new[] { "PENDIENTE", "PAGADO", "VENCIDO", "MOROSO" };
            if (!estadosValidos.Contains(nuevoEstado))
            {
                TempData["ErrorMessage"] = "Estado no válido.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Validaciones específicas por estado
            if (nuevoEstado == "PAGADO" && credito.Saldo > 0)
            {
                TempData["ErrorMessage"] = "No se puede marcar como PAGADO un crédito con saldo pendiente.";
                return RedirectToAction(nameof(Details), new { id });
            }

            credito.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Estado cambiado a {nuevoEstado} exitosamente.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private bool CreditoExists(int id)
        {
            return _context.Creditos.Any(e => e.Id == id);
        }
    }
}