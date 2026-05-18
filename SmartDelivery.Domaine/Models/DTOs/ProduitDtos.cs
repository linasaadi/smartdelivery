namespace SmartDelivery.Domaine.Models.DTOs
{
    public record ProduitDto(
        int Id,
        string Nom,
        string? Description,
        double PoidsKg,
        double VolumeM3,
        decimal PrixUnitaire
    );

    public record ProduitFormDto(
        string Nom,
        string? Description,
        double PoidsKg,
        double VolumeM3,
        decimal PrixUnitaire
    );
}
