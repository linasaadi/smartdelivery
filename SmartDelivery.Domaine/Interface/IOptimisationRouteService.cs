using SmartDelivery.Domaine.Models;

namespace SmartDelivery.Domaine.Interface
{
    public class RequeteOptimisation
    {
        public double LatitudeDépart { get; set; }
        public double LongitudeDépart { get; set; }
        public double LatitudeArrivee { get; set; }
        public double LongitudeArrivee { get; set; }
    }

    public class PointItineraire
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Instruction { get; set; }
    }

    public class ResultatRoute
    {
        public double DistanceKm { get; set; }
        public double DureeEstimeeMinutes { get; set; }
        public string TypeRoute { get; set; } = string.Empty;
        public List<PointItineraire> Itineraire { get; set; } = new();
        public DateTime ETAEstimee { get; set; }
    }

    public class RequeteMultiArret
    {
        public double LatitudeDépart { get; set; }
        public double LongitudeDépart { get; set; }
        public List<Destination> Destinations { get; set; } = new();
    }

    public class ResultatMultiArret
    {
        public List<Destination> OrdreOptimal { get; set; } = new();
        public double DistanceTotaleKm { get; set; }
        public double DureeTotaleMinutes { get; set; }
    }

    public interface IOptimisationRouteService
    {
        ResultatRoute CalculerRoutePlusCourte(RequeteOptimisation requete);
        ResultatRoute CalculerRoutePlusRapide(RequeteOptimisation requete);
        ResultatMultiArret OptimiserMultiArrets(RequeteMultiArret requete);
    }
}
