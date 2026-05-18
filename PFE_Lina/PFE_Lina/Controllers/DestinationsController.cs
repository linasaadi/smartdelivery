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
    public class DestinationsController : ControllerBase
    {
        private readonly IMediator _mediateur;
        private readonly IMapper _mapper;

        public DestinationsController(IMediator mediateur, IMapper mapper)
        {
            _mediateur = mediateur;
            _mapper    = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenirTous()
        {
            var destinations = await _mediateur.Send(new GetListGenericQuery<Destination>());
            return Ok(ResultatOperation<IEnumerable<DestinationDto>>.Ok(
                _mapper.Map<IEnumerable<DestinationDto>>(destinations)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenirParId(int id)
        {
            var dest = await _mediateur.Send(new GetGenericQuery<Destination>(id));
            if (dest == null)
                return NotFound(ResultatOperation<DestinationDto>.NonTrouve($"Destination {id} introuvable."));
            return Ok(ResultatOperation<DestinationDto>.Ok(_mapper.Map<DestinationDto>(dest)));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> Creer([FromBody] DestinationFormDto dto)
        {
            var destination = _mapper.Map<Destination>(dto);
            var creee       = await _mediateur.Send(new AddGenericCommand<Destination>(destination));
            return CreatedAtAction(nameof(ObtenirParId), new { id = creee.Id },
                ResultatOperation<DestinationDto>.Ok(
                    _mapper.Map<DestinationDto>(creee), "Destination créée."));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> Modifier(int id, [FromBody] DestinationFormDto dto)
        {
            var destination = _mapper.Map<Destination>(dto);
            destination.Id  = id;
            var modifiee    = await _mediateur.Send(new PutGenericCommand<Destination>(destination));
            return Ok(ResultatOperation<DestinationDto>.Ok(_mapper.Map<DestinationDto>(modifiee)));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var succes = await _mediateur.Send(new DeleteGenericCommand<Destination>(id));
            if (!succes)
                return NotFound(ResultatOperation<bool>.NonTrouve($"Destination {id} introuvable."));
            return Ok(ResultatOperation<bool>.Vide("Destination supprimée."));
        }
    }
}
