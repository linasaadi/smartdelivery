using Microsoft.EntityFrameworkCore;
using SmartDelivery.Domaine.Interface;
using SmartDelivery.Donnees.Context;

namespace SmartDelivery.Donnees.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _contexte;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext contexte)
        {
            _contexte = contexte;
            _dbSet = contexte.Set<T>();
        }

        public async Task<T?> ObtenirParId(int id)
            => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> ObtenirTous()
            => await _dbSet.ToListAsync();

        public async Task<T> Ajouter(T entite)
        {
            await _dbSet.AddAsync(entite);
            await _contexte.SaveChangesAsync();
            return entite;
        }

        public async Task<T> Modifier(T entite)
        {
            _contexte.Entry(entite).State = EntityState.Modified;
            await _contexte.SaveChangesAsync();
            return entite;
        }

        public async Task<bool> Supprimer(int id)
        {
            var entite = await _dbSet.FindAsync(id);
            if (entite == null) return false;
            _dbSet.Remove(entite);
            await _contexte.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Existe(int id)
            => await _dbSet.FindAsync(id) != null;
    }
}
