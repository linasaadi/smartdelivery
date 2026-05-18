using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Models.Enums;
using SmartDelivery.Donnees.Context;

namespace SmartDelivery.Infrastructure.Handlers.Livraisons
{
    public class ModifierStatutLivraisonHandler : IRequestHandler<ModifierStatutLivraisonCommand, bool>
    {
        private readonly ApplicationDbContext _contexte;
        public ModifierStatutLivraisonHandler(ApplicationDbContext contexte) => _contexte = contexte;

        public async Task<bool> Handle(ModifierStatutLivraisonCommand requete, CancellationToken annulation)
        {
            var livraison = await _contexte.Livraisons.FindAsync(requete.LivraisonId);
            if (livraison == null) return false;

            livraison.Statut = requete.NouveauStatut;

            if (requete.NouveauStatut == StatutLivraison.Livree)
                livraison.DateLivraisonReelle = DateTime.UtcNow;

            await _contexte.SaveChangesAsync(annulation);
            return true;
        }
    }
}
