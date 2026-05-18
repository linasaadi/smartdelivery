using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.Enums;
using SmartDelivery.Domaine.Queries;
using SmartDelivery.Donnees.Context;

namespace SmartDelivery.Infrastructure.Handlers.Camions
{
    public class GetCamionsDisponiblesHandler : IRequestHandler<GetCamionsDisponiblesQuery, IEnumerable<Camion>>
    {
        private readonly ApplicationDbContext _contexte;
        public GetCamionsDisponiblesHandler(ApplicationDbContext contexte) => _contexte = contexte;

        public async Task<IEnumerable<Camion>> Handle(GetCamionsDisponiblesQuery requete, CancellationToken annulation)
            => await _contexte.Camions
                .Include(c => c.Chauffeur)
                .Where(c => c.Statut == StatutCamion.Disponible && c.ChauffeurId != null)
                .AsNoTracking()
                .ToListAsync(annulation);
    }
}
