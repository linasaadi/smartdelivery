using SmartDelivery.Domaine.Interface;
using SmartDelivery.Domaine.Models;

namespace SmartDelivery.Infrastructure.Services
{
    public class OptimisationRouteService : IOptimisationRouteService
    {
        private const double RayonTerreKm = 6371.0;
        private const double VitesseMoyenneKmh = 80.0;

        public ResultatRoute CalculerRoutePlusCourte(RequeteOptimisation requete)
        {
            var distance = Haversine(
                requete.LatitudeDépart, requete.LongitudeDépart,
                requete.LatitudeArrivee, requete.LongitudeArrivee);

            return new ResultatRoute
            {
                DistanceKm = Math.Round(distance, 2),
                DureeEstimeeMinutes = Math.Round(distance / VitesseMoyenneKmh * 60, 0),
                TypeRoute = "PlusCourte",
                ETAEstimee = DateTime.UtcNow.AddMinutes(distance / VitesseMoyenneKmh * 60),
                Itineraire = new List<PointItineraire>
                {
                    new() { Latitude = requete.LatitudeDépart,  Longitude = requete.LongitudeDépart,  Instruction = "Départ" },
                    new() { Latitude = requete.LatitudeArrivee, Longitude = requete.LongitudeArrivee, Instruction = "Arrivée" }
                }
            };
        }

        public ResultatRoute CalculerRoutePlusRapide(RequeteOptimisation requete)
        {
            var distance = Haversine(
                requete.LatitudeDépart, requete.LongitudeDépart,
                requete.LatitudeArrivee, requete.LongitudeArrivee);

            // Facteur de trafic selon l'heure de la journée (heure locale)
            var facteurTrafic = EstimerFacteurTraficParHeure(DateTime.Now.Hour);
            var vitesseAjustee = VitesseMoyenneKmh / facteurTrafic;
            var dureeMinutes = distance / vitesseAjustee * 60;

            return new ResultatRoute
            {
                DistanceKm = Math.Round(distance * 1.05, 2),  // +5% par rapport au chemin direct (voie rapide)
                DureeEstimeeMinutes = Math.Round(dureeMinutes, 0),
                TypeRoute = "PlusRapide",
                ETAEstimee = DateTime.UtcNow.AddMinutes(dureeMinutes),
                Itineraire = new List<PointItineraire>
                {
                    new() { Latitude = requete.LatitudeDépart,  Longitude = requete.LongitudeDépart,  Instruction = "Départ — Prendre l'autoroute" },
                    new() { Latitude = requete.LatitudeArrivee, Longitude = requete.LongitudeArrivee, Instruction = "Arrivée" }
                }
            };
        }

        public ResultatMultiArret OptimiserMultiArrets(RequeteMultiArret requete)
        {
            if (!requete.Destinations.Any())
                return new ResultatMultiArret();

            // Algorithme du plus proche voisin (Greedy TSP)
            var nonVisites = new List<Destination>(requete.Destinations);
            var ordreOptimal = new List<Destination>();
            var latActuelle = requete.LatitudeDépart;
            var lonActuelle = requete.LongitudeDépart;
            var distanceTotale = 0.0;

            while (nonVisites.Any())
            {
                var plusProche = nonVisites
                    .OrderBy(d => Haversine(latActuelle, lonActuelle, d.Latitude, d.Longitude))
                    .First();

                distanceTotale += Haversine(latActuelle, lonActuelle, plusProche.Latitude, plusProche.Longitude);
                ordreOptimal.Add(plusProche);
                latActuelle = plusProche.Latitude;
                lonActuelle = plusProche.Longitude;
                nonVisites.Remove(plusProche);
            }

            return new ResultatMultiArret
            {
                OrdreOptimal = ordreOptimal,
                DistanceTotaleKm = Math.Round(distanceTotale, 2),
                DureeTotaleMinutes = Math.Round(distanceTotale / VitesseMoyenneKmh * 60, 0)
            };
        }

        private static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return RayonTerreKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        // Modèle heuristique de trafic selon l'heure (1.0 = fluide, 2.0 = embouteillages)
        private static double EstimerFacteurTraficParHeure(int heure) => heure switch
        {
            >= 7 and <= 9 => 1.8,    // Heure de pointe matin
            >= 12 and <= 14 => 1.4,  // Heure du déjeuner
            >= 17 and <= 19 => 1.9,  // Heure de pointe soir
            >= 22 or <= 5 => 0.8,    // Nuit — trafic faible
            _ => 1.1
        };
    }
}
