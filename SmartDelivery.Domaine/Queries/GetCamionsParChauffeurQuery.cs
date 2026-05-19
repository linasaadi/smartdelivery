using MediatR;
using SmartDelivery.Domaine.Models;

namespace SmartDelivery.Domaine.Queries
{
    public record GetCamionsParChauffeurQuery(int ChauffeurId) : IRequest<IEnumerable<Camion>>;
}
