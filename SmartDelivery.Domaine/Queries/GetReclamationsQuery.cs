using MediatR;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.Domaine.Queries
{
    public record GetReclamationsQuery(
        string? UtilisateurId,  // null = Admin voit tout
        bool    EstAdmin
    ) : IRequest<IEnumerable<ReclamationDto>>;
}
