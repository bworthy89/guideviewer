using System.Linq.Expressions;
using LiteDB;

namespace GuideViewer.Data.Repositories;

/// <summary>
/// Generic repository interface for data access operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    T? GetById(ObjectId id);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    IEnumerable<T> GetAll();

    /// <summary>
    /// Finds entities matching the predicate.
    /// </summary>
    IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Gets the first entity matching the predicate, or null.
    /// </summary>
    T? FirstOrDefault(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Inserts a new entity.
    /// </summary>
    ObjectId Insert(T entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    bool Update(T entity);

    /// <summary>
    /// Deletes an entity by ID.
    /// </summary>
    bool Delete(ObjectId id);

    /// <summary>
    /// Deletes entities matching the predicate.
    /// </summary>
    int DeleteMany(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Counts entities matching the predicate.
    /// </summary>
    int Count(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    bool Exists(Expression<Func<T, bool>> predicate);
}
