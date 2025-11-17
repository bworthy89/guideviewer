using LiteDB;

namespace GuideViewer.Data.Entities;

/// <summary>
/// Represents a user in the system with their license and role information.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    /// <summary>
    /// The encrypted product key used for activation.
    /// </summary>
    public string ProductKey { get; set; } = string.Empty;

    /// <summary>
    /// The user's role (ADMIN or TECHNICIAN).
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// When the license was activated.
    /// </summary>
    public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last time the user logged in.
    /// </summary>
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
}
