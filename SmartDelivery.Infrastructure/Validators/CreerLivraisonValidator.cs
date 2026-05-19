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

            RuleFor(x => x.NomProduit)
                .NotEmpty()
                .WithMessage("Le nom du produit est requis.")
                .MaximumLength(150)
                .WithMessage("Le nom du produit ne peut pas dépasser 150 caractères.");

            RuleFor(x => x.Quantite)
                .GreaterThan(0)
                .WithMessage("La quantité doit être supérieure à 0.");

            RuleFor(x => x.PoidsKg)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Le poids ne peut pas être négatif.");

            RuleFor(x => x.PrixUnitaire)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Le prix unitaire ne peut pas être négatif.");
        }
    }
}
