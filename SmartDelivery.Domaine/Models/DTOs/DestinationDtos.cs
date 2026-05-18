namespace SmartDelivery.Domaine.Models.DTOs
{
    public record DestinationDto(
        int Id,
        string Adresse,
        string Ville,
        string CodePostal,
        string? Pays,
        double Latitude,
        double Longitude
    );

    public record DestinationFormDto(
        string Adresse,
        string Ville,
        string CodePostal,
        string? Pays,
        double Latitude,
        double Longitude
    );
}
