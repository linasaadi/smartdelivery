using SmartDelivery.Domaine.Interface;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Infrastructure.Services
{
    public class DetectionAnomalieService : IDetectionAnomalieService
    {
        private const double VitesseMaxNormaleKmh = 120.0;
        private const double VitesseMinSuspicieuseKmh = 5.0;
        private const int SeuilRetardMinutes = 30;

        public AnalyseAnomalie AnalyserLivraison(Livraison livraison, PointTracking? dernierPoint)
        {
            var analyse = new AnalyseAnomalie { AnomalieDetectee = false };

            // Règle 1 : Retard par rapport à la date prévue
            if (livraison.DateLivraisonPrevue.HasValue
                && livraison.Statut == StatutLivraison.EnCours)
            {
                var retardMinutes = (int)(DateTime.UtcNow - livraison.DateLivraisonPrevue.Value).TotalMinutes;
                if (retardMinutes > SeuilRetardMinutes)
                {
                    analyse.AnomalieDetectee = true;
                    analyse.TypeAnomalie = TypeAnomalie.Retard;
                    analyse.RetardEstimeMinutes = retardMinutes;
                    analyse.Description = $"Livraison en retard de {retardMinutes} minutes par rapport à la prévision.";
                }
            }

            // Règle 2 : ETA dépassée
            if (!analyse.AnomalieDetectee && livraison.ETA.HasValue
                && livraison.ETA.Value < DateTime.UtcNow
                && livraison.Statut == StatutLivraison.EnCours)
            {
                var retard = (int)(DateTime.UtcNow - livraison.ETA.Value).TotalMinutes;
                analyse.AnomalieDetectee = true;
                analyse.TypeAnomalie = TypeAnomalie.Retard;
                analyse.RetardEstimeMinutes = retard;
                analyse.Description = $"L'ETA a été dépassée de {retard} minutes.";
            }

            // Règle 3 : Vitesse anormale sur le dernier point GPS
            if (dernierPoint != null && DetecterVitesseAnormale(dernierPoint.VitesseKmh))
            {
                analyse.AnomalieDetectee = true;
                analyse.TypeAnomalie = dernierPoint.VitesseKmh < VitesseMinSuspicieuseKmh
                    ? TypeAnomalie.PanneVehicule
                    : TypeAnomalie.VitesseAnormale;
                analyse.Description = dernierPoint.VitesseKmh < VitesseMinSuspicieuseKmh
                    ? $"Le camion est presque immobile (vitesse : {dernierPoint.VitesseKmh} km/h) — panne possible."
                    : $"Vitesse excessive détectée : {dernierPoint.VitesseKmh} km/h.";
            }

            // Suggestion de route alternative si anomalie
            if (analyse.AnomalieDetectee && dernierPoint != null)
            {
                var facteur = EstimerFacteurTrafic(dernierPoint.Latitude, dernierPoint.Longitude, DateTime.UtcNow);
                analyse.FacteurTrafic = facteur;

                if (facteur > 1.5)
                    analyse.RouteAlternativeSuggeree =
                        "Trafic dense détecté. Suggérer une route alternative via les voies secondaires.";
                else
                    analyse.RouteAlternativeSuggeree = "Continuer sur l'itinéraire actuel.";
            }

            return analyse;
        }

        public double EstimerFacteurTrafic(double latitude, double longitude, DateTime heure)
        {
            // Modèle simplifié : facteur basé sur l'heure + zone géographique
            var facteurHeure = heure.Hour switch
            {
                >= 7 and <= 9 => 1.8,
                >= 12 and <= 14 => 1.4,
                >= 17 and <= 19 => 1.9,
                >= 22 or <= 5 => 0.7,
                _ => 1.1
            };

            // Légère variation selon la position (simulation zone urbaine vs rurale)
            var estZoneUrbaine = Math.Abs(latitude) < 37 && Math.Abs(longitude) < 11;
            var facteurZone = estZoneUrbaine ? 1.2 : 1.0;

            return Math.Round(facteurHeure * facteurZone, 2);
        }

        public bool DetecterVitesseAnormale(double vitesseKmh, double vitesseMoyenneAttendue = 80.0)
            => vitesseKmh > VitesseMaxNormaleKmh || vitesseKmh < VitesseMinSuspicieuseKmh;
    }
}
