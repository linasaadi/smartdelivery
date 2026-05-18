using System.Text.Json;
using SmartDelivery.Domaine.Models;

namespace SmartDelivery.API.Middleware
{
    public class GestionnaireExceptionsMiddleware(
        RequestDelegate _suivant,
        ILogger<GestionnaireExceptionsMiddleware> _logger)
    {
        public async Task InvokeAsync(HttpContext contexte)
        {
            try
            {
                await _suivant(contexte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception non gérée sur {Methode} {Chemin}: {Message}",
                    contexte.Request.Method,
                    contexte.Request.Path,
                    ex.Message);

                await GererExceptionAsync(contexte, ex);
            }
        }

        private static async Task GererExceptionAsync(HttpContext contexte, Exception ex)
        {
            var (codeStatut, message) = ex switch
            {
                KeyNotFoundException      => (StatusCodes.Status404NotFound,      "Ressource introuvable"),
                UnauthorizedAccessException => (StatusCodes.Status403Forbidden,   "Accès refusé"),
                ArgumentNullException e   => (StatusCodes.Status400BadRequest,    e.Message),
                ArgumentException e       => (StatusCodes.Status400BadRequest,    e.Message),
                InvalidOperationException e => (StatusCodes.Status409Conflict,    e.Message),
                _                         => (StatusCodes.Status500InternalServerError, "Une erreur interne est survenue")
            };

            contexte.Response.StatusCode  = codeStatut;
            contexte.Response.ContentType = "application/json";

            var reponse = ResultatOperation<object>.Echec(message);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await contexte.Response.WriteAsync(JsonSerializer.Serialize(reponse, options));
        }
    }
}
