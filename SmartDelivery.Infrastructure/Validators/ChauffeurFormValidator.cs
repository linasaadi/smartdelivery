using FluentValidation;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.Infrastructure.Validators
{
    public class ChauffeurFormValidator : AbstractValidator<ChauffeurFormDto>
    {
        public ChauffeurFormValidator()
        {
            RuleFor(x => x.Nom)
                .NotEmpty().WithMessage("Le nom est obligatoire.")
                .MaximumLength(100);

            RuleFor(x => x.Prenom)
                .NotEmpty().WithMessage("Le prénom est obligatoire.")
                .MaximumLength(100);

            RuleFor(x => x.NumeroPermis)
                .NotEmpty().WithMessage("Le numéro de permis est obligatoire.")
                .MaximumLength(20);

            RuleFor(x => x.Telephone)
                .NotEmpty().WithMessage("Le téléphone est obligatoire.")
                .Matches(@"^[\+0-9\s\-]{8,20}$").WithMessage("Format de téléphone invalide.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Format d'email invalide.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.DateEmbauche)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("La date d'embauche ne peut pas être dans le futur.");
        }
    }
}
