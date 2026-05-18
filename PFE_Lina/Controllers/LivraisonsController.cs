using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Interface;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;
using SmartDelivery.Domaine.Models.Enums;
using SmartDelivery.Domaine.Queries;
using SmartDelivery.Infrastructure.Handlers.Livraisons;

namespace SmartDelivery.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LivraisonsController : ControllerBase
    {
        private readonly IMediator _mediateur;
        private readonly IMapper _mapper;
        private readonly IDetectionAnomalieService _detectionAnomalie;

        public LivraisonsController(IMediator mediateur, IMapper mapper,
            IDetectionAnomalieService detectionAnomalie)
        {
            _mediateur = mediateur;
            _mapper = mapper;
            _detectionAnomalie = detectionAnomalie;
        }

        // GET api/livraisons?page=1&taillePage=20&statut=EnCours&recherche=LIV
        [HttpGet]
        public async Task<IActionResult> ObtenirTous(
            [FromQuery] int page       = 1,
            [FromQuery] int taillePage = 20,
            [FromQuery] SmartDelivery.Domaine.Models.Enums.StatutLivraison? statut = null,
            [FromQuery] string? recherche = null)
        {
            var resultat = await _mediateur.Send(
                new GetLivraisonsPagineeQuery(page, taillePage, statut, null, recherche));
            return Ok(ResultatOperation<PageDonnees<LivraisonResumeDto>>.Ok(resultat));
        }

        // GET api/livraisons/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenirParId(int id)
        {
            var livraison = await _mediateur.Send(new GetGenericQuery<Livraison>(id));
            if (livraison == null)
                return NotFound(ResultatOperation<LivraisonDetailDto>.NonTrouve($"Livraison {id} introuvable."));
            return Ok(ResultatOperation<LivraisonDetailDto>.Ok(
                _mapper.Map<LivraisonDetailDto>(livraison)));
        }

        // GET api/livraisons/delayed — Admin et Dispatcher
        [HttpGet("delayed")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> ObtenirEnRetard()
        {
            var livraisons = await _mediateur.Send(new GetLivraisonsEnRetardQuery());
            return Ok(ResultatOperation<IEnumerable<LivraisonResumeDto>>.Ok(
                _mapper.Map<IEnumerable<LivraisonResumeDto>>(livraisons)));
        }

        // POST api/livraisons — Dispatcher uniquement
        [HttpPost]
        [Authorize(Roles = "Dispatcher")]
        public async Task<IActionResult> Creer([FromBody] CreerLivraisonDto dto)
        {
            var livraison = new Livraison
            {
                Reference            = $"LIV-{DateTime.UtcNow:yyyyMMddHHmmss}",
                DateCreation         = DateTime.UtcNow,
                Statut               = StatutLivraison.EnAttente,
                CamionId             = dto.CamionId,
                DestinationId        = dto.DestinationId,
                DateLivraisonPrevue  = dto.DateLivraisonPrevue,
                Notes                = dto.Notes
            };

            var creee = await _mediateur.Send(new AddGenericCommand<Livraison>(livraison));
            return CreatedAtAction(nameof(ObtenirParId), new { id = creee.Id },
                ResultatOperation<LivraisonResumeDto>.Ok(
                    _mapper.Map<LivraisonResumeDto>(creee), "Livraison créée avec succès."));
        }

        // PUT api/livraisons/{id}/status — Chauffeur (accepter/rejeter) + Dispatcher (annuler)
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Chauffeur,Dispatcher,Admin")]
        public async Task<IActionResult> ModifierStatut(int id, [FromBody] ModifierStatutDto dto)
        {
            var succes = await _mediateur.Send(new ModifierStatutLivraisonCommand
            {
                LivraisonId   = id,
                NouveauStatut = dto.NouveauStatut
            });

            if (!succes)
                return NotFound(ResultatOperation<bool>.NonTrouve($"Livraison {id} introuvable."));
            return Ok(ResultatOperation<bool>.Vide("Statut mis à jour."));
        }

        // DELETE api/livraisons/{id} — Admin uniquement
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var succes = await _mediateur.Send(new DeleteGenericCommand<Livraison>(id));
            if (!succes)
                return NotFound(ResultatOperation<bool>.NonTrouve($"Livraison {id} introuvable."));
            return Ok(ResultatOperation<bool>.Vide("Livraison supprimée."));
        }

        // GET api/livraisons/{id}/tracking
        [HttpGet("{id:int}/tracking")]
        public async Task<IActionResult> ObtenirTracking(int id)
        {
            var points = await _mediateur.Send(new GetHistoriqueTrackingQuery(id));
            return Ok(ResultatOperation<IEnumerable<PointTrackingDto>>.Ok(
                _mapper.Map<IEnumerable<PointTrackingDto>>(points)));
        }

        // GET api/livraisons/{id}/anomalies
        [HttpGet("{id:int}/anomalies")]
        public async Task<IActionResult> AnalyserAnomalies(int id)
        {
            var livraison = await _mediateur.Send(new GetGenericQuery<Livraison>(id));
            if (livraison == null)
                return NotFound(ResultatOperation<object>.NonTrouve());

            var points      = await _mediateur.Send(new GetHistoriqueTrackingQuery(id));
            var dernierPoint = points.OrderByDescending(p => p.Horodatage).FirstOrDefault();
            var analyse      = _detectionAnomalie.AnalyserLivraison(livraison, dernierPoint);

            return Ok(ResultatOperation<object>.Ok(analyse));
        }
    }
}
