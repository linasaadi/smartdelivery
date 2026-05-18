using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var role      = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userId    = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Groupe par rôle pour les notifications ciblées
            if (!string.IsNullOrEmpty(role))
                await Groups.AddToGroupAsync(Context.ConnectionId, role);

            // Groupe personnel pour les notifications individuelles
            if (!string.IsNullOrEmpty(userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            // Groupe global pour les alertes système
            await Groups.AddToGroupAsync(Context.ConnectionId, "tous");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "tous");
            await base.OnDisconnectedAsync(exception);
        }

        // Chauffeur → signale une anomalie en temps réel
        [Authorize(Roles = "Chauffeur")]
        public async Task SignalerAnomalie(int livraisonId, AnomalieDto anomalie)
        {
            // Notifier tous les dispatchers immédiatement
            await Clients.Group("Dispatcher")
                .SendAsync("NouvelleAnomalie", new {
                    livraisonId,
                    anomalie,
                    horodatage = DateTime.UtcNow
                });

            // Notifier les admins aussi
            await Clients.Group("Admin")
                .SendAsync("NouvelleAnomalie", new {
                    livraisonId,
                    anomalie,
                    horodatage = DateTime.UtcNow
                });
        }
    }
}
