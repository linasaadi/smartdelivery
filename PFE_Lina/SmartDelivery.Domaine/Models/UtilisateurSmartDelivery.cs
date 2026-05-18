using Microsoft.AspNetCore.Identity;

namespace SmartDelivery.Domaine.Models
{
    public class UtilisateurSmartDelivery : IdentityUser
    {
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        // Lien optionnel vers l'entité Chauffeur (si rôle = Chauffeur)
        public int? ChauffeurId { get; set; }
    }

    public static class Roles
    {
        public const string Admin      = "Admin";
        public const string Dispatcher = "Dispatcher";
        public const string Chauffeur  = "Chauffeur";

        public static readonly string[] Tous = [Admin, Dispatcher, Chauffeur];
    }
}
