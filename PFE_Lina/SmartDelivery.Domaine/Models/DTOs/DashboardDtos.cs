namespace SmartDelivery.Domaine.Models.DTOs
{
    // ── KPI Admin — tableau de bord global ────────────────────────────────────
    public record KpiAdminDto(
        int TotalLivraisons,
        int EnAttente,
        int EnCours,
        int Livrees,
        int Annulees,
        int EnRetard,
        int TotalChauffeurs,
        int ChauffeursDisponibles,
        int TotalCamions,
        int CamionsDisponibles,
        double TauxReussite,
        List<PerformanceChauffeurDto> TopChauffeurs,
        List<LivraisonResumeDto> LivraisonsEnRetard
    );

    // ── Performances par chauffeur ────────────────────────────────────────────
    public record PerformanceChauffeurDto(
        int ChauffeurId,
        string NomComplet,
        int TotalLivraisons,
        int LivraisonsReussies,
        int LivraisonsEnRetard,
        double TauxReussite
    );

    // ── KPI Dispatcher — vue opérationnelle ───────────────────────────────────
    public record KpiDispatcherDto(
        int LivraisonsEnAttente,
        int LivraisonsEnCours,
        int CamionsDisponibles,
        int ChauffeursDisponibles,
        List<LivraisonResumeDto> UrgentesEnRetard
    );
}
