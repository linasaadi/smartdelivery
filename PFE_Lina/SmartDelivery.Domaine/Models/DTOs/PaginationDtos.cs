namespace SmartDelivery.Domaine.Models.DTOs
{
    public record ParametresPagination(
        int Page       = 1,
        int TaillePage = 20,
        string? Tri    = null,
        bool Croissant = false
    );

    public record PageDonnees<T>(
        List<T> Elements,
        int     TotalElements,
        int     Page,
        int     TaillePage,
        int     TotalPages,
        bool    APrecedent,
        bool    ASuivant
    )
    {
        public static PageDonnees<T> Creer(List<T> elements, int total,
            int page, int taillePage) => new(
            Elements      : elements,
            TotalElements : total,
            Page          : page,
            TaillePage    : taillePage,
            TotalPages    : (int)Math.Ceiling(total / (double)taillePage),
            APrecedent    : page > 1,
            ASuivant      : page < (int)Math.Ceiling(total / (double)taillePage)
        );
    }
}
