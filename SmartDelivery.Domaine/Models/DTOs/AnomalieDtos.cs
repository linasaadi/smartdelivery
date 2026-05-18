using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Domaine.Models.DTOs
{
    public record AnomalieDto(
        int Id,
        string Type,
        string Description,
        DateTime DateDetection,
        bool EstResolue,
        DateTime? DateResolution,
        int RetardMinutes,
        int LivraisonId
    );

    // ── Signalement par le chauffeur ──────────────────────────────────────────
    public record SignalerAnomalieDto(
        TypeAnomalie Type,
        string Description,
        int RetardMinutes
    );
}
