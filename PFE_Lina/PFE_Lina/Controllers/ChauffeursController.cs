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
    [Authorize(Roles = "Admin,Dispatcher")]
    public class ChauffeursController : ControllerBase
    {
        private readonly IMediator _mediateur;
        private readonly IMapper _mapper;

        public ChauffeursController(IMediator mediateur, IMapper mapper)
        {
            _mediateur = mediateur;
            _mapper    = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenirTous()
        {
            var chauffeurs = await _mediateur.Send(new GetListGenericQuery<Chauffeur>());
            return Ok(ResultatOperation<IEnumerable<ChauffeurDetailDto>>.Ok(
                _mapper.Map<IEnumerable<ChauffeurDetailDto>>(chauffeurs)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenirParId(int id)
        {
            var chauffeur = await _mediateur.Send(new GetGenericQuery<Chauffeur>(id));
            if (chauffeur == null)
                return NotFound(ResultatOperation<ChauffeurDetailDto>.NonTrouve($"Chauffeur {id} introuvable."));
            return Ok(ResultatOperation<ChauffeurDetailDto>.Ok(_mapper.Map<ChauffeurDetailDto>(chauffeur)));
        }

        [HttpPost]
        public async Task<IActionResult> Creer([FromBody] ChauffeurFormDto dto)
        {
            var chauffeur = _mapper.Map<Chauffeur>(dto);
            var cree      = await _mediateur.Send(new AddGenericCommand<Chauffeur>(chauffeur));
            return CreatedAtAction(nameof(ObtenirParId), new { id = cree.Id },
                ResultatOperation<ChauffeurDetailDto>.Ok(
                    _mapper.Map<ChauffeurDetailDto>(cree), "Chauffeur créé avec succès."));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Modifier(int id, [FromBody] ChauffeurFormDto dto)
        {
            var chauffeur  = _mapper.Map<Chauffeur>(dto);
            chauffeur.Id   = id;
            var modifie    = await _mediateur.Send(new PutGenericCommand<Chauffeur>(chauffeur));
            return Ok(ResultatOperation<ChauffeurDetailDto>.Ok(_mapper.Map<ChauffeurDetailDto>(modifie)));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var succes = await _mediateur.Send(new DeleteGenericCommand<Chauffeur>(id));
            if (!succes)
                return NotFound(ResultatOperation<bool>.NonTrouve($"Chauffeur {id} introuvable."));
            return Ok(ResultatOperation<bool>.Vide("Chauffeur supprimé."));
        }
    }
}
