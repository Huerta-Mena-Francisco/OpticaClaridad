using Microsoft.Extensions.Options;
using System.Drawing; // Agregar este using
using System.Drawing.Imaging;
using System.Drawing.Drawing2D; // Agregar este using

namespace OpticaClaridad.Services
{
    public class ImagenesConfig
    {
        public string RutaBase { get; set; } = @"C:\ImgOptica";
        public string UrlBase { get; set; } = "/imagenes-externas";

        // Tamaños para redimensionamiento
        public Dictionary<string, (int Ancho, int Alto)> Tamanos { get; set; } = new()
        {
            { "original", (0, 0) },           // Sin redimensionar
            { "grande", (800, 800) },         // Para vista detallada
            { "mediana", (400, 400) },        // Para listados
            { "miniatura", (150, 150) }       // Para grids y miniaturas
        };
    }

    public class ImagenesService
    {
        private readonly ImagenesConfig _config;
        private readonly ILogger<ImagenesService> _logger;
        private readonly IWebHostEnvironment _env;

        public ImagenesService(IOptions<ImagenesConfig> config,
                              ILogger<ImagenesService> logger,
                              IWebHostEnvironment env)
        {
            _config = config.Value;
            _logger = logger;
            _env = env;

            // Crear estructura de carpetas al iniciar
            CrearEstructuraCarpetas();
        }

        private void CrearEstructuraCarpetas()
        {
            try
            {
                var rutaBase = _config.RutaBase;

                // Si la ruta es relativa, hacerla absoluta respecto al contenido
                if (!Path.IsPathRooted(rutaBase))
                {
                    rutaBase = Path.Combine(_env.ContentRootPath, rutaBase);
                }

                // Crear carpeta base si no existe
                if (!Directory.Exists(rutaBase))
                {
                    Directory.CreateDirectory(rutaBase);
                    _logger.LogInformation($"Carpeta base creada: {rutaBase}");
                }

                // Carpeta para productos
                var carpetaProductos = Path.Combine(rutaBase, "productos");
                if (!Directory.Exists(carpetaProductos))
                {
                    Directory.CreateDirectory(carpetaProductos);
                }

                // Crear subcarpetas de tamaños para productos
                foreach (var tamano in _config.Tamanos.Keys)
                {
                    var rutaTamano = Path.Combine(carpetaProductos, tamano);
                    if (!Directory.Exists(rutaTamano))
                    {
                        Directory.CreateDirectory(rutaTamano);
                    }
                }

                // Carpeta para clientes (para futuros usos)
                var carpetaClientes = Path.Combine(rutaBase, "clientes");
                if (!Directory.Exists(carpetaClientes))
                {
                    Directory.CreateDirectory(carpetaClientes);
                }

                // Carpeta para perfiles (para futuros usos)
                var carpetaPerfiles = Path.Combine(rutaBase, "perfiles");
                if (!Directory.Exists(carpetaPerfiles))
                {
                    Directory.CreateDirectory(carpetaPerfiles);
                }

                _logger.LogInformation("Estructura de carpetas de imágenes creada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando estructura de carpetas de imágenes");

                // En caso de error, usar carpeta dentro del proyecto
                var rutaFallback = Path.Combine(_env.ContentRootPath, "ImagenesExternas");
                if (!Directory.Exists(rutaFallback))
                {
                    Directory.CreateDirectory(rutaFallback);
                }
                _config.RutaBase = rutaFallback;
            }
        }

