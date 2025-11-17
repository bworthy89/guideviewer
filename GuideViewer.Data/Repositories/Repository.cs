using System.Linq.Expressions;
using GuideViewer.Data.Services;
using LiteDB;

namespace GuideViewer.Data.Repositories;

/// <summary>
/// Base repository implementation using LiteDB.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DatabaseService _databaseService;
    protected readonly ILiteCollection<T> _collection;

    /// <summary>
    /// Initializes a new instance of the Repository class.
    /// </summary>
    /// <param name="databaseService">The database service.</param>
    /// <param name="collectionName">The collection name.</param>
    public Repository(DatabaseService databaseService, string collectionName)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _collection = _databaseService.GetCollection<T>(collectionName);
    }

    /// <inheritdoc/>
    public virtual T? GetById(ObjectId id)
    {
        return _collection.FindById(id);
    }

    /// <inheritdoc/>
    public virtual IEnumerable<T> GetAll()
    {
        return _collection.FindAll();
    }

    /// <inheritdoc/>
    public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return _collection.Find(predicate);
    }

    /// <inheritdoc/>
    public virtual T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        return _collection.FindOne(predicate);
    }

    /// <inheritdoc/>
    public virtual ObjectId Insert(T entity)
    {
        return _collection.Insert(entity);
    }

    /// <inheritdoc/>
    public virtual bool Update(T entity)
    {
        return _collection.Update(entity);
    }

    /// <inheritdoc/>
    public virtual bool Delete(ObjectId id)
    {
        return _collection.Delete(id);
    }

    /// <inheritdoc/>
    public virtual int DeleteMany(Expression<Func<T, bool>> predicate)
    {
        return _collection.DeleteMany(predicate);
    }

    /// <inheritdoc/>
    public virtual int Count(Expression<Func<T, bool>> predicate)
    {
        return _collection.Count(predicate);
    }

    /// <inheritdoc/>
    public virtual bool Exists(Expression<Func<T, bool>> predicate)
    {
        return _collection.Exists(predicate);
    }
}
