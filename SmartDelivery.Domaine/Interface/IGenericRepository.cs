namespace SmartDelivery.Domaine.Interface
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> ObtenirParId(int id);
        Task<IEnumerable<T>> ObtenirTous();
        Task<T> Ajouter(T entite);
        Task<T> Modifier(T entite);
        Task<bool> Supprimer(int id);
        Task<bool> Existe(int id);
    }
}
