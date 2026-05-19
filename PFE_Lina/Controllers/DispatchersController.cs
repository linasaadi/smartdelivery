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
    public class DispatchersController : ControllerBase
    {
        private readonly IMediator _mediateur;
        private readonly IMapper   _mapper;

        public DispatchersController(IMediator mediateur, IMapper mapper)
        {
            _mediateur = mediateur;
            _mapper    = mapper;
        }

        // GET api/dispatchers — Admin uniquement
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObtenirTous()
        {
            var dispatchers = await _mediateur.Send(new GetListGenericQuery<Dispatcher>());
            return Ok(ResultatOperation<IEnumerable<DispatcherDetailDto>>.Ok(
                _mapper.Map<IEnumerable<DispatcherDetailDto>>(dispatchers)));
        }

        // GET api/dispatchers/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObtenirParId(int id)
        {
            var dispatcher = await _mediateur.Send(new GetGenericQuery<Dispatcher>(id));
            if (dispatcher == null)
                return NotFound(ResultatOperation<DispatcherDetailDto>.NonTrouve($"Dispatcher {id} introuvable."));
            return Ok(ResultatOperation<DispatcherDetailDto>.Ok(
                _mapper.Map<DispatcherDetailDto>(dispatcher)));
        }

        // POST api/dispatchers — Admin uniquement
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Creer([FromBody] DispatcherFormDto dto)
        {
            var dispatcher = _mapper.Map<Dispatcher>(dto);
            var cree = await _mediateur.Send(new AddGenericCommand<Dispatcher>(dispatcher));
            return CreatedAtAction(nameof(ObtenirParId), new { id = cree.Id },
                ResultatOperation<DispatcherDetailDto>.Ok(
                    _mapper.Map<DispatcherDetailDto>(cree), "Dispatcher créé avec succès."));
        }

        // PUT api/dispatchers/{id} — Admin uniquement
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Modifier(int id, [FromBody] DispatcherFormDto dto)
        {
            var existant = await _mediateur.Send(new GetGenericQuery<Dispatcher>(id));
            if (existant == null)
                return NotFound(ResultatOperation<object>.NonTrouve($"Dispatcher {id} introuvable."));

            _mapper.Map(dto, existant);
            await _mediateur.Send(new PutGenericCommand<Dispatcher>(existant));
            return Ok(ResultatOperation<object>.Vide("Dispatcher mis à jour."));
        }

        // DELETE api/dispatchers/{id} — Admin uniquement
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var succes = await _mediateur.Send(new DeleteGenericCommand<Dispatcher>(id));
            if (!succes)
                return NotFound(ResultatOperation<bool>.NonTrouve($"Dispatcher {id} introuvable."));
            return Ok(ResultatOperation<bool>.Vide("Dispatcher supprimé."));
        }
    }
}
