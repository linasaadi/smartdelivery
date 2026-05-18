using FluentValidation;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.Infrastructure.Validators
{
    public class CreerLivraisonValidator : AbstractValidator<CreerLivraisonDto>
    {
        public CreerLivraisonValidator()
        {
            RuleFor(x => x.CamionId)
                .GreaterThan(0)
                .WithMessage("L'identifiant du camion est requis.");

            RuleFor(x => x.DestinationId)
                .GreaterThan(0)
                .WithMessage("La destination est requise.");

            RuleFor(x => x.DateLivraisonPrevue)
                .GreaterThan(DateTime.UtcNow.AddMinutes(30))
                .WithMessage("La date de livraison prévue doit être dans au moins 30 minutes.");

            RuleFor(x => x.Produits)
                .NotEmpty()
                .WithMessage("Au moins un produit est requis.")
                .Must(p => p.All(x => x.Quantite > 0))
                .WithMessage("La quantité de chaque produit doit être supérieure à 0.");
        }
    }
}
