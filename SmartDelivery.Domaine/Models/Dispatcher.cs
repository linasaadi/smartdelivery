namespace SmartDelivery.Domaine.Models
{
    public class Dispatcher
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public DateTime DatePriseEnCharge { get; set; } = DateTime.UtcNow;
        public string? ZoneResponsabilite { get; set; }

        public ICollection<Livraison> Livraisons { get; set; } = new List<Livraison>();
    }
}
