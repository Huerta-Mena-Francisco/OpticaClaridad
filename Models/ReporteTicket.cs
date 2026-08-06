// Models/ReporteTicket.cs
namespace OpticaClaridad.Models
{
    public class ReporteTicket
    {
        public Credito Credito { get; set; }
        public List<AbonoCredito> Abonos { get; set; }
        public decimal TotalAbonado { get; set; }
        public DateTime FechaReporte { get; set; } = DateTime.Now;
    }
}