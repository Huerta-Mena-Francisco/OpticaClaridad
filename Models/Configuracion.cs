using System.ComponentModel.DataAnnotations;

namespace OpticaClaridad.Models
{
    public class Configuracion
    {
        [Key]
        [StringLength(50)]
        public string Clave { get; set; }

        [StringLength(500)]
        public string Valor { get; set; }

        [StringLength(200)]
        public string Descripcion { get; set; }
    }
}