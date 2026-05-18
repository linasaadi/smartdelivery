namespace SmartDelivery.Domaine.Models.DTOs
{
    public record PointTrackingDto(
        int Id,
        double Latitude,
        double Longitude,
        double VitesseKmh,
        DateTime Horodatage,
        string? Adresse
    );

    // ── Envoi position par le chauffeur ───────────────────────────────────────
    public record EnvoyerPositionDto(
        int LivraisonId,
        double Latitude,
        double Longitude,
        double VitesseKmh,
        string? Adresse
    );
}
