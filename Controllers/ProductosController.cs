using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpticaClaridad.Data;
using OpticaClaridad.Models;


namespace OpticaClaridad.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductosController> _logger;
        private readonly string _rootPath;
        private readonly string _imagenesPath;
        private const string CARPETA_IMAGENES = "Imagenes/Productos";

        public ProductosController(
            ApplicationDbContext context,
            ILogger<ProductosController> logger,
            IOptions<FileStorageSettings> fileStorageSettings)
        {
            _context = context;
            _logger = logger;
            _rootPath = fileStorageSettings.Value.RootPath;
            _imagenesPath = Path.Combine(_rootPath, CARPETA_IMAGENES);

            // Crear el directorio si no existe
            if (!Directory.Exists(_imagenesPath))
            {
                Directory.CreateDirectory(_imagenesPath);
                _logger.LogInformation("Directorio de imágenes creado: {Path}", _imagenesPath);
            }
        }

        // GET: Productos
        public async Task<IActionResult> Index(string busqueda = "",
                                               int? categoriaId = null,
                                               string estadoStock = "Todos",
                                               int page = 1,
                                               int pageSize = 10)
        {
            // Solo admin puede gestionar productos
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Gestión de Productos";
            ViewData["Subtitle"] = "Listado de productos en inventario";

            IQueryable<Producto> query = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Imagenes.Where(i => i.Activa))
                .Where(p => p.Activo);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    p.Codigo.Contains(busqueda) ||
                    p.Descripcion.Contains(busqueda) ||
                    p.Marca.Contains(busqueda));
            }

            if (categoriaId.HasValue && categoriaId > 0)
            {
                query = query.Where(p => p.CategoriaId == categoriaId);
            }

            // Filtrar por estado de stock
            if (!string.IsNullOrEmpty(estadoStock) && estadoStock != "Todos")
            {
                switch (estadoStock)
                {
                    case "Agotado":
                        query = query.Where(p => p.Stock == 0);
                        break;
                    case "Bajo":
                        query = query.Where(p => p.Stock <= p.StockMinimo && p.Stock > 0);
                        break;
                    case "Normal":
                        query = query.Where(p => p.Stock > p.StockMinimo);
                        break;
                }
            }

            // Paginación
            var totalProductos = await query.CountAsync();
            var productos = await query
                .OrderBy(p => p.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pasar datos a la vista
            ViewBag.Categorias = await _context.CategoriasProducto
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                })
                .ToListAsync();

            ViewBag.BusquedaActual = busqueda;
            ViewBag.CategoriaIdActual = categoriaId;
            ViewBag.EstadoStockActual = estadoStock;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalProductos = totalProductos;
            ViewBag.TotalPages = (int)Math.Ceiling(totalProductos / (double)pageSize);

            return View(productos);
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Imagenes.Where(i => i.Activa))
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

            if (producto == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Detalles: {producto.Nombre}";
            return View(producto);
        }

        // GET: Productos/Create
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Nuevo Producto";
            ViewData["Subtitle"] = "Agregar nuevo producto al inventario";

            await CargarDatosVista();
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto,
                                               List<IFormFile> imagenesFiles,
                                               int imagenPrincipalIndex = 0)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            _logger.LogInformation("=== INICIANDO CREACIÓN DE PRODUCTO ===");

            // Validaciones manuales
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(producto.Nombre))
                errores.Add("El nombre es obligatorio");

            if (string.IsNullOrWhiteSpace(producto.Codigo))
                errores.Add("El código es obligatorio");

            // Validar que el código no exista
            if (!string.IsNullOrWhiteSpace(producto.Codigo))
            {
                var codigoExistente = await _context.Productos
                    .AnyAsync(p => p.Codigo == producto.Codigo && p.Activo);

                if (codigoExistente)
                    errores.Add("El código ya está registrado");
            }

            if (errores.Any())
            {
                foreach (var error in errores)
                {
                    ModelState.AddModelError("", error);
                }

                await CargarDatosVista();
                return View(producto);
            }

            try
            {
                // Establecer valores por defecto
                producto.FechaCreacion = DateTime.Today;
                producto.FechaActualizacion = DateTime.Now;
                producto.Activo = true;
                producto.ImagenPrincipalUrl = string.Empty;

                // Asegurar valores mínimos
                if (producto.StockMinimo < 0) producto.StockMinimo = 0;
                if (producto.PrecioCompra < 0) producto.PrecioCompra = 0;

                // Inicializar la colección de imágenes
                producto.Imagenes = new List<ProductoImagen>();

                // GUARDAR PRODUCTO PRIMERO para obtener ID
                _context.Add(producto);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Producto guardado con ID: {Id}", producto.Id);

                // MANEJAR IMÁGENES (OPCIONAL)
                if (imagenesFiles != null && imagenesFiles.Any(f => f != null && f.Length > 0))
                {
                    _logger.LogInformation("Procesando imágenes...");

                    var imagenesValidas = imagenesFiles
                        .Where(f => f != null && f.Length > 0 && f.Length <= 10 * 1024 * 1024)
                        .ToList();

                    _logger.LogInformation("{Count} imágenes válidas encontradas", imagenesValidas.Count);

                    if (imagenesValidas.Any())
                    {
                        var primeraImagenUrl = "";
                        var imagenPrincipalUrl = "";

                        for (int i = 0; i < imagenesValidas.Count; i++)
                        {
                            var imagenFile = imagenesValidas[i];

                            // Guardar imagen física y obtener ruta relativa
                            var rutaRelativa = await GuardarImagen(imagenFile);
                            if (string.IsNullOrEmpty(rutaRelativa))
                                continue;

                            // Guardar primera imagen
                            if (i == 0)
                            {
                                primeraImagenUrl = rutaRelativa;
                            }

                            // Verificar si esta es la imagen principal
                            bool esPrincipal = (i == imagenPrincipalIndex);

                            if (esPrincipal)
                            {
                                imagenPrincipalUrl = rutaRelativa;
                            }

                            // Crear registro de imagen
                            var productoImagen = new ProductoImagen
                            {
                                ProductoId = producto.Id,
                                ImagenUrl = rutaRelativa,
                                Descripcion = $"Imagen {i + 1}",
                                Orden = i,
                                EsPrincipal = esPrincipal,
                                Activa = true,
                                FechaCreacion = DateTime.Now
                            };

                            _context.ProductosImagenes.Add(productoImagen);
                            _logger.LogInformation("Imagen {Index} agregada: {Ruta}", i, rutaRelativa);
                        }

                        // Establecer imagen principal
                        if (!string.IsNullOrEmpty(imagenPrincipalUrl))
                        {
                            producto.ImagenPrincipalUrl = imagenPrincipalUrl;
                        }
                        else if (!string.IsNullOrEmpty(primeraImagenUrl))
                        {
                            producto.ImagenPrincipalUrl = primeraImagenUrl;
                        }

                        // GUARDAR CAMBIOS DE IMÁGENES
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Imágenes guardadas en la base de datos");
                    }
                }

                TempData["SuccessMessage"] = $"Producto '{producto.Nombre}' creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                await CargarDatosVista();
                return View(producto);
            }
        }

        // GET: Productos/Edit/5
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

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Imagenes.Where(i => i.Activa))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Editar: {producto.Nombre}";
            ViewData["Subtitle"] = "Modificar información del producto";

            await CargarDatosVista();
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
                                             Producto producto,
                                             List<IFormFile> nuevasImagenesFiles,
                                             string imagenesAEliminar = "")
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != producto.Id)
            {
                return NotFound();
            }

            // Validar que el código no exista en otro producto
            var codigoExistente = await _context.Productos
                .AnyAsync(p => p.Codigo == producto.Codigo && p.Id != id && p.Activo);

            if (codigoExistente)
            {
                ModelState.AddModelError("Codigo", "El código ya está registrado en otro producto");
                await CargarDatosVista();
                return View(producto);
            }

            try
            {
                // Obtener producto actual con sus imágenes
                var productoActual = await _context.Productos
                    .Include(p => p.Imagenes)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (productoActual == null)
                {
                    return NotFound();
                }

                // MANEJAR IMÁGENES A ELIMINAR
                await EliminarImagenesMarcadas(imagenesAEliminar, productoActual);

                // MANEJAR NUEVAS IMÁGENES
                await AgregarNuevasImagenes(nuevasImagenesFiles, productoActual.Id);

                // Actualizar campos del producto
                productoActual.Nombre = producto.Nombre;
                productoActual.Codigo = producto.Codigo;
                productoActual.Descripcion = producto.Descripcion;
                productoActual.CategoriaId = producto.CategoriaId;
                productoActual.PrecioVenta = producto.PrecioVenta;
                productoActual.PrecioCompra = producto.PrecioCompra;
                productoActual.Stock = producto.Stock;
                productoActual.StockMinimo = producto.StockMinimo;
                productoActual.Marca = producto.Marca;
                productoActual.Modelo = producto.Modelo;
                productoActual.Color = producto.Color;
                productoActual.Material = producto.Material;
                productoActual.Genero = producto.Genero;
                productoActual.TipoLente = producto.TipoLente;
                productoActual.TieneUV = producto.TieneUV;
                productoActual.TieneAntireflejante = producto.TieneAntireflejante;
                productoActual.EsFotocromatico = producto.EsFotocromatico;
                productoActual.Activo = producto.Activo;
                productoActual.FechaActualizacion = DateTime.Now;

                // Si no hay imagen principal y hay imágenes, usar la primera activa
                if (string.IsNullOrEmpty(productoActual.ImagenPrincipalUrl) &&
                    productoActual.Imagenes.Any(i => i.Activa))
                {
                    var primeraImagen = productoActual.Imagenes
                        .Where(i => i.Activa)
                        .OrderBy(i => i.Orden)
                        .FirstOrDefault();

                    if (primeraImagen != null)
                    {
                        productoActual.ImagenPrincipalUrl = primeraImagen.ImagenUrl;
                        primeraImagen.EsPrincipal = true;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Producto '{producto.Nombre}' actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(producto.Id))
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
                _logger.LogError(ex, "Error al editar producto");
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");

                await CargarDatosVista();
                return View(producto);
            }
        }

        // GET: Productos/Delete/5
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

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

            if (producto == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Eliminar: {producto.Nombre}";
            return View(producto);
        }

        // POST: Productos/Delete/5
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
                var producto = await _context.Productos
                    .Include(p => p.Imagenes)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (producto != null)
                {
                    // 1. ELIMINAR IMÁGENES FÍSICAS ANTES de marcar como inactivo
                    await EliminarImagenesFisicas(producto.Imagenes);

                    // 2. Marcar producto como inactivo (soft delete)
                    producto.Activo = false;
                    producto.FechaActualizacion = DateTime.Now;

                    // 3. Marcar imágenes como inactivas en BD
                    foreach (var imagen in producto.Imagenes)
                    {
                        imagen.Activa = false;
                    }

                    _context.Productos.Update(producto);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Producto {Id} marcado como inactivo e imágenes eliminadas", id);
                    TempData["SuccessMessage"] = $"Producto '{producto.Nombre}' eliminado exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Producto no encontrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto");
                TempData["ErrorMessage"] = $"Error al eliminar el producto: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Productos/StockBajo
        public async Task<IActionResult> StockBajo()
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Productos con Stock Bajo";
            ViewData["Subtitle"] = "Productos que necesitan reposición";

            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Stock <= p.StockMinimo)
                .OrderBy(p => p.Stock)
                .ToListAsync();

            return View(productos);
        }

        // AJAX: Actualizar stock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarStock([FromBody] ActualizarStockRequest request)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return Json(new { success = false, message = "No autenticado" });
            }

            try
            {
                var producto = await _context.Productos.FindAsync(request.Id);
                if (producto == null)
                    return Json(new { success = false, message = "Producto no encontrado" });

                // Validar que no quede negativo
                if (producto.Stock + request.Cantidad < 0)
                    return Json(new { success = false, message = $"Stock insuficiente. Stock actual: {producto.Stock}" });

                producto.Stock += request.Cantidad;
                producto.FechaActualizacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    nuevoStock = producto.Stock,
                    estadoStock = producto.EstadoStock,
                    mensaje = $"Stock actualizado: {producto.Stock} unidades"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // Clase para recibir los datos
        public class ActualizarStockRequest
        {
            public int Id { get; set; }
            public int Cantidad { get; set; }
        }

        // AJAX: Obtener producto por código
        [HttpGet]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Codigo == codigo && p.Activo && p.Disponible);

            if (producto == null)
                return Json(new { success = false });

            // Convertir ruta relativa a URL pública
            var imagenUrl = string.IsNullOrEmpty(producto.ImagenPrincipalUrl)
                ? "/images/productos/default.png"
                : $"/archivos/{producto.ImagenPrincipalUrl.Replace('\\', '/')}";

            return Json(new
            {
                success = true,
                producto = new
                {
                    id = producto.Id,
                    nombre = producto.Nombre,
                    codigo = producto.Codigo,
                    precioVenta = producto.PrecioVenta,
                    stock = producto.Stock,
                    categoria = producto.Categoria?.Nombre,
                    imagenUrl = imagenUrl
                }
            });
        }

        // GET: Establecer imagen como principal
        public async Task<IActionResult> EstablecerImagenPrincipal(int productoId, int imagenId)
        {
            if (HttpContext.Session.GetString("EsAdmin") != "True")
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var producto = await _context.Productos
                    .Include(p => p.Imagenes)
                    .FirstOrDefaultAsync(p => p.Id == productoId);

                if (producto == null)
                {
                    return NotFound();
                }

                // Buscar la imagen
                var imagen = producto.Imagenes.FirstOrDefault(i => i.Id == imagenId);
                if (imagen == null)
                {
                    TempData["ErrorMessage"] = "Imagen no encontrada";
                    return RedirectToAction("Edit", new { id = productoId });
                }

                // Quitar principal de todas
                foreach (var img in producto.Imagenes)
                {
                    img.EsPrincipal = false;
                }

                // Establecer nueva principal
                imagen.EsPrincipal = true;
                producto.ImagenPrincipalUrl = imagen.ImagenUrl;
                producto.FechaActualizacion = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Imagen principal actualizada";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer imagen principal");
                TempData["ErrorMessage"] = "Error al actualizar la imagen principal";
            }

            return RedirectToAction("Edit", new { id = productoId });
        }

        // ==================== MÉTODOS PRIVADOS ====================

        /// <summary>
        /// Guarda una imagen física en el FileStorage y retorna la ruta relativa
        /// </summary>
        private async Task<string> GuardarImagen(IFormFile imagenFile)
        {
            if (imagenFile == null || imagenFile.Length == 0)
                return string.Empty;

            // Validar extensión
            var extension = Path.GetExtension(imagenFile.FileName).ToLower();
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

            if (!extensionesPermitidas.Contains(extension))
            {
                _logger.LogWarning("Extensión no permitida: {Extension}", extension);
                return string.Empty;
            }

            // Validar tamaño (10MB max)
            if (imagenFile.Length > 10 * 1024 * 1024)
            {
                _logger.LogWarning("Archivo demasiado grande: {Size} bytes", imagenFile.Length);
                return string.Empty;
            }

            // Generar nombre único
            var fileName = $"{Guid.NewGuid():N}{extension}";

            // Ruta relativa para BD (usando / para compatibilidad web)
            var relativePath = $"{CARPETA_IMAGENES.Replace('\\', '/')}/{fileName}";

            // Ruta física completa
            var fullPath = Path.Combine(_rootPath, CARPETA_IMAGENES, fileName);

            // Asegurar que el directorio existe
            var directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Guardar archivo
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await imagenFile.CopyToAsync(stream);
            }

            _logger.LogInformation("Imagen guardada: {RelativePath} -> {FullPath}", relativePath, fullPath);

            return relativePath;
        }

        /// <summary>
        /// Elimina una imagen física del FileStorage usando su ruta relativa
        /// </summary>
        private void EliminarArchivoImagen(string rutaRelativa)
        {
            if (string.IsNullOrEmpty(rutaRelativa))
                return;

            try
            {
                // Convertir ruta relativa a ruta física
                var rutaLimpia = rutaRelativa.Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(_rootPath, rutaLimpia);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation("Archivo eliminado: {FullPath}", fullPath);
                }
                else
                {
                    _logger.LogWarning("Archivo no encontrado para eliminar: {FullPath}", fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar el archivo de imagen: {Ruta}", rutaRelativa);
            }
        }

        private async Task CargarDatosVista()
        {
            ViewBag.Categorias = await _context.CategoriasProducto
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                })
                .ToListAsync();

            // Opciones para dropdowns
            ViewBag.Generos = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Seleccionar..." },
                new SelectListItem { Value = "Hombre", Text = "Hombre" },
                new SelectListItem { Value = "Mujer", Text = "Mujer" },
                new SelectListItem { Value = "Unisex", Text = "Unisex" },
                new SelectListItem { Value = "Niño", Text = "Niño" },
                new SelectListItem { Value = "Niña", Text = "Niña" }
            };

            ViewBag.TiposLente = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Seleccionar..." },
                new SelectListItem { Value = "Graduado", Text = "Graduado" },
                new SelectListItem { Value = "Sol", Text = "Sol" },
                new SelectListItem { Value = "Protección", Text = "Protección" },
                new SelectListItem { Value = "Contacto", Text = "Contacto" },
                new SelectListItem { Value = "Computadora", Text = "Computadora" },
                new SelectListItem { Value = "Deportivo", Text = "Deportivo" }
            };
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }

        private async Task EliminarImagenesMarcadas(string imagenesAEliminar, Producto productoActual)
        {
            if (string.IsNullOrEmpty(imagenesAEliminar))
                return;

            var idsAEliminar = imagenesAEliminar.Split(',')
                .Select(idStr => int.TryParse(idStr, out int imgId) ? imgId : 0)
                .Where(imgId => imgId > 0)
                .ToList();

            if (!idsAEliminar.Any())
                return;

            foreach (var imgId in idsAEliminar)
            {
                var imagen = productoActual.Imagenes.FirstOrDefault(i => i.Id == imgId);
                if (imagen != null)
                {
                    // 1. ELIMINAR ARCHIVO FÍSICO usando la ruta relativa
                    EliminarArchivoImagen(imagen.ImagenUrl);

                    // 2. Eliminar de la base de datos
                    _context.ProductosImagenes.Remove(imagen);

                    _logger.LogInformation("Imagen {Id} eliminada", imgId);
                }
            }

            // Guardar cambios inmediatamente
            await _context.SaveChangesAsync();
        }

        private async Task AgregarNuevasImagenes(List<IFormFile> nuevasImagenesFiles, int productoId)
        {
            if (nuevasImagenesFiles == null || !nuevasImagenesFiles.Any())
                return;

            var ordenBase = await _context.ProductosImagenes
                .Where(pi => pi.ProductoId == productoId)
                .CountAsync();

            for (int i = 0; i < nuevasImagenesFiles.Count; i++)
            {
                var imagenFile = nuevasImagenesFiles[i];
                if (imagenFile == null || imagenFile.Length == 0)
                    continue;

                // Guardar imagen física y obtener ruta relativa
                var rutaRelativa = await GuardarImagen(imagenFile);
                if (string.IsNullOrEmpty(rutaRelativa))
                    continue;

                // Crear registro de imagen
                var productoImagen = new ProductoImagen
                {
                    ProductoId = productoId,
                    ImagenUrl = rutaRelativa,
                    Descripcion = $"Imagen adicional {i + 1}",
                    Orden = ordenBase + i,
                    EsPrincipal = false,
                    Activa = true,
                    FechaCreacion = DateTime.Now
                };

                _context.ProductosImagenes.Add(productoImagen);
                _logger.LogInformation("Nueva imagen agregada: {Ruta}", rutaRelativa);
            }
        }

        private async Task EliminarImagenesFisicas(ICollection<ProductoImagen> imagenes)
        {
            if (imagenes == null || !imagenes.Any())
                return;

            foreach (var imagen in imagenes.Where(i => i.Activa))
            {
                EliminarArchivoImagen(imagen.ImagenUrl);
            }

            _logger.LogInformation("Eliminadas {Count} imágenes físicas", imagenes.Count(i => i.Activa));
        }
    }
}