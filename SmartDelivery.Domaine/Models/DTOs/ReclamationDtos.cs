namespace SmartDelivery.Domaine.Models.DTOs
{
    // ── Lecture ───────────────────────────────────────────────────────────────────
    public record ReclamationDto(
        int Id,
        string Titre,
        string Description,
        string Statut,
        DateTime DateCreation,
        DateTime? DateResolution,
        string? ReponseAdmin,
        string UtilisateurId,
        string? NomUtilisateur,
        string? RoleUtilisateur,
        int? LivraisonId,
        string? ReferenceLivraison
    );

    // ── Création / Modification (Chauffeur ou Dispatcher) ─────────────────────────
    public record ReclamationFormDto(
        string Titre,
        string Description,
        int? LivraisonId
    );

    // ── Réponse Admin (statut + commentaire) ─────────────────────────────────────
    public record RepondreReclamationDto(
        string NouveauStatut,
        string? ReponseAdmin
    );
}
