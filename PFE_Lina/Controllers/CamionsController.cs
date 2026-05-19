using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;
using SmartDelivery.Domaine.Queries;
using System.Security.Claims;

namespace SmartDelivery.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CamionsController : ControllerBase
    {
        private readonly IMediator _mediateur;
        private readonly IMapper _mapper;

        public CamionsController(IMediator mediateur, IMapper mapper)
        {
            _mediateur = mediateur;
            _mapper    = mapper;
        }

        // GET api/camions — Admin et Dispatcher uniquement
        [HttpGet]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> ObtenirTous()
        {
            var camions = await _mediateur.Send(new GetListGenericQuery<Camion>());
            return Ok(ResultatOperation<IEnumerable<CamionResumeDto>>.Ok(
                _mapper.Map<IEnumerable<CamionResumeDto>>(camions)));
        }

        // GET api/camions/mes-camions — Chauffeur voit uniquement ses propres camions
        [HttpGet("mes-camions")]
        [Authorize(Roles = "Chauffeur")]
        public async Task<IActionResult> MesCamions()
        {
            var chauffeurIdStr = User.FindFirstValue("ChauffeurId");
            if (!int.TryParse(chauffeurIdStr, out int chauffeurId) || chauffeurId == 0)
                return BadRequest(ResultatOperation<object>.Echec("Compte non lié à un chauffeur."));

            var camions = await _mediateur.Send(new GetCamionsParChauffeurQuery(chauffeurId));
            return Ok(ResultatOperation<IEnumerable<CamionResumeDto>>.Ok(
                _mapper.Map<IEnumerable<CamionResumeDto>>(camions)));
        }

        // GET api/camions/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Dispatcher,Chauffeur")]
        public async Task<IActionResult> ObtenirParId(int id)
        {
            var camion = await _mediateur.Send(new GetGenericQuery<Camion>(id));
            if (camion == null)
                return NotFound(ResultatOperation<CamionDetailDto>.NonTrouve($"Camion {id} introuvable."));

            // Chauffeur ne peut voir que ses propres camions
            if (User.IsInRole(Roles.Chauffeur))
            {
                var chauffeurId = int.Parse(User.FindFirstValue("ChauffeurId") ?? "0");
                if (camion.ChauffeurId != chauffeurId)
                    return Forbid();
            }

            return Ok(ResultatOperation<CamionDetailDto>.Ok(_mapper.Map<CamionDetailDto>(camion)));
        }

        // GET api/camions/available — Dispatcher et Admin
        [HttpGet("available")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> ObtenirDisponibles()
        {
            var camions = await _mediateur.Send(new GetCamionsDisponiblesQuery());
            return Ok(ResultatOperation<IEnumerable<CamionResumeDto>>.Ok(
                _mapper.Map<IEnumerable<CamionResumeDto>>(camions)));
        }

        // POST api/camions — Admin, Dispatcher ou Chauffeur (pour ses propres camions)
        [HttpPost]
        [Authorize(Roles = "Admin,Dispatcher,Chauffeur")]
        public async Task<IActionResult> Creer([FromBody] CamionFormDto dto)
        {
            // Si c'est un chauffeur, forcer ChauffeurId au sien
            if (User.IsInRole(Roles.Chauffeur))
            {
                if (!int.TryParse(User.FindFirstValue("ChauffeurId"), out int chauffeurId) || chauffeurId == 0)
                    return BadRequest(ResultatOperation<object>.Echec("Compte non lié à un chauffeur."));

                dto = dto with { ChauffeurId = chauffeurId };
            }

            var camion = _mapper.Map<Camion>(dto);
            var cree   = await _mediateur.Send(new AddGenericCommand<Camion>(camion));
            return CreatedAtAction(nameof(ObtenirParId), new { id = cree.Id },
                ResultatOperation<CamionResumeDto>.Ok(
                    _mapper.Map<CamionResumeDto>(cree), "Camion ajouté avec succès."));
        }

        // PUT api/camions/{id} — Admin, Dispatcher ou Chauffeur (le sien seulement)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Dispatcher,Chauffeur")]
        public async Task<IActionResult> Modifier(int id, [FromBody] CamionFormDto dto)
        {
            if (User.IsInRole(Roles.Chauffeur))
            {
                var chauffeurId    = int.Parse(User.FindFirstValue("ChauffeurId") ?? "0");
                var camionExistant = await _mediateur.Send(new GetGenericQuery<Camion>(id));
                if (camionExistant == null || camionExistant.ChauffeurId != chauffeurId)
                    return Forbid();
            }

            var camion  = _mapper.Map<Camion>(dto);
            camion.Id   = id;
            var modifie = await _mediateur.Send(new PutGenericCommand<Camion>(camion));
            return Ok(ResultatOperation<CamionResumeDto>.Ok(_mapper.Map<CamionResumeDto>(modifie)));
        }

        // DELETE api/camions/{id} — Admin ou Chauffeur (le sien seulement)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Chauffeur")]
        public async Task<IActionResult> Supprimer(int id)
        {
            if (User.IsInRole(Roles.Chauffeur))
            {
                var chauffeurId    = int.Parse(User.FindFirstValue("ChauffeurId") ?? "0");
                var camionExistant = await _mediateur.Send(new GetGenericQuery<Camion>(id));
                if (camionExistant == null || camionExistant.ChauffeurId != chauffeurId)
                    return Forbid();
            }

            var succes = await _mediateur.Send(new DeleteGenericCommand<Camion>(id));
            if (!succes)
                return NotFound(ResultatOperation<bool>.NonTrouve($"Camion {id} introuvable."));
            return Ok(ResultatOperation<bool>.Vide("Camion supprimé."));
        }
    }
}
