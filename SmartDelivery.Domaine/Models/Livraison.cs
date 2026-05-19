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
        public string? RaisonRefus { get; set; }

        // ── Produit (inline — pas de table séparée) ──────────────────────────
        public string NomProduit { get; set; } = string.Empty;
        public string? DescriptionProduit { get; set; }
        public decimal PoidsKg { get; set; }
        public decimal VolumeM3 { get; set; }
        public int Quantite { get; set; } = 1;
        public decimal PrixUnitaire { get; set; }

        // ── Relations ────────────────────────────────────────────────────────
        public int CamionId { get; set; }
        public Camion? Camion { get; set; }

        public int DestinationId { get; set; }
        public Destination? Destination { get; set; }

        // Dispatcher qui a créé la livraison
        public int? DispatcheurId { get; set; }
        public Dispatcher? Dispatcher { get; set; }

        public ICollection<PointTracking> PointsTracking { get; set; } = new List<PointTracking>();
        public ICollection<Anomalie> Anomalies { get; set; } = new List<Anomalie>();
    }
}
