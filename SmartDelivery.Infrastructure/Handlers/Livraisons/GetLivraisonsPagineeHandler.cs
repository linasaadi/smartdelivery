using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartDelivery.Domaine.Models.DTOs;
using SmartDelivery.Domaine.Queries;
using SmartDelivery.Donnees.Context;

namespace SmartDelivery.Infrastructure.Handlers.Livraisons
{
    public class GetLivraisonsPagineeHandler(ApplicationDbContext _ctx, IMapper _mapper)
        : IRequestHandler<GetLivraisonsPagineeQuery, PageDonnees<LivraisonResumeDto>>
    {
        public async Task<PageDonnees<LivraisonResumeDto>> Handle(
            GetLivraisonsPagineeQuery requete, CancellationToken ct)
        {
            var query = _ctx.Livraisons
                .Include(l => l.Camion).ThenInclude(c => c!.Chauffeur)
                .Include(l => l.Destination)
                .Include(l => l.Anomalies)
                .AsNoTracking()
                .AsQueryable();

            if (requete.Statut.HasValue)
                query = query.Where(l => l.Statut == requete.Statut.Value);

            if (requete.CamionId.HasValue)
                query = query.Where(l => l.CamionId == requete.CamionId.Value);

            if (requete.ChauffeurId.HasValue)
                query = query.Where(l => l.Camion != null && l.Camion.ChauffeurId == requete.ChauffeurId.Value);

            if (!string.IsNullOrWhiteSpace(requete.Recherche))
            {
                var recherche = requete.Recherche.ToLower();
                query = query.Where(l =>
                    l.Reference.ToLower().Contains(recherche) ||
                    (l.Destination != null && l.Destination.Ville.ToLower().Contains(recherche)));
            }

            var total = await query.CountAsync(ct);

            var elements = await query
                .OrderByDescending(l => l.DateCreation)
                .Skip((requete.Page - 1) * requete.TaillePage)
                .Take(requete.TaillePage)
                .ToListAsync(ct);

            return PageDonnees<LivraisonResumeDto>.Creer(
                _mapper.Map<List<LivraisonResumeDto>>(elements),
                total,
                requete.Page,
                requete.TaillePage);
        }
    }
}
