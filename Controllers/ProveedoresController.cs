using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using OpticaClaridad.Models;
using System.Diagnostics;

namespace OpticaClaridad.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProveedoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Proveedores
        public async Task<IActionResult> Index()
        {
            var proveedores = await _context.Proveedores.ToListAsync();
            return View(proveedores);
        }

        // GET: Proveedores/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(m => m.Id == id);

            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // GET: Proveedores/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Proveedores/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    proveedor.FechaRegistro = DateTime.Today;
                    _context.Add(proveedor);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Proveedor creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al crear proveedor: {ex.Message}");
                    ModelState.AddModelError("", "Error al crear el proveedor");
                }
            }
            return View(proveedor);
        }

        // GET: Proveedores/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }
            return View(proveedor);
        }

        // POST: Proveedores/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Proveedor proveedor)
        {
            if (id != proveedor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proveedor);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Proveedor actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProveedorExists(proveedor.Id))
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
                    Debug.WriteLine($"Error al editar proveedor: {ex.Message}");
                    ModelState.AddModelError("", "Error al actualizar el proveedor");
                }
            }
            return View(proveedor);
        }

        // GET: Proveedores/Eliminar/5
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(m => m.Id == id);

            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // POST: Proveedores/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            try
            {
                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor != null)
                {
                    // No eliminar físicamente, solo desactivar
                    proveedor.Activo = false;
                    _context.Update(proveedor);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Proveedor desactivado exitosamente";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al desactivar proveedor: {ex.Message}");
                TempData["ErrorMessage"] = "Error al desactivar el proveedor";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Proveedores/Activar/5
        public async Task<IActionResult> Activar(int id)
        {
            try
            {
                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor != null)
                {
                    proveedor.Activo = true;
                    _context.Update(proveedor);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Proveedor activado exitosamente";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al activar proveedor: {ex.Message}");
                TempData["ErrorMessage"] = "Error al activar el proveedor";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProveedorExists(int id)
        {
            return _context.Proveedores.Any(e => e.Id == id);
        }
    }
}