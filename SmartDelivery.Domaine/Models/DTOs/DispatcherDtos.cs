namespace SmartDelivery.Domaine.Models.DTOs
{
    public record DispatcherResumeDto(
        int Id,
        string NomComplet,
        string? Telephone,
        string? ZoneResponsabilite
    );

    public record DispatcherDetailDto(
        int Id,
        string Nom,
        string Prenom,
        string? Telephone,
        string? Email,
        DateTime DatePriseEnCharge,
        string? ZoneResponsabilite
    );

    public record DispatcherFormDto(
        string Nom,
        string Prenom,
        string? Telephone,
        string? Email,
        string? ZoneResponsabilite
    );
}
