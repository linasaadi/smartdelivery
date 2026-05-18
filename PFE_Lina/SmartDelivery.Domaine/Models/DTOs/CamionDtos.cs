using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Domaine.Models.DTOs
{
    // ── Résumé (pour les listes et les sous-objets) ───────────────────────────
    public record CamionResumeDto(
        int Id,
        string Immatriculation,
        string Marque,
        string Modele,
        string Statut,
        string? NomChauffeur
    );

    // ── Détail complet ────────────────────────────────────────────────────────
    public record CamionDetailDto(
        int Id,
        string Immatriculation,
        string Marque,
        string Modele,
        double CapaciteKg,
        double CapaciteM3,
        string Statut,
        double? LatitudeActuelle,
        double? LongitudeActuelle,
        ChauffeurResumeDto? Chauffeur
    );

    // ── Création / Modification ───────────────────────────────────────────────
    public record CamionFormDto(
        string Immatriculation,
        string Marque,
        string Modele,
        double CapaciteKg,
        double CapaciteM3,
        int? ChauffeurId
    );

    // ── Position GPS (mise à jour temps réel) ─────────────────────────────────
    public record CamionPositionDto(
        int CamionId,
        double Latitude,
        double Longitude,
        double VitesseKmh,
        DateTime Horodatage
    );
}
