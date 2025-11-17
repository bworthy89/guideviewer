using GuideViewer.Data.Entities;
using LiteDB;

namespace GuideViewer.Data.Services;

/// <summary>
/// Service for managing the LiteDB database connection and operations.
/// </summary>
public class DatabaseService : IDisposable
{
    private readonly string _databasePath;
    private readonly Lazy<LiteDatabase> _database;
    private bool _disposed;

    /// <summary>
    /// Gets the LiteDB database instance.
    /// </summary>
    public LiteDatabase Database => _database.Value;

    /// <summary>
    /// Initializes a new instance of the DatabaseService.
    /// </summary>
    /// <param name="databasePath">Path to the database file. If null, uses default location.</param>
    public DatabaseService(string? databasePath = null)
    {
        _databasePath = databasePath ?? GetDefaultDatabasePath();
        _database = new Lazy<LiteDatabase>(() => InitializeDatabase());
    }

    /// <summary>
    /// Gets the default database path in the user's local app data folder.
    /// </summary>
    private static string GetDefaultDatabasePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "GuideViewer");

        // Create directory if it doesn't exist
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        return Path.Combine(appFolder, "data.db");
    }

    /// <summary>
    /// Initializes the LiteDB database with proper configuration.
    /// </summary>
    private LiteDatabase InitializeDatabase()
    {
        var connectionString = new ConnectionString
        {
            Filename = _databasePath,
            Connection = ConnectionType.Shared,
            Upgrade = true
        };

        var db = new LiteDatabase(connectionString);

        // Ensure collections exist and create indexes
        InitializeCollections(db);

        return db;
    }

    /// <summary>
    /// Initializes database collections and indexes.
    /// </summary>
    private void InitializeCollections(LiteDatabase db)
    {
        // Register entity types with BsonMapper first
        db.Mapper.Entity<User>();
        db.Mapper.Entity<AppSetting>();
        db.Mapper.Entity<Guide>();
        db.Mapper.Entity<Category>();
        db.Mapper.Entity<Step>();
        db.Mapper.Entity<Progress>();

        // Users collection
        var users = db.GetCollection<User>("users");
        users.EnsureIndex(x => x.Role);

        // Settings collection
        var settings = db.GetCollection<AppSetting>("settings");
        settings.EnsureIndex(x => x.Key, unique: true);

        // Guides collection - indexes for search and filtering
        var guides = db.GetCollection<Guide>("guides");
        guides.EnsureIndex(x => x.Title);
        guides.EnsureIndex(x => x.Category);
        guides.EnsureIndex(x => x.UpdatedAt);

        // Categories collection - index on Name for fast lookup
        var categories = db.GetCollection<Category>("categories");
        categories.EnsureIndex(x => x.Name, unique: true);

        // Progress collection - indexes for user lookup and filtering
        var progress = db.GetCollection<Progress>("progress");
        // Composite index for primary lookup (ensures one progress per user+guide)
        progress.EnsureIndex(x => new { x.UserId, x.GuideId }, unique: true);
        // Individual indexes for filtering and sorting
        progress.EnsureIndex(x => x.UserId);
        progress.EnsureIndex(x => x.GuideId);
        progress.EnsureIndex(x => x.CompletedAt);
        progress.EnsureIndex(x => x.LastAccessedAt);
    }

    /// <summary>
    /// Gets a collection from the database.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="name">The collection name.</param>
    public ILiteCollection<T> GetCollection<T>(string name)
    {
        return Database.GetCollection<T>(name);
    }

    /// <summary>
    /// Performs a checkpoint operation to save all changes to disk.
    /// </summary>
    public void Checkpoint()
    {
        if (_database.IsValueCreated)
        {
            Database.Checkpoint();
        }
    }

    /// <summary>
    /// Creates a backup of the database.
    /// </summary>
    /// <param name="backupPath">Path where the backup should be saved. If null, uses default backup location.</param>
    public void Backup(string? backupPath = null)
    {
        var targetPath = backupPath ?? GetDefaultBackupPath();
        var targetDirectory = Path.GetDirectoryName(targetPath);

        if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        if (_database.IsValueCreated)
        {
            Database.Checkpoint();
        }

        File.Copy(_databasePath, targetPath, overwrite: true);
    }

    /// <summary>
    /// Gets the default backup path.
    /// </summary>
    private static string GetDefaultBackupPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var backupFolder = Path.Combine(appDataPath, "GuideViewer", "backups");
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return Path.Combine(backupFolder, $"data_backup_{timestamp}.db");
    }

    /// <summary>
    /// Disposes the database connection.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the database connection.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_database.IsValueCreated)
                {
                    _database.Value?.Dispose();
                }
            }
            _disposed = true;
        }
    }
}
