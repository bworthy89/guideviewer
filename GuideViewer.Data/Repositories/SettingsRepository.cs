using GuideViewer.Data.Entities;
using GuideViewer.Data.Services;

namespace GuideViewer.Data.Repositories;

/// <summary>
/// Repository for managing application settings.
/// </summary>
public class SettingsRepository : Repository<AppSetting>
{
    public SettingsRepository(DatabaseService databaseService)
        : base(databaseService, "settings")
    {
    }

    /// <summary>
    /// Gets a setting value by key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <returns>The setting value, or null if not found.</returns>
    public string? GetValue(string key)
    {
        var setting = FirstOrDefault(s => s.Key == key);
        return setting?.Value;
    }

    /// <summary>
    /// Sets a setting value by key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The setting value.</param>
    public void SetValue(string key, string value)
    {
        var setting = FirstOrDefault(s => s.Key == key);

        if (setting != null)
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
            Update(setting);
        }
        else
        {
            Insert(new AppSetting
            {
                Key = key,
                Value = value,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Deletes a setting by key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    public bool DeleteByKey(string key)
    {
        return DeleteMany(s => s.Key == key) > 0;
    }
}
