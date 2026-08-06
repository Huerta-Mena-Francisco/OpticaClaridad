using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Data;
using OpticaClaridad.Models;

namespace OpticaClaridad.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Login/Index
        public IActionResult Index()
        {
            if (!string.IsNullOrEmpty(_httpContextAccessor.HttpContext?.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // GET: Login/GetUsuarios
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Where(u => u.Activo)
                    .Select(u => new { u.Id, u.NombreCompleto, u.NombreUsuario, u.EsAdmin })
                    .ToListAsync();
                return Json(usuarios);
            }
            catch (Exception ex)
            {
                // Registra el error o lo lanza para ver el fallo
                throw;
            }
        }

        // POST: Login/Index
        [HttpPost]
        // ⚠️ Temporalmente comentado por error 400 en HTTP
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string nombreUsuario, string clave)
        {
            if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(clave))
            {
                ModelState.AddModelError("", "Usuario y contraseña son requeridos");
                return View();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Activo);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Usuario no encontrado");
                return View();
            }

            if (usuario.Clave != clave)
            {
                ModelState.AddModelError("", "Contraseña incorrecta");
                return View();
            }

            var session = _httpContextAccessor.HttpContext!.Session;
            session.SetString("UsuarioId", usuario.Id.ToString());
            session.SetString("NombreUsuario", usuario.NombreUsuario);
            session.SetString("NombreCompleto", usuario.NombreCompleto);
            session.SetString("EsAdmin", usuario.EsAdmin.ToString());

            return RedirectToAction("Index", "Home");
        }

        // GET: Login/Logout
        public IActionResult Logout()
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
            return RedirectToAction("Index");
        }

        // GET: Login/CrearUsuarioInicial
        public IActionResult CrearUsuarioInicial()
        {
            return View();
        }

        // POST: Login/CrearUsuarioInicial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuarioInicial(UsuarioSimple usuario)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.Usuarios
                    .AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario);

                if (!existe)
                {
                    usuario.FechaRegistro = DateTime.Today;
                    usuario.Activo = true;
                    _context.Usuarios.Add(usuario);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Usuario creado exitosamente";
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "El usuario ya existe");
            }
            return View(usuario);
        }

        // Helpers para chequear sesión
        public static bool EstaAutenticado(ISession session) =>
            !string.IsNullOrEmpty(session.GetString("UsuarioId"));

        public static bool EsAdministrador(ISession session) =>
            session.GetString("EsAdmin") == "True";
    }
}
