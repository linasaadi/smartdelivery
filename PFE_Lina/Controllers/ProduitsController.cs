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
    public class ProduitsController : ControllerBase
    {
        private readonly IMediator _mediateur;
        private readonly IMapper _mapper;

        public ProduitsController(IMediator mediateur, IMapper mapper)
        {
            _mediateur = mediateur;
            _mapper    = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenirTous()
        {
            var produits = await _mediateur.Send(new GetListGenericQuery<Produit>());
            return Ok(ResultatOperation<IEnumerable<ProduitDto>>.Ok(
                _mapper.Map<IEnumerable<ProduitDto>>(produits)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenirParId(int id)
        {
            var produit = await _mediateur.Send(new GetGenericQuery<Produit>(id));
            if (produit == null)
                return NotFound(ResultatOperation<ProduitDto>.NonTrouve($"Produit {id} introuvable."));
            return Ok(ResultatOperation<ProduitDto>.Ok(_mapper.Map<ProduitDto>(produit)));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> Creer([FromBody] ProduitFormDto dto)
        {
            var produit = _mapper.Map<Produit>(dto);
            var cree    = await _mediateur.Send(new AddGenericCommand<Produit>(produit));
            return CreatedAtAction(nameof(ObtenirParId), new { id = cree.Id },
                ResultatOperation<ProduitDto>.Ok(_mapper.Map<ProduitDto>(cree), "Produit créé."));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> Modifier(int id, [FromBody] ProduitFormDto dto)
        {
            var produit = _mapper.Map<Produit>(dto);
            produit.Id  = id;
            var modifie = await _mediateur.Send(new PutGenericCommand<Produit>(produit));
            return Ok(ResultatOperation<ProduitDto>.Ok(_mapper.Map<ProduitDto>(modifie)));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var succes = await _mediateur.Send(new DeleteGenericCommand<Produit>(id));
            if (!succes)
                return NotFound(ResultatOperation<bool>.NonTrouve($"Produit {id} introuvable."));
            return Ok(ResultatOperation<bool>.Vide("Produit supprimé."));
        }
    }
}
