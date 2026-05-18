using MediatR;
using SmartDelivery.Domaine.Models.DTOs;
using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Domaine.Queries
{
    public record GetLivraisonsPagineeQuery(
        int              Page        = 1,
        int              TaillePage  = 20,
        StatutLivraison? Statut      = null,
        int?             CamionId    = null,
        string?          Recherche   = null
    ) : IRequest<PageDonnees<LivraisonResumeDto>>;
}
