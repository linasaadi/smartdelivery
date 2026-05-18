namespace SmartDelivery.Domaine.Models
{
    public class Produit
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double PoidsKg { get; set; }
        public double VolumeM3 { get; set; }
        public decimal PrixUnitaire { get; set; }

        public ICollection<LivraisonProduit> LivraisonProduits { get; set; } = new List<LivraisonProduit>();
    }
}
