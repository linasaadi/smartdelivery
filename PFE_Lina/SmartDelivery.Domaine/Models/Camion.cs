using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Domaine.Models
{
    public class Camion
    {
        public int Id { get; set; }
        public string Immatriculation { get; set; } = string.Empty;
        public string Marque { get; set; } = string.Empty;
        public string Modele { get; set; } = string.Empty;
        public double CapaciteKg { get; set; }
        public double CapaciteM3 { get; set; }
        public StatutCamion Statut { get; set; } = StatutCamion.Disponible;
        public double? LatitudeActuelle { get; set; }
        public double? LongitudeActuelle { get; set; }
        public int? ChauffeurId { get; set; }
        public Chauffeur? Chauffeur { get; set; }

        public ICollection<Livraison> Livraisons { get; set; } = new List<Livraison>();
    }
}
