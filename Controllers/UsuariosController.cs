using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using OpticaClaridad.Models;
using System.Diagnostics;

namespace OpticaClaridad.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            // Verificar que sea administrador
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            var usuarios = await _context.Usuarios.ToListAsync();
            return View(usuarios);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioSimple usuario)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el nombre de usuario ya existe
                    var existe = await _context.Usuarios
                        .AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario);

                    if (existe)
                    {
                        ModelState.AddModelError("NombreUsuario", "Este nombre de usuario ya está registrado");
                        return View(usuario);
                    }

                    usuario.FechaRegistro = DateTime.Today;
                    usuario.Activo = true;

                    _context.Usuarios.Add(usuario);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Usuario creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al crear usuario: {ex.Message}");
                    ModelState.AddModelError("", "Error al crear el usuario. Intente nuevamente.");
                }
            }

            return View(usuario);
        }

        // GET: Usuarios/Edit/5
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

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioSimple usuario)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != usuario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el nombre de usuario ya existe (excluyendo el actual)
                    var existe = await _context.Usuarios
                        .AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario && u.Id != id);

                    if (existe)
                    {
                        ModelState.AddModelError("NombreUsuario", "Este nombre de usuario ya está registrado");
                        return View(usuario);
                    }

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Usuario actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
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
                    Debug.WriteLine($"Error al editar usuario: {ex.Message}");
                    ModelState.AddModelError("", "Error al actualizar el usuario.");
                }
            }

            return View(usuario);
        }

        // GET: Usuarios/Delete/5
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

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
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
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario != null)
                {
                    // No eliminar físicamente, solo desactivar
                    usuario.Activo = false;
                    _context.Usuarios.Update(usuario);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Usuario desactivado exitosamente";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al desactivar usuario: {ex.Message}");
                TempData["ErrorMessage"] = "Error al desactivar el usuario";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/Activar/5
        public async Task<IActionResult> Activar(int id)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario != null)
                {
                    usuario.Activo = true;
                    _context.Usuarios.Update(usuario);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Usuario activado exitosamente";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al activar usuario: {ex.Message}");
                TempData["ErrorMessage"] = "Error al activar el usuario";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}