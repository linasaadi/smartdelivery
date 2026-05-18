using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.Enums;
using SmartDelivery.Donnees.Context;

namespace SmartDelivery.Infrastructure.Handlers.Tracking
{
    public class AjouterPointTrackingHandler : IRequestHandler<AjouterPointTrackingCommand, bool>
    {
        private readonly ApplicationDbContext _contexte;
        public AjouterPointTrackingHandler(ApplicationDbContext contexte) => _contexte = contexte;

        public async Task<bool> Handle(AjouterPointTrackingCommand requete, CancellationToken annulation)
        {
            var livraison = await _contexte.Livraisons
                .Include(l => l.Destination)
                .FirstOrDefaultAsync(l => l.Id == requete.LivraisonId, annulation);
            if (livraison == null) return false;

            var point = new PointTracking
            {
                LivraisonId = requete.LivraisonId,
                Latitude    = requete.Latitude,
                Longitude   = requete.Longitude,
                VitesseKmh  = requete.VitesseKmh,
                Adresse     = requete.Adresse,
                Horodatage  = DateTime.UtcNow
            };

            await _contexte.PointsTracking.AddAsync(point, annulation);

            if (livraison.Statut == StatutLivraison.EnCours && livraison.Destination != null)
            {
                var distanceKm = CalculerDistanceHaversine(
                    requete.Latitude, requete.Longitude,
                    livraison.Destination.Latitude, livraison.Destination.Longitude);

                var vitesseMoyenne = requete.VitesseKmh > 0 ? requete.VitesseKmh : 60;
                livraison.ETA = DateTime.UtcNow.AddHours(distanceKm / vitesseMoyenne);
            }

            await _contexte.SaveChangesAsync(annulation);
            return true;
        }

        private static double CalculerDistanceHaversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double RayonTerreKm = 6371.0;
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return RayonTerreKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}
