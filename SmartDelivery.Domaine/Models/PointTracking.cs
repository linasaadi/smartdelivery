namespace SmartDelivery.Domaine.Models
{
    public class PointTracking
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double VitesseKmh { get; set; }
        public DateTime Horodatage { get; set; } = DateTime.UtcNow;
        public string? Adresse { get; set; }

        public int LivraisonId { get; set; }
        public Livraison Livraison { get; set; } = null!;
    }
}
