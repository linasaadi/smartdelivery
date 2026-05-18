namespace SmartDelivery.Domaine.Models
{
    public class Destination
    {
        public int Id { get; set; }
        public string Adresse { get; set; } = string.Empty;
        public string Ville { get; set; } = string.Empty;
        public string CodePostal { get; set; } = string.Empty;
        public string? Pays { get; set; } = "Tunisie";
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public ICollection<Livraison> Livraisons { get; set; } = new List<Livraison>();
    }
}
