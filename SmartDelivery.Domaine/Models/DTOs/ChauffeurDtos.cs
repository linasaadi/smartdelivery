namespace SmartDelivery.Domaine.Models.DTOs
{
    // ── Résumé (pour les sous-objets) ─────────────────────────────────────────
    public record ChauffeurResumeDto(
        int Id,
        string NomComplet,
        string Telephone,
        bool EstDisponible
    );

    // ── Détail complet ────────────────────────────────────────────────────────
    public record ChauffeurDetailDto(
        int Id,
        string Nom,
        string Prenom,
        string NomComplet,
        string NumeroPermis,
        string Telephone,
        string? Email,
        DateTime DateEmbauche,
        bool EstDisponible,
        List<CamionResumeDto> Camions
    );

    // ── Création / Modification ───────────────────────────────────────────────
    public record ChauffeurFormDto(
        string Nom,
        string Prenom,
        string NumeroPermis,
        string Telephone,
        string? Email,
        DateTime DateEmbauche
    );
}
