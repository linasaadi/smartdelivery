using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Domaine.Models
{
    public class Chauffeur
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string NumeroPermis { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime DateEmbauche { get; set; }
        public bool EstDisponible { get; set; } = true;

        public ICollection<Camion> Camions { get; set; } = new List<Camion>();
    }
}
