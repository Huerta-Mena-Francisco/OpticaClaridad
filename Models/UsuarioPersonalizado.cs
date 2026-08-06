using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpticaClaridad.Models
{
    public class UsuarioPersonalizado : IdentityUser
    {
        // IdentityUser ya tiene: 
        // - UserName (lo usaremos como NombreUsuario)
        // - Email
        // - PhoneNumber
        // - etc.

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        [Display(Name = "Nombre Completo")]
        [PersonalData]
        public string NombreCompleto { get; set; }

        [Display(Name = "Es Administrador")]
        public bool EsAdmin { get; set; } = false;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Today;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Constructor para inicializar UserName
        public UsuarioPersonalizado()
        {
            FechaRegistro = DateTime.Today;
            Activo = true;
        }

        // Método para asignar automáticamente el UserName
        public void AsignarNombreUsuario(string nombreUsuario)
        {
            UserName = nombreUsuario;
            NormalizedUserName = nombreUsuario.ToUpper();
        }
    }
}