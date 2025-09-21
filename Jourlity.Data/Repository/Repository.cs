using Jourlity.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Jourlity.Data.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly JourlityContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(JourlityContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll() => await _dbSet.ToListAsync();

        public async Task<T> GetById(Guid id) => 
            await _dbSet.FindAsync(id);

        public async Task Add(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
