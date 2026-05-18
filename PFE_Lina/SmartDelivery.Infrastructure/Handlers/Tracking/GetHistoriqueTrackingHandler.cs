using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Queries;
using SmartDelivery.Donnees.Context;

namespace SmartDelivery.Infrastructure.Handlers.Tracking
{
    public class GetHistoriqueTrackingHandler : IRequestHandler<GetHistoriqueTrackingQuery, IEnumerable<PointTracking>>
    {
        private readonly ApplicationDbContext _contexte;
        public GetHistoriqueTrackingHandler(ApplicationDbContext contexte) => _contexte = contexte;

        public async Task<IEnumerable<PointTracking>> Handle(GetHistoriqueTrackingQuery requete, CancellationToken annulation)
            => await _contexte.PointsTracking
                .Where(pt => pt.LivraisonId == requete.LivraisonId)
                .OrderBy(pt => pt.Horodatage)
                .AsNoTracking()
                .ToListAsync(annulation);
    }
}
