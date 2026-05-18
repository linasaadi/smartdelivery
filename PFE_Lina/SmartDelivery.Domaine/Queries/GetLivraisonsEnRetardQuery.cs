using MediatR;
using SmartDelivery.Domaine.Models;

namespace SmartDelivery.Domaine.Queries
{
    public class GetLivraisonsEnRetardQuery : IRequest<IEnumerable<Livraison>> { }
}
