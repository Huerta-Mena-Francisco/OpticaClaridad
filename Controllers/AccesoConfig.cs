// Models/AccesoConfig.cs
namespace OpticaClaridad.Models
{
    public class AccesoConfig
    {
        public string? IpServidor { get; set; }  // Nullable
        public int Puerto { get; set; } = 5000;
        public string NombreAplicacion { get; set; } = "Óptica Claridad";
    }
}