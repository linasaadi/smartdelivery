using MediatR;
using SmartDelivery.Domaine.Models;

namespace SmartDelivery.Domaine.Queries
{
    public class GetHistoriqueTrackingQuery : IRequest<IEnumerable<PointTracking>>
    {
        public int LivraisonId { get; set; }
        public GetHistoriqueTrackingQuery(int livraisonId) => LivraisonId = livraisonId;
    }
}
