using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using OpticaClaridad.Models;
using System.Diagnostics;

namespace OpticaClaridad.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categorias
        public async Task<IActionResult> Index()
        {
            // Solo admin puede gestionar categorías
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            var categorias = await _context.CategoriasProducto.ToListAsync();
            return View(categorias);
        }

        // GET: Categorias/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaProducto categoria)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si ya existe una categoría con ese nombre
                    var existe = await _context.CategoriasProducto
                        .AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower());

                    if (existe)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe una categoría con este nombre");
                        return View(categoria);
                    }

                    categoria.Activo = true;
                    _context.CategoriasProducto.Add(categoria);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Categoría creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al crear categoría: {ex.Message}");
                    ModelState.AddModelError("", "Error al crear la categoría");
                }
            }

            return View(categoria);
        }

        // GET: Categorias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.CategoriasProducto.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoriaProducto categoria)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != categoria.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si ya existe otra categoría con ese nombre
                    var existe = await _context.CategoriasProducto
                        .AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower() && c.Id != id);

                    if (existe)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe una categoría con este nombre");
                        return View(categoria);
                    }

                    _context.Update(categoria);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Categoría actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.Id))
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
                    Debug.WriteLine($"Error al editar categoría: {ex.Message}");
                    ModelState.AddModelError("", "Error al actualizar la categoría");
                }
            }

            return View(categoria);
        }

        // GET: Categorias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.CategoriasProducto
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            // Verificar si la categoría tiene productos asociados
            var tieneProductos = await _context.Productos
                .AnyAsync(p => p.CategoriaId == id && p.Activo);

            ViewBag.TieneProductos = tieneProductos;
            ViewBag.CantidadProductos = tieneProductos ?
                await _context.Productos.CountAsync(p => p.CategoriaId == id && p.Activo) : 0;

            return View(categoria);
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var categoria = await _context.CategoriasProducto.FindAsync(id);
                if (categoria != null)
                {
                    // Verificar si tiene productos activos
                    var tieneProductos = await _context.Productos
                        .AnyAsync(p => p.CategoriaId == id && p.Activo);

                    if (tieneProductos)
                    {
                        TempData["ErrorMessage"] = "No se puede eliminar la categoría porque tiene productos asociados. Desactive la categoría en su lugar.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Solo desactivar en lugar de eliminar
                    categoria.Activo = false;
                    _context.CategoriasProducto.Update(categoria);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Categoría desactivada exitosamente";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al desactivar categoría: {ex.Message}");
                TempData["ErrorMessage"] = "Error al desactivar la categoría";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Categorias/Activar/5
        public async Task<IActionResult> Activar(int id)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var categoria = await _context.CategoriasProducto.FindAsync(id);
                if (categoria != null)
                {
                    categoria.Activo = true;
                    _context.CategoriasProducto.Update(categoria);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Categoría activada exitosamente";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al activar categoría: {ex.Message}");
                TempData["ErrorMessage"] = "Error al activar la categoría";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int id)
        {
            return _context.CategoriasProducto.Any(e => e.Id == id);
        }
    }
}