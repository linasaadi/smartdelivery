using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.Enums;
using SmartDelivery.Domaine.Queries;
using SmartDelivery.Donnees.Context;

namespace SmartDelivery.Infrastructure.Handlers.Livraisons
{
    public class GetLivraisonsEnRetardHandler : IRequestHandler<GetLivraisonsEnRetardQuery, IEnumerable<Livraison>>
    {
        private readonly ApplicationDbContext _contexte;
        public GetLivraisonsEnRetardHandler(ApplicationDbContext contexte) => _contexte = contexte;

        public async Task<IEnumerable<Livraison>> Handle(GetLivraisonsEnRetardQuery requete, CancellationToken annulation)
        {
            var maintenant = DateTime.UtcNow;
            return await _contexte.Livraisons
                .Include(l => l.Camion).ThenInclude(c => c!.Chauffeur)
                .Include(l => l.Destination)
                .Where(l => (l.Statut == StatutLivraison.EnCours && l.DateLivraisonPrevue < maintenant)
                         || l.Statut == StatutLivraison.EnRetard)
                .OrderBy(l => l.DateLivraisonPrevue)
                .AsNoTracking()
                .ToListAsync(annulation);
        }
    }
}
