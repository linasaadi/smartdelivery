using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Interface;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Queries;

namespace SmartDelivery.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrackingController : ControllerBase
    {
        private readonly IMediator _mediateur;
        private readonly IDetectionAnomalieService _detectionAnomalie;

        public TrackingController(IMediator mediateur, IDetectionAnomalieService detectionAnomalie)
        {
            _mediateur = mediateur;
            _detectionAnomalie = detectionAnomalie;
        }

        // POST api/tracking  — réception d'une position GPS
        [HttpPost]
        public async Task<IActionResult> EnregistrerPosition([FromBody] AjouterPointTrackingCommand commande)
        {
            var succes = await _mediateur.Send(commande);
            if (!succes) return NotFound($"Livraison {commande.LivraisonId} introuvable.");

            // Détection automatique d'anomalie après chaque point GPS
            var livraison = await _mediateur.Send(new GetGenericQuery<Livraison>(commande.LivraisonId));
            if (livraison != null)
            {
                var points = await _mediateur.Send(new GetHistoriqueTrackingQuery(commande.LivraisonId));
                var dernierPoint = points.OrderByDescending(p => p.Horodatage).FirstOrDefault();
                var analyse = _detectionAnomalie.AnalyserLivraison(livraison, dernierPoint);

                return Ok(new { message = "Position enregistrée.", analyse });
            }

            return Ok(new { message = "Position enregistrée." });
        }

        // GET api/tracking/facteur-trafic?heure=14&zone=urbaine
        [HttpGet("facteur-trafic")]
        public IActionResult ObtenirFacteurTrafic([FromQuery] int heure = -1, [FromQuery] string? zone = null)
        {
            var heureEffective = heure >= 0 && heure <= 23 ? heure : DateTime.Now.Hour;
            var (lat, lon) = zone?.ToLowerInvariant() switch
            {
                "rurale" => (35.5, 9.5),
                _ => (36.8065, 10.1815)   // Tunis par défaut
            };
            var dateAvecHeure = DateTime.UtcNow.Date.AddHours(heureEffective);
            var facteur = _detectionAnomalie.EstimerFacteurTrafic(lat, lon, dateAvecHeure);
            return Ok(new { heure = heureEffective, facteur, zone });
        }
    }
}
