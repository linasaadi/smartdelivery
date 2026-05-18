using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Domaine.Models
{
    public class Livraison
    {
        public int Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public StatutLivraison Statut { get; set; } = StatutLivraison.EnAttente;
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public DateTime? DateLivraisonPrevue { get; set; }
        public DateTime? DateLivraisonReelle { get; set; }
        public DateTime? ETA { get; set; }
        public decimal Cout { get; set; }
        public string? Notes { get; set; }

        public int CamionId { get; set; }
        public Camion? Camion { get; set; }

        public int DestinationId { get; set; }
        public Destination? Destination { get; set; }

        public ICollection<LivraisonProduit> LivraisonProduits { get; set; } = new List<LivraisonProduit>();
        public ICollection<PointTracking> PointsTracking { get; set; } = new List<PointTracking>();
        public ICollection<Anomalie> Anomalies { get; set; } = new List<Anomalie>();
    }
}
