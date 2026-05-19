using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Domaine.Models
{
    public class Reclamation
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public StatutReclamation Statut { get; set; } = StatutReclamation.EnAttente;
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public DateTime? DateResolution { get; set; }
        public string? ReponseAdmin { get; set; }

        // Créateur (Chauffeur ou Dispatcher)
        public string UtilisateurId { get; set; } = string.Empty;
        public UtilisateurSmartDelivery? Utilisateur { get; set; }

        // Livraison concernée (optionnel)
        public int? LivraisonId { get; set; }
        public Livraison? Livraison { get; set; }
    }
}
