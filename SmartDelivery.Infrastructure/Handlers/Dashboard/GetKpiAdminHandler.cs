using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SmartDelivery.Domaine.Models.DTOs;
using SmartDelivery.Domaine.Models.Enums;
using SmartDelivery.Domaine.Queries;
using SmartDelivery.Donnees.Context;

namespace SmartDelivery.Infrastructure.Handlers.Dashboard
{
    public class GetKpiAdminHandler(ApplicationDbContext _ctx, IMapper _mapper, IMemoryCache _cache)
        : IRequestHandler<GetKpiAdminQuery, KpiAdminDto>
    {
        private const string CleCache = "kpi_admin";

        public async Task<KpiAdminDto> Handle(GetKpiAdminQuery _, CancellationToken ct)
        {
            if (_cache.TryGetValue(CleCache, out KpiAdminDto? kpiCache))
                return kpiCache!;

            var kpi = await CalculerKpi(ct);

            _cache.Set(CleCache, kpi, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration               = TimeSpan.FromMinutes(2)
            });

            return kpi;
        }

        private async Task<KpiAdminDto> CalculerKpi(CancellationToken ct)
        {
            var livraisons = await _ctx.Livraisons
                .Include(l => l.Camion).ThenInclude(c => c!.Chauffeur)
                .Include(l => l.Destination)
                .AsNoTracking()
                .ToListAsync(ct);

            var chauffeurs = await _ctx.Chauffeurs.AsNoTracking().ToListAsync(ct);
            var camions    = await _ctx.Camions.AsNoTracking().ToListAsync(ct);

            var total    = livraisons.Count;
            var livrees  = livraisons.Count(l => l.Statut == StatutLivraison.Livree);
            var tauxReussite = total > 0 ? Math.Round((double)livrees / total * 100, 1) : 0;

            var livraisonsEnRetard = livraisons
                .Where(l => l.Statut == StatutLivraison.EnRetard ||
                    (l.Statut == StatutLivraison.EnCours && l.DateLivraisonPrevue < DateTime.UtcNow))
                .ToList();

            var topChauffeurs = livraisons
                .Where(l => l.Camion?.Chauffeur != null)
                .GroupBy(l => l.Camion!.Chauffeur!)
                .Select(g => new PerformanceChauffeurDto(
                    ChauffeurId         : g.Key.Id,
                    NomComplet          : $"{g.Key.Prenom} {g.Key.Nom}",
                    TotalLivraisons     : g.Count(),
                    LivraisonsReussies  : g.Count(l => l.Statut == StatutLivraison.Livree),
                    LivraisonsEnRetard  : g.Count(l => l.Statut == StatutLivraison.EnRetard),
                    TauxReussite        : g.Count() > 0
                        ? Math.Round((double)g.Count(l => l.Statut == StatutLivraison.Livree) / g.Count() * 100, 1)
                        : 0
                ))
                .OrderByDescending(p => p.TauxReussite)
                .Take(5)
                .ToList();

            return new KpiAdminDto(
                TotalLivraisons      : total,
                EnAttente            : livraisons.Count(l => l.Statut == StatutLivraison.EnAttente),
                EnCours              : livraisons.Count(l => l.Statut == StatutLivraison.EnCours),
                Livrees              : livrees,
                Annulees             : livraisons.Count(l => l.Statut == StatutLivraison.Annulee),
                EnRetard             : livraisonsEnRetard.Count,
                TotalChauffeurs      : chauffeurs.Count,
                ChauffeursDisponibles: chauffeurs.Count(c => c.EstDisponible),
                TotalCamions         : camions.Count,
                CamionsDisponibles   : camions.Count(c => c.Statut == StatutCamion.Disponible),
                TauxReussite         : tauxReussite,
                TopChauffeurs        : topChauffeurs,
                LivraisonsEnRetard   : _mapper.Map<List<LivraisonResumeDto>>(livraisonsEnRetard)
            );
        }
    }
}
