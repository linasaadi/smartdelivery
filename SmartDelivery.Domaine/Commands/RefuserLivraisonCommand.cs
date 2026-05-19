using MediatR;

namespace SmartDelivery.Domaine.Commands
{
    public record RefuserLivraisonCommand(
        int    LivraisonId,
        int    ChauffeurId,
        string RaisonRefus
    ) : IRequest<bool>;
}
