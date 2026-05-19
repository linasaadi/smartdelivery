using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Models.Enums;
using SmartDelivery.Donnees.Context;
using SmartDelivery.Infrastructure.Hubs;

namespace SmartDelivery.Infrastructure.Handlers.Livraisons
{
    public class RefuserLivraisonHandler(
        ApplicationDbContext _ctx,
        IHubContext<NotificationHub> _notificationHub)
        : IRequestHandler<RefuserLivraisonCommand, bool>
    {
        public async Task<bool> Handle(RefuserLivraisonCommand cmd, CancellationToken ct)
        {
            var livraison = await _ctx.Livraisons
                .Include(l => l.Camion)
                .FirstOrDefaultAsync(l => l.Id == cmd.LivraisonId, ct);

            if (livraison == null) return false;

            // Vérifier que ce chauffeur est bien assigné à cette livraison
            if (livraison.Camion?.ChauffeurId != cmd.ChauffeurId) return false;

            livraison.Statut      = StatutLivraison.Refusee;
            livraison.RaisonRefus = cmd.RaisonRefus;

            await _ctx.SaveChangesAsync(ct);

            // Notifier les dispatchers en temps réel
            await _notificationHub.Clients.Group("Dispatcher")
                .SendAsync("LivraisonRefusee", new
                {
                    livraisonId  = livraison.Id,
                    reference    = livraison.Reference,
                    raisonRefus  = cmd.RaisonRefus,
                    horodatage   = DateTime.UtcNow
                }, ct);

            return true;
        }
    }
}
