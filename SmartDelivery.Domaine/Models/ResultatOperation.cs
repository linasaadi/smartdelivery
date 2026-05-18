namespace SmartDelivery.Domaine.Models
{
    public class ResultatOperation<T>
    {
        public bool Succes { get; init; }
        public T? Donnees { get; init; }
        public string? Message { get; init; }
        public List<string> Erreurs { get; init; } = [];

        public static ResultatOperation<T> Ok(T donnees, string? message = null)
            => new() { Succes = true, Donnees = donnees, Message = message };

        public static ResultatOperation<T> Vide(string? message = null)
            => new() { Succes = true, Message = message };

        public static ResultatOperation<T> Echec(string erreur)
            => new() { Succes = false, Erreurs = [erreur] };

        public static ResultatOperation<T> EchecValidation(List<string> erreurs)
            => new() { Succes = false, Erreurs = erreurs };

        public static ResultatOperation<T> NonTrouve(string? message = null)
            => new() { Succes = false, Erreurs = [message ?? "Ressource introuvable"] };
    }
}
