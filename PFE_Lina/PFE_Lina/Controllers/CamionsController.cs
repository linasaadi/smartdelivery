using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;
using SmartDelivery.Domaine.Queries;

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

        // GET api/camions — Admin et Dispatcher
        [HttpGet]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> ObtenirTous()
        {
            var camions = await _mediateur.Send(new GetListGenericQuery<Camion>());
            return Ok(ResultatOperation<IEnumerable<CamionResumeDto>>.Ok(
                _mapper.Map<IEnumerable<CamionResumeDto>>(camions)));
        }

        // GET api/camions/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> ObtenirParId(int id)
        {
            var camion = await _mediateur.Send(new GetGenericQuery<Camion>(id));
            if (camion == null)
                return NotFound(ResultatOperation<CamionDetailDto>.NonTrouve($"Camion {id} introuvable."));
            return Ok(ResultatOperation<CamionDetailDto>.Ok(_mapper.Map<CamionDetailDto>(camion)));
        }

        // GET api/camions/available — Dispatcher (pour assigner une livraison)
        [HttpGet("available")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> ObtenirDisponibles()
        {
            var camions = await _mediateur.Send(new GetCamionsDisponiblesQuery());
            return Ok(ResultatOperation<IEnumerable<CamionResumeDto>>.Ok(
                _mapper.Map<IEnumerable<CamionResumeDto>>(camions)));
        }

        // POST api/camions — Admin et Dispatcher (ou Chauffeur pour ses propres camions)
        [HttpPost]
        [Authorize(Roles = "Admin,Dispatcher,Chauffeur")]
        public async Task<IActionResult> Creer([FromBody] CamionFormDto dto)
        {
            var camion = _mapper.Map<Camion>(dto);
            var cree   = await _mediateur.Send(new AddGenericCommand<Camion>(camion));
            return CreatedAtAction(nameof(ObtenirParId), new { id = cree.Id },
                ResultatOperation<CamionResumeDto>.Ok(
                    _mapper.Map<CamionResumeDto>(cree), "Camion ajouté avec succès."));
        }

        // PUT api/camions/{id} — Admin et Dispatcher
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> Modifier(int id, [FromBody] CamionFormDto dto)
        {
            var camion  = _mapper.Map<Camion>(dto);
            camion.Id   = id;
            var modifie = await _mediateur.Send(new PutGenericCommand<Camion>(camion));
            return Ok(ResultatOperation<CamionResumeDto>.Ok(_mapper.Map<CamionResumeDto>(modifie)));
        }

        // DELETE api/camions/{id} — Admin uniquement
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var succes = await _mediateur.Send(new DeleteGenericCommand<Camion>(id));
            if (!succes)
                return NotFound(ResultatOperation<bool>.NonTrouve($"Camion {id} introuvable."));
            return Ok(ResultatOperation<bool>.Vide("Camion supprimé."));
        }
    }
}
