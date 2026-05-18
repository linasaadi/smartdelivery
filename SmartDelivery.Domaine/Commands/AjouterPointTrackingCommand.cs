using MediatR;

namespace SmartDelivery.Domaine.Commands
{
    public class AjouterPointTrackingCommand : IRequest<bool>
    {
        public int LivraisonId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double VitesseKmh { get; set; }
        public string? Adresse { get; set; }
    }
}
