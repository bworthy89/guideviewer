using LiteDB;

namespace GuideViewer.Data.Entities;

/// <summary>
/// Represents an application setting stored in the database.
/// </summary>
public class AppSetting
{
    /// <summary>
    /// Unique identifier for the setting.
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    /// <summary>
    /// The setting key (e.g., "theme", "windowSize").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The setting value as a JSON string.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// When the setting was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
