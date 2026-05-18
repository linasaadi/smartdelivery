using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Domaine.Models.DTOs
{
    // ── Lecture liste ─────────────────────────────────────────────────────────
    public record LivraisonResumeDto(
        int Id,
        string Reference,
        string Statut,
        string? NomChauffeur,
        string? ImmatriculationCamion,
        string Destination,
        DateTime? DateLivraisonPrevue,
        DateTime? DateLivraisonReelle,
        DateTime? ETA,
        decimal Cout,
        int NombreAnomaliesOuvertes
    );

    // ── Lecture détail ────────────────────────────────────────────────────────
    public record LivraisonDetailDto(
        int Id,
        string Reference,
        string Statut,
        CamionResumeDto? Camion,
        DestinationDto Destination,
        List<LivraisonProduitDto> Produits,
        List<AnomalieDto> Anomalies,
        DateTime DateCreation,
        DateTime? DateLivraisonPrevue,
        DateTime? DateLivraisonReelle,
        DateTime? ETA,
        decimal Cout,
        string? Notes
    );

    // ── Création (Dispatcher uniquement) ──────────────────────────────────────
    public record CreerLivraisonDto(
        int CamionId,
        int DestinationId,
        DateTime DateLivraisonPrevue,
        List<LivraisonProduitDto> Produits,
        string? Notes
    );

    // ── Modification statut ───────────────────────────────────────────────────
    public record ModifierStatutDto(
        StatutLivraison NouveauStatut
    );

    // ── Produit dans livraison ────────────────────────────────────────────────
    public record LivraisonProduitDto(
        int ProduitId,
        string? NomProduit,
        int Quantite,
        decimal PrixTotal
    );
}
