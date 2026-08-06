using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using OpticaClaridad.Models;

namespace OpticaClaridad.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Catalogo
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener TODOS los productos activos
                var productos = await _context.Productos
                    .Include(p => p.Categoria)
                    .Include(p => p.Imagenes)
                    .Where(p => p.Activo == true)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                ViewData["Title"] = "Catálogo de Productos";
                return View(productos);
            }
            catch (Exception)
            {
                // Si hay error, retornar lista vacía
                return View(new List<Producto>());
            }
        }

        // GET: /Catalogo/Detalles/5
        public async Task<IActionResult> Detalles(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Imagenes.Where(i => i.Activa))
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo == true);

            if (producto == null)
            {
                return NotFound();
            }

            ViewData["Title"] = producto.Nombre;
            return View(producto);
        }
    }
}