        public async Task<(string RutaRelativa, string NombreArchivo)> GuardarImagenProductoAsync(IFormFile imagen, int productoId)
        {
            try
            {
                if (imagen == null || imagen.Length == 0)
                    return (null, null);

                // Validar tipo de archivo
                var extension = Path.GetExtension(imagen.FileName).ToLower();
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (!extensionesPermitidas.Contains(extension))
                    throw new ArgumentException("Formato de imagen no permitido. Use JPG, PNG, GIF o WebP");

                // Validar tamaño (máximo 5MB)
                if (imagen.Length > 5 * 1024 * 1024)
                    throw new ArgumentException("La imagen es demasiado grande. Máximo 5MB");

                // Generar nombre único
                var nombreArchivo = $"{productoId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
                var carpetaProductos = Path.Combine(_config.RutaBase, "productos");

                // Guardar imagen original
                var rutaOriginal = Path.Combine(carpetaProductos, "original", nombreArchivo);
                using (var stream = new FileStream(rutaOriginal, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                // Crear versiones redimensionadas
                CrearVersionesRedimensionadas(rutaOriginal, nombreArchivo, carpetaProductos);

                // Ruta relativa para la base de datos
                var rutaRelativa = $"{_config.UrlBase}/productos/original/{nombreArchivo}";

                _logger.LogInformation($"Imagen guardada: {rutaRelativa}");
                return (rutaRelativa, nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error guardando imagen para producto {productoId}");
                return (null, null);
            }
        }

        private void CrearVersionesRedimensionadas(string rutaOriginal, string nombreArchivo, string carpetaProductos)
        {
            try
            {
                using (var imagenOriginal = System.Drawing.Image.FromFile(rutaOriginal))
                {
                    foreach (var tamano in _config.Tamanos)
                    {
                        if (tamano.Key == "original") continue; // Saltar original

                        var rutaDestino = Path.Combine(carpetaProductos, tamano.Key, nombreArchivo);

                        if (tamano.Value.Ancho > 0 && tamano.Value.Alto > 0)
                        {
                            RedimensionarImagen(imagenOriginal, rutaDestino, tamano.Value.Ancho, tamano.Value.Alto);
                        }
                        else
                        {
                            // Copiar original si no hay dimensiones especificadas
                            File.Copy(rutaOriginal, rutaDestino, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando versiones redimensionadas");
            }
        }

        private void RedimensionarImagen(System.Drawing.Image imagenOriginal, string rutaDestino, int anchoDestino, int altoDestino)
        {
            try
            {
                // Calcular nuevas dimensiones manteniendo aspecto
                var relacionOriginal = (double)imagenOriginal.Width / imagenOriginal.Height;
                var relacionDestino = (double)anchoDestino / altoDestino;

                int nuevoAncho, nuevoAlto;

                if (relacionOriginal > relacionDestino)
                {
                    // Imagen más ancha que el destino
                    nuevoAncho = anchoDestino;
                    nuevoAlto = (int)(anchoDestino / relacionOriginal);
                }
                else
                {
                    // Imagen más alta que el destino
                    nuevoAlto = altoDestino;
                    nuevoAncho = (int)(altoDestino * relacionOriginal);
                }

                using (var imagenRedimensionada = new Bitmap(nuevoAncho, nuevoAlto))
                using (var graphics = Graphics.FromImage(imagenRedimensionada))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;

                    graphics.DrawImage(imagenOriginal, 0, 0, nuevoAncho, nuevoAlto);

                    // Determinar formato basado en extensión
                    var extension = Path.GetExtension(rutaDestino).ToLower();
                    var formato = extension switch
                    {
                        ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                        ".png" => ImageFormat.Png,
                        ".gif" => ImageFormat.Gif,
                        _ => ImageFormat.Jpeg
                    };

                    // Configurar calidad para JPEG
                    if (formato == ImageFormat.Jpeg)
                    {
                        var encoder = ImageCodecInfo.GetImageEncoders()
                            .FirstOrDefault(e => e.FormatID == ImageFormat.Jpeg.Guid);

                        if (encoder != null)
                        {
                            var encoderParams = new EncoderParameters(1);
                            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L); // 85% calidad

                            imagenRedimensionada.Save(rutaDestino, encoder, encoderParams);
                            return;
                        }
                    }

                    imagenRedimensionada.Save(rutaDestino, formato);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error redimensionando imagen: {rutaDestino}");
            }
        }

        public void EliminarImagenProducto(string rutaRelativa)
        {
            try
            {
                if (string.IsNullOrEmpty(rutaRelativa))
                    return;

                // Convertir ruta relativa a física
                var rutaRelativaLimpia = rutaRelativa.Replace(_config.UrlBase, "").TrimStart('/');
                var rutaFisica = Path.Combine(_config.RutaBase, rutaRelativaLimpia);

                if (File.Exists(rutaFisica))
                {
                    // Obtener nombre del archivo
                    var nombreArchivo = Path.GetFileName(rutaFisica);
                    var carpeta = Path.GetDirectoryName(rutaFisica);

                    // Eliminar todas las versiones
                    foreach (var tamano in _config.Tamanos.Keys)
                    {
                        var rutaVersion = Path.Combine(_config.RutaBase, "productos", tamano, nombreArchivo);
                        if (File.Exists(rutaVersion))
                        {
                            File.Delete(rutaVersion);
                        }
                    }

                    _logger.LogInformation($"Imagen eliminada: {rutaRelativa}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error eliminando imagen: {rutaRelativa}");
            }
        }

        public string ObtenerUrlMiniatura(string rutaOriginal)
        {
            if (string.IsNullOrEmpty(rutaOriginal))
                return "/images/productos/default.png";

            return rutaOriginal.Replace("/original/", "/miniatura/");
        }

        public string ObtenerUrlMediana(string rutaOriginal)
        {
            if (string.IsNullOrEmpty(rutaOriginal))
                return "/images/productos/default.png";

            return rutaOriginal.Replace("/original/", "/mediana/");
        }

        public bool ValidarImagen(IFormFile imagen, out string mensajeError)
        {
            mensajeError = null;

            if (imagen == null || imagen.Length == 0)
            {
                mensajeError = "No se ha seleccionado ninguna imagen";
                return false;
            }

            // Validar extensión
            var extension = Path.GetExtension(imagen.FileName).ToLower();
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            if (!extensionesPermitidas.Contains(extension))
            {
                mensajeError = "Formato no permitido. Use JPG, PNG, GIF o WebP";
                return false;
            }

            // Validar tamaño (5MB máximo)
            if (imagen.Length > 5 * 1024 * 1024)
            {
                mensajeError = "La imagen es demasiado grande. Máximo 5MB";
                return false;
            }

            return true;
        }
    }
}