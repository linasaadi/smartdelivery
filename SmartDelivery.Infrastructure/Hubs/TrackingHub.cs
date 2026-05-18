using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.Infrastructure.Hubs
{
    [Authorize]
    public class TrackingHub(IMediator _mediateur) : Hub
    {
        // Chauffeur → diffuse sa position et persiste le point tracking
        [Authorize(Roles = "Chauffeur")]
        public async Task EnvoyerPosition(int livraisonId, double lat, double lng, double vitesseKmh, string? adresse = null)
        {
            // Persister le point en base via MediatR
            await _mediateur.Send(new AjouterPointTrackingCommand
            {
                LivraisonId = livraisonId,
                Latitude    = lat,
                Longitude   = lng,
                VitesseKmh  = vitesseKmh,
                Adresse     = adresse
            });

            var position = new CamionPositionDto(
                CamionId    : 0,
                Latitude    : lat,
                Longitude   : lng,
                VitesseKmh  : vitesseKmh,
                Horodatage  : DateTime.UtcNow
            );

            // Diffuser aux abonnés de cette livraison
            await Clients.Group($"livraison-{livraisonId}")
                .SendAsync("PositionMisAJour", new {
                    livraisonId,
                    latitude   = lat,
                    longitude  = lng,
                    vitesseKmh,
                    horodatage = DateTime.UtcNow
                });

            // Notifier les dispatchers qu'un camion est en mouvement
            await Clients.Group("Dispatcher")
                .SendAsync("CamionEnMouvement", new {
                    livraisonId,
                    latitude  = lat,
                    longitude = lng
                });
        }

        // Dispatcher/Admin → rejoindre le groupe de suivi d'une livraison
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task SuivreLivraison(int livraisonId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"livraison-{livraisonId}");

        // Quitter le suivi d'une livraison
        public async Task ArretSuivi(int livraisonId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"livraison-{livraisonId}");
    }
}
