using FluentValidation;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.Infrastructure.Validators
{
    public class CamionFormValidator : AbstractValidator<CamionFormDto>
    {
        public CamionFormValidator()
        {
            RuleFor(x => x.Immatriculation)
                .NotEmpty().WithMessage("L'immatriculation est obligatoire.")
                .MaximumLength(20).WithMessage("L'immatriculation ne peut pas dépasser 20 caractères.")
                .Matches(@"^[A-Z0-9\-]+$").WithMessage("Format d'immatriculation invalide.");

            RuleFor(x => x.Marque)
                .NotEmpty().WithMessage("La marque est obligatoire.")
                .MaximumLength(50);

            RuleFor(x => x.Modele)
                .NotEmpty().WithMessage("Le modèle est obligatoire.")
                .MaximumLength(50);

            RuleFor(x => x.CapaciteKg)
                .GreaterThan(0).WithMessage("La capacité en kg doit être supérieure à 0.")
                .LessThanOrEqualTo(50000).WithMessage("Capacité maximale : 50 000 kg.");

            RuleFor(x => x.CapaciteM3)
                .GreaterThan(0).WithMessage("La capacité en m³ doit être supérieure à 0.")
                .LessThanOrEqualTo(200).WithMessage("Capacité maximale : 200 m³.");
        }
    }
}
