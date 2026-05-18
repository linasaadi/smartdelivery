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
    public class GetKpiDispatcherHandler(ApplicationDbContext _ctx, IMapper _mapper, IMemoryCache _cache)
        : IRequestHandler<GetKpiDispatcherQuery, KpiDispatcherDto>
    {
        private const string CleCache = "kpi_dispatcher";

        public async Task<KpiDispatcherDto> Handle(GetKpiDispatcherQuery _, CancellationToken ct)
        {
            if (_cache.TryGetValue(CleCache, out KpiDispatcherDto? kpiCache))
                return kpiCache!;

            var maintenant = DateTime.UtcNow;

            var urgentes = await _ctx.Livraisons
                .Include(l => l.Camion).ThenInclude(c => c!.Chauffeur)
                .Include(l => l.Destination)
                .Where(l => l.Statut == StatutLivraison.EnRetard ||
                    (l.Statut == StatutLivraison.EnCours && l.DateLivraisonPrevue < maintenant))
                .OrderBy(l => l.DateLivraisonPrevue)
                .Take(10)
                .AsNoTracking()
                .ToListAsync(ct);

            var kpi = new KpiDispatcherDto(
                LivraisonsEnAttente   : await _ctx.Livraisons.CountAsync(l => l.Statut == StatutLivraison.EnAttente, ct),
                LivraisonsEnCours     : await _ctx.Livraisons.CountAsync(l => l.Statut == StatutLivraison.EnCours, ct),
                CamionsDisponibles    : await _ctx.Camions.CountAsync(c => c.Statut == StatutCamion.Disponible, ct),
                ChauffeursDisponibles : await _ctx.Chauffeurs.CountAsync(c => c.EstDisponible, ct),
                UrgentesEnRetard      : _mapper.Map<List<LivraisonResumeDto>>(urgentes)
            );

            _cache.Set(CleCache, kpi, TimeSpan.FromMinutes(2));
            return kpi;
        }
    }
}
