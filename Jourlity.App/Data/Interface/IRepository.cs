using System.Linq.Expressions;

namespace Jourlity.App.Data.Interface;

/// <summary>
/// Represents a generic repository for entities of type T.
/// </summary>
/// <typeparam name="T">The type of the entity. Must be a class inheriting from BaseEntity.</typeparam>
public interface IRepository<T> where T : IBaseEntity
{
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync();

    /// <summary>
    /// Finds entities based on a predicate.
    /// </summary>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);

    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}