using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartDelivery.Domaine.Models;

namespace SmartDelivery.API.Middleware
{
    public class FiltreValidationAutomatique(IServiceProvider _services) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                var validator     = _services.GetService(validatorType) as IValidator;

                if (validator == null) continue;

                var validationContext = new ValidationContext<object>(argument);
                var resultat          = await validator.ValidateAsync(validationContext);

                if (!resultat.IsValid)
                {
                    var erreurs = resultat.Errors.Select(e => e.ErrorMessage).ToList();
                    context.Result = new BadRequestObjectResult(
                        ResultatOperation<object>.EchecValidation(erreurs));
                    return;
                }
            }

            await next();
        }
    }
}
