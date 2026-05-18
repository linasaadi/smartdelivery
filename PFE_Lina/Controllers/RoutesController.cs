using Microsoft.AspNetCore.Mvc;
using SmartDelivery.Domaine.Interface;
using SmartDelivery.Domaine.Models;

namespace SmartDelivery.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly IOptimisationRouteService _optimisationRoute;
        public RoutesController(IOptimisationRouteService optimisationRoute) => _optimisationRoute = optimisationRoute;

        // POST api/routes/shortest
        [HttpPost("shortest")]
        public IActionResult RoutePlusCourte([FromBody] RequeteOptimisation requete)
        {
            var resultat = _optimisationRoute.CalculerRoutePlusCourte(requete);
            return Ok(resultat);
        }

        // POST api/routes/fastest
        [HttpPost("fastest")]
        public IActionResult RoutePlusRapide([FromBody] RequeteOptimisation requete)
        {
            var resultat = _optimisationRoute.CalculerRoutePlusRapide(requete);
            return Ok(resultat);
        }

        // POST api/routes/multi-stop
        [HttpPost("multi-stop")]
        public IActionResult OptimiserMultiArrets([FromBody] RequeteMultiArret requete)
        {
            if (!requete.Destinations.Any())
                return BadRequest("Aucune destination fournie.");

            var resultat = _optimisationRoute.OptimiserMultiArrets(requete);
            return Ok(resultat);
        }
    }
}
