using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;
using SmartDelivery.Domaine.Models.Enums;
using SmartDelivery.Domaine.Queries;
using SmartDelivery.Donnees.Context;
using System.Security.Claims;

namespace SmartDelivery.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReclamationsController : ControllerBase
    {
        private readonly IMediator _mediateur;
        private readonly ApplicationDbContext _ctx;

        public ReclamationsController(IMediator mediateur, ApplicationDbContext ctx)
        {
            _mediateur = mediateur;
            _ctx       = ctx;
        }

        // GET api/reclamations — Admin voit tout, Chauffeur/Dispatcher voient les leurs
        [HttpGet]
        public async Task<IActionResult> ObtenirTous()
        {
            var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var estAdmin = User.IsInRole(Roles.Admin);

            var reclamations = await _mediateur.Send(
                new GetReclamationsQuery(estAdmin ? null : userId, estAdmin));

            return Ok(ResultatOperation<IEnumerable<ReclamationDto>>.Ok(reclamations));
        }

        // GET api/reclamations/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenirParId(int id)
        {
            var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var estAdmin = User.IsInRole(Roles.Admin);

            var reclamation = await _ctx.Reclamations
                .Include(r => r.Utilisateur)
                .Include(r => r.Livraison)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reclamation == null)
                return NotFound(ResultatOperation<ReclamationDto>.NonTrouve($"Réclamation {id} introuvable."));

            if (!estAdmin && reclamation.UtilisateurId != userId)
                return Forbid();

            return Ok(ResultatOperation<ReclamationDto>.Ok(MapperReclamation(reclamation)));
        }

        // POST api/reclamations — Chauffeur ou Dispatcher
        [HttpPost]
        [Authorize(Roles = "Chauffeur,Dispatcher")]
        public async Task<IActionResult> Creer([FromBody] ReclamationFormDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var reclamation = new Reclamation
            {
                Titre         = dto.Titre,
                Description   = dto.Description,
                LivraisonId   = dto.LivraisonId,
                UtilisateurId = userId,
                DateCreation  = DateTime.UtcNow,
                Statut        = StatutReclamation.EnAttente
            };

            var creee = await _mediateur.Send(new AddGenericCommand<Reclamation>(reclamation));
            return CreatedAtAction(nameof(ObtenirParId), new { id = creee.Id },
                ResultatOperation<object>.Vide("Réclamation créée avec succès."));
        }

        // PUT api/reclamations/{id} — propriétaire modifie, Admin change statut + réponse
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Modifier(int id, [FromBody] ModifierReclamationDto dto)
        {
            var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var estAdmin = User.IsInRole(Roles.Admin);

            var reclamation = await _ctx.Reclamations.FindAsync(id);
            if (reclamation == null)
                return NotFound(ResultatOperation<object>.NonTrouve($"Réclamation {id} introuvable."));

            if (!estAdmin && reclamation.UtilisateurId != userId)
                return Forbid();

            if (estAdmin)
            {
                // Admin modifie le statut et peut ajouter une réponse
                if (dto.NouveauStatut != null &&
                    Enum.TryParse<StatutReclamation>(dto.NouveauStatut, out var statut))
                {
                    reclamation.Statut = statut;
                    if (statut == StatutReclamation.Resolue)
                        reclamation.DateResolution = DateTime.UtcNow;
                }
                if (dto.ReponseAdmin != null)
                    reclamation.ReponseAdmin = dto.ReponseAdmin;
            }
            else
            {
                // Propriétaire modifie seulement si encore EnAttente
                if (reclamation.Statut != StatutReclamation.EnAttente)
                    return BadRequest(ResultatOperation<object>.Echec(
                        "Impossible de modifier une réclamation déjà prise en charge."));

                if (dto.Titre != null) reclamation.Titre = dto.Titre;
                if (dto.Description != null) reclamation.Description = dto.Description;
            }

            await _ctx.SaveChangesAsync();
            return Ok(ResultatOperation<object>.Vide("Réclamation mise à jour."));
        }

        // DELETE api/reclamations/{id} — propriétaire uniquement (pas Admin)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Chauffeur,Dispatcher")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var reclamation = await _ctx.Reclamations.FindAsync(id);
            if (reclamation == null)
                return NotFound(ResultatOperation<object>.NonTrouve($"Réclamation {id} introuvable."));

            if (reclamation.UtilisateurId != userId)
                return Forbid();

            var succes = await _mediateur.Send(new DeleteGenericCommand<Reclamation>(id));
            if (!succes)
                return NotFound(ResultatOperation<object>.NonTrouve());

            return Ok(ResultatOperation<object>.Vide("Réclamation supprimée."));
        }

        private static ReclamationDto MapperReclamation(Reclamation r)
        {
            return new ReclamationDto(
                Id:                r.Id,
                Titre:             r.Titre,
                Description:       r.Description,
                Statut:            r.Statut.ToString(),
                DateCreation:      r.DateCreation,
                DateResolution:    r.DateResolution,
                ReponseAdmin:      r.ReponseAdmin,
                UtilisateurId:     r.UtilisateurId,
                NomUtilisateur:    r.Utilisateur != null
                                       ? $"{r.Utilisateur.Prenom} {r.Utilisateur.Nom}"
                                       : null,
                RoleUtilisateur:   null,
                LivraisonId:       r.LivraisonId,
                ReferenceLivraison: r.Livraison?.Reference
            );
        }
    }

    // ── DTO pour modification par propriétaire ou Admin ────────────────────────────
    public record ModifierReclamationDto(
        string? Titre,
        string? Description,
        string? NouveauStatut,
        string? ReponseAdmin
    );
}
