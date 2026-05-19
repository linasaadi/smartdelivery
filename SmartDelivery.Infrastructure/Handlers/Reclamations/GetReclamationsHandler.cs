using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;
using SmartDelivery.Domaine.Queries;
using SmartDelivery.Donnees.Context;

namespace SmartDelivery.Infrastructure.Handlers.Reclamations
{
    public class GetReclamationsHandler(
        ApplicationDbContext _ctx,
        UserManager<UtilisateurSmartDelivery> _userManager)
        : IRequestHandler<GetReclamationsQuery, IEnumerable<ReclamationDto>>
    {
        public async Task<IEnumerable<ReclamationDto>> Handle(
            GetReclamationsQuery requete, CancellationToken ct)
        {
            var query = _ctx.Reclamations
                .Include(r => r.Utilisateur)
                .Include(r => r.Livraison)
                .AsNoTracking()
                .AsQueryable();

            if (!requete.EstAdmin && requete.UtilisateurId != null)
                query = query.Where(r => r.UtilisateurId == requete.UtilisateurId);

            var reclamations = await query
                .OrderByDescending(r => r.DateCreation)
                .ToListAsync(ct);

            // Récupérer les rôles en dehors de la requête EF (Identity ne supporte pas les joins LINQ)
            var result = new List<ReclamationDto>();
            foreach (var r in reclamations)
            {
                string? nomUtilisateur = r.Utilisateur != null
                    ? $"{r.Utilisateur.Prenom} {r.Utilisateur.Nom}"
                    : null;

                string? roleUtilisateur = null;
                if (r.Utilisateur != null)
                {
                    var roles = await _userManager.GetRolesAsync(r.Utilisateur);
                    roleUtilisateur = roles.FirstOrDefault();
                }

                result.Add(new ReclamationDto(
                    Id:                r.Id,
                    Titre:             r.Titre,
                    Description:       r.Description,
                    Statut:            r.Statut.ToString(),
                    DateCreation:      r.DateCreation,
                    DateResolution:    r.DateResolution,
                    ReponseAdmin:      r.ReponseAdmin,
                    UtilisateurId:     r.UtilisateurId,
                    NomUtilisateur:    nomUtilisateur,
                    RoleUtilisateur:   roleUtilisateur,
                    LivraisonId:       r.LivraisonId,
                    ReferenceLivraison: r.Livraison?.Reference
                ));
            }

            return result;
        }
    }
}
