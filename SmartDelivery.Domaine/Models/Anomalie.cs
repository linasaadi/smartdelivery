using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Domaine.Models
{
    public class Anomalie
    {
        public int Id { get; set; }
        public TypeAnomalie Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DateDetection { get; set; } = DateTime.UtcNow;
        public bool EstResolue { get; set; } = false;
        public DateTime? DateResolution { get; set; }
        public int RetardMinutes { get; set; }

        public int LivraisonId { get; set; }
        public Livraison Livraison { get; set; } = null!;
    }
}
