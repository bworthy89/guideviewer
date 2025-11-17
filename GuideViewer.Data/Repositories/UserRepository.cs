using GuideViewer.Data.Entities;
using GuideViewer.Data.Services;

namespace GuideViewer.Data.Repositories;

/// <summary>
/// Repository for managing User entities.
/// </summary>
public class UserRepository : Repository<User>
{
    public UserRepository(DatabaseService databaseService)
        : base(databaseService, "users")
    {
    }

    /// <summary>
    /// Gets the current user (assumes single user per installation).
    /// </summary>
    public User? GetCurrentUser()
    {
        return _collection.FindAll().FirstOrDefault();
    }

    /// <summary>
    /// Updates the last login time for a user.
    /// </summary>
    public void UpdateLastLogin(User user)
    {
        user.LastLogin = DateTime.UtcNow;
        Update(user);
    }
}
