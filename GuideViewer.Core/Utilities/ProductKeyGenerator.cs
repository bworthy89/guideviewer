using GuideViewer.Core.Models;
using GuideViewer.Core.Services;

namespace GuideViewer.Core.Utilities;

/// <summary>
/// Utility class for generating product keys.
/// </summary>
public static class ProductKeyGenerator
{
    /// <summary>
    /// Generates multiple product keys for testing purposes.
    /// </summary>
    /// <param name="adminCount">Number of admin keys to generate.</param>
    /// <param name="techCount">Number of technician keys to generate.</param>
    /// <returns>A list of generated product keys with their roles.</returns>
    public static List<(string ProductKey, UserRole Role)> GenerateKeys(int adminCount, int techCount)
    {
        var validator = new LicenseValidator();
        var keys = new List<(string, UserRole)>();

        for (int i = 0; i < adminCount; i++)
        {
            var key = validator.GenerateProductKey(UserRole.Admin);
            keys.Add((key, UserRole.Admin));
        }

        for (int i = 0; i < techCount; i++)
        {
            var key = validator.GenerateProductKey(UserRole.Technician);
            keys.Add((key, UserRole.Technician));
        }

        return keys;
    }

    /// <summary>
    /// Prints product keys to console for easy copying.
    /// </summary>
    public static void PrintKeys(int adminCount = 5, int techCount = 5)
    {
        var keys = GenerateKeys(adminCount, techCount);

        Console.WriteLine("=== GuideViewer Product Keys ===");
        Console.WriteLine();
        Console.WriteLine("ADMIN KEYS:");
        foreach (var (key, role) in keys.Where(k => k.Role == UserRole.Admin))
        {
            Console.WriteLine($"  {key}");
        }

        Console.WriteLine();
        Console.WriteLine("TECHNICIAN KEYS:");
        foreach (var (key, role) in keys.Where(k => k.Role == UserRole.Technician))
        {
            Console.WriteLine($"  {key}");
        }

        Console.WriteLine();
        Console.WriteLine("=== End of Keys ===");
    }
}
