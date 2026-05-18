using MediatR;
using SmartDelivery.Domaine.Models;

namespace SmartDelivery.Domaine.Queries
{
    public class GetCamionsDisponiblesQuery : IRequest<IEnumerable<Camion>> { }
}
