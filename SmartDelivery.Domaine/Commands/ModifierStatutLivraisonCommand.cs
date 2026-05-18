using MediatR;
using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Domaine.Commands
{
    public class ModifierStatutLivraisonCommand : IRequest<bool>
    {
        public int LivraisonId { get; set; }
        public StatutLivraison NouveauStatut { get; set; }
    }
}
