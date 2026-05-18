namespace SmartDelivery.Domaine.Models
{
    public class LivraisonProduit
    {
        public int LivraisonId { get; set; }
        public Livraison Livraison { get; set; } = null!;

        public int ProduitId { get; set; }
        public Produit Produit { get; set; } = null!;

        public int Quantite { get; set; }
        public decimal PrixTotal { get; set; }
    }
}
