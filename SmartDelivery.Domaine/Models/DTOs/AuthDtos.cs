namespace SmartDelivery.Domaine.Models.DTOs
{
    public record LoginDto(string Email, string MotDePasse);

    public record RegisterDto(
        string Email,
        string MotDePasse,
        string Nom,
        string Prenom,
        string Role,
        int? ChauffeurId   = null,
        int? DispatcherId  = null);

    public record TokenResponseDto(
        string Token,
        string Email,
        string Nom,
        string Prenom,
        string Role,
        DateTime Expiration,
        int? ChauffeurId   = null,
        int? DispatcherId  = null);

    public record UtilisateurDto(
        string Id,
        string Email,
        string Nom,
        string Prenom,
        string Role,
        int? ChauffeurId  = null,
        int? DispatcherId = null);

    public record ChangerRoleDto(string NouveauRole);
}
