using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Queries;
using SmartDelivery.Donnees.Context;

namespace SmartDelivery.Infrastructure.Handlers.Camions
{
    public class GetCamionsParChauffeurHandler(ApplicationDbContext _ctx)
        : IRequestHandler<GetCamionsParChauffeurQuery, IEnumerable<Camion>>
    {
        public async Task<IEnumerable<Camion>> Handle(
            GetCamionsParChauffeurQuery requete, CancellationToken ct)
        {
            return await _ctx.Camions
                .Include(c => c.Chauffeur)
                .Where(c => c.ChauffeurId == requete.ChauffeurId)
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}
