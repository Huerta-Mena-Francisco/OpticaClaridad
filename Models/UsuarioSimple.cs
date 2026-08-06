using System;
using System.ComponentModel.DataAnnotations;

namespace OpticaClaridad.Models
{
    public class UsuarioSimple
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "La clave es obligatoria")]
        [StringLength(100, ErrorMessage = "La clave no puede exceder 100 caracteres")]
        [Display(Name = "Clave")]
        [DataType(DataType.Password)]
        public string Clave { get; set; }

        [Display(Name = "Es Administrador")]
        public bool EsAdmin { get; set; } = false;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Today;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Métodos de ayuda
        public bool VerificarClave(string claveIngresada)
        {
            // Simple comparación para desarrollo
            // En producción usar BCrypt: return BCrypt.Net.BCrypt.Verify(claveIngresada, Clave);
            return Clave == claveIngresada;
        }

        public void EncriptarClave(string clavePlana)
        {
            // Para desarrollo: guardar en texto plano
            // En producción: Clave = BCrypt.Net.BCrypt.HashPassword(clavePlana);
            Clave = clavePlana;
        }
    }
}