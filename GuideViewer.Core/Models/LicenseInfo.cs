namespace GuideViewer.Core.Models;

/// <summary>
/// Contains information about a validated license.
/// </summary>
public class LicenseInfo
{
    /// <summary>
    /// Gets or sets whether the license is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the user role associated with the license.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Gets or sets the product key.
    /// </summary>
    public string ProductKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation error message if the license is invalid.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Creates a valid license info.
    /// </summary>
    public static LicenseInfo CreateValid(string productKey, UserRole role)
    {
        return new LicenseInfo
        {
            IsValid = true,
            ProductKey = productKey,
            Role = role
        };
    }

    /// <summary>
    /// Creates an invalid license info with an error message.
    /// </summary>
    public static LicenseInfo CreateInvalid(string errorMessage)
    {
        return new LicenseInfo
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}
