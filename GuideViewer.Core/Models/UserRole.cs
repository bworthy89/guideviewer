namespace GuideViewer.Core.Models;

/// <summary>
/// Represents the user role in the application.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Administrator role with full access to create and edit guides.
    /// </summary>
    Admin,

    /// <summary>
    /// Technician role with read-only access to guides and progress tracking.
    /// </summary>
    Technician
}
