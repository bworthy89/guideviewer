using System.Security.Cryptography;
using System.Text;
using GuideViewer.Core.Models;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for validating product keys using HMAC-SHA256.
/// </summary>
public class LicenseValidator
{
    private const string SecretSalt = "GuideViewer-2025-SecretSalt-ChangeInProduction";
    private const string AdminPrefix = "A";
    private const string TechPrefix = "T";

    /// <summary>
    /// Validates a product key and returns license information.
    /// </summary>
    /// <param name="productKey">The product key to validate (format: XXXX-XXXX-XXXX-XXXX).</param>
    /// <returns>License information with validation result.</returns>
    public LicenseInfo ValidateProductKey(string productKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
        {
            return LicenseInfo.CreateInvalid("Product key cannot be empty.");
        }

        // Remove any whitespace and convert to uppercase
        var cleanKey = productKey.Replace(" ", "").Replace("-", "").ToUpperInvariant();

        // Validate format: should be 16 characters total
        if (cleanKey.Length != 16)
        {
            return LicenseInfo.CreateInvalid("Invalid product key format. Expected format: XXXX-XXXX-XXXX-XXXX");
        }

        // Extract role from first character
        var roleChar = cleanKey[0].ToString();
        UserRole role;

        if (roleChar == AdminPrefix)
        {
            role = UserRole.Admin;
        }
        else if (roleChar == TechPrefix)
        {
            role = UserRole.Technician;
        }
        else
        {
            return LicenseInfo.CreateInvalid("Invalid product key. Unrecognized role prefix.");
        }

        // Extract payload (characters 0-11) and checksum (characters 12-15)
        var payload = cleanKey[..12];
        var providedChecksum = cleanKey[12..];

        // Calculate expected checksum
        var calculatedChecksum = CalculateChecksum(payload);

        // Verify checksum
        if (!calculatedChecksum.Equals(providedChecksum, StringComparison.OrdinalIgnoreCase))
        {
            return LicenseInfo.CreateInvalid("Invalid product key. Checksum verification failed.");
        }

        // Format the product key back to standard format
        var formattedKey = FormatProductKey(cleanKey);

        return LicenseInfo.CreateValid(formattedKey, role);
    }

    /// <summary>
    /// Generates a product key for a specific role.
    /// </summary>
    /// <param name="role">The user role.</param>
    /// <returns>A formatted product key.</returns>
    public string GenerateProductKey(UserRole role)
    {
        var random = new Random();
        var prefix = role == UserRole.Admin ? AdminPrefix : TechPrefix;

        // Generate random 11 characters (excluding the first character which is the role prefix)
        var randomChars = new char[11];
        const string validChars = "0123456789ABCDEF";

        for (int i = 0; i < 11; i++)
        {
            randomChars[i] = validChars[random.Next(validChars.Length)];
        }

        var payload = prefix + new string(randomChars);

        // Calculate checksum
        var checksum = CalculateChecksum(payload);

        // Combine payload and checksum
        var fullKey = payload + checksum;

        return FormatProductKey(fullKey);
    }

    /// <summary>
    /// Calculates a 4-character checksum using HMAC-SHA256.
    /// </summary>
    private string CalculateChecksum(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretSalt));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

        // Take first 2 bytes and convert to hex (4 characters)
        return BitConverter.ToString(hashBytes, 0, 2).Replace("-", "");
    }

    /// <summary>
    /// Formats a product key with dashes (XXXX-XXXX-XXXX-XXXX).
    /// </summary>
    private string FormatProductKey(string key)
    {
        if (key.Length != 16)
        {
            throw new ArgumentException("Key must be 16 characters long.", nameof(key));
        }

        return $"{key.Substring(0, 4)}-{key.Substring(4, 4)}-{key.Substring(8, 4)}-{key.Substring(12, 4)}";
    }
}
