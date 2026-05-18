using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Domaine.Interface
{
    public class AnalyseAnomalie
    {
        public bool AnomalieDetectee { get; set; }
        public TypeAnomalie? TypeAnomalie { get; set; }
        public string Description { get; set; } = string.Empty;
        public int RetardEstimeMinutes { get; set; }
        public string RouteAlternativeSuggeree { get; set; } = string.Empty;
        public double FacteurTrafic { get; set; } = 1.0;
    }

    public interface IDetectionAnomalieService
    {
        AnalyseAnomalie AnalyserLivraison(Livraison livraison, PointTracking? dernierPoint);
        double EstimerFacteurTrafic(double latitude, double longitude, DateTime heure);
        bool DetecterVitesseAnormale(double vitesseKmh, double vitesseMoyenneAttendue = 80.0);
    }
}
