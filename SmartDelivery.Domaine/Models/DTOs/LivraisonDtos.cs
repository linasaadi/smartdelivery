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
        string NomProduit,
        int Quantite,
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
        // Produit inline
        string NomProduit,
        string? DescriptionProduit,
        decimal PoidsKg,
        decimal VolumeM3,
        int Quantite,
        decimal PrixUnitaire,
        // Anomalies et autres
        List<AnomalieDto> Anomalies,
        DateTime DateCreation,
        DateTime? DateLivraisonPrevue,
        DateTime? DateLivraisonReelle,
        DateTime? ETA,
        decimal Cout,
        string? Notes,
        int? DispatcheurId
    );

    // ── Création (Dispatcher uniquement) ──────────────────────────────────────
    public record CreerLivraisonDto(
        int CamionId,
        int DestinationId,
        DateTime DateLivraisonPrevue,
        // Produit inline
        string NomProduit,
        string? DescriptionProduit,
        decimal PoidsKg,
        decimal VolumeM3,
        int Quantite,
        decimal PrixUnitaire,
        string? Notes
    );

    // ── Modification statut ───────────────────────────────────────────────────
    public record ModifierStatutDto(
        StatutLivraison NouveauStatut
    );
}
