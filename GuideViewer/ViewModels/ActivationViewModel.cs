using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using Serilog;
using System.Text.RegularExpressions;

namespace GuideViewer.ViewModels;

/// <summary>
/// ViewModel for the activation window where users enter their product key.
/// </summary>
public partial class ActivationViewModel : ObservableObject
{
    private readonly LicenseValidator _licenseValidator;
    private readonly UserRepository _userRepository;

    [ObservableProperty]
    private string segment1 = string.Empty;

    [ObservableProperty]
    private string segment2 = string.Empty;

    [ObservableProperty]
    private string segment3 = string.Empty;

    [ObservableProperty]
    private string segment4 = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool hasError = false;

    /// <summary>
    /// Gets the full product key from all segments.
    /// </summary>
    public string FullProductKey => $"{Segment1}-{Segment2}-{Segment3}-{Segment4}".ToUpperInvariant();

    public ActivationViewModel(LicenseValidator licenseValidator, UserRepository userRepository)
    {
        _licenseValidator = licenseValidator;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Validates and activates the product key.
    /// </summary>
    [RelayCommand]
    private async Task ActivateAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            // Validate that all segments are filled
            if (string.IsNullOrWhiteSpace(Segment1) ||
                string.IsNullOrWhiteSpace(Segment2) ||
                string.IsNullOrWhiteSpace(Segment3) ||
                string.IsNullOrWhiteSpace(Segment4))
            {
                ShowError("Please enter all four segments of the product key.");
                return;
            }

            // Validate each segment format (4 alphanumeric characters)
            if (!IsValidSegment(Segment1) || !IsValidSegment(Segment2) ||
                !IsValidSegment(Segment3) || !IsValidSegment(Segment4))
            {
                ShowError("Each segment must contain exactly 4 alphanumeric characters.");
                return;
            }

            var productKey = FullProductKey;
            Log.Information("Attempting to validate product key: {ProductKey}", productKey);

            // Validate the product key
            var licenseInfo = _licenseValidator.ValidateProductKey(productKey);

            if (!licenseInfo.IsValid)
            {
                ShowError(licenseInfo.ErrorMessage ?? "Invalid product key. Please check and try again.");
                Log.Warning("Product key validation failed: {Error}", licenseInfo.ErrorMessage);
                return;
            }

            // Save user with role to database
            await Task.Run(() =>
            {
                var user = new User
                {
                    ProductKey = productKey,
                    Role = licenseInfo.Role.ToString(),
                    ActivatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow
                };

                _userRepository.Insert(user);
                Log.Information("User activated successfully with role: {Role}", licenseInfo.Role);
            });

            // Signal successful activation (will be handled by the window)
            ActivationSucceeded?.Invoke(this, licenseInfo.Role);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during product key activation");
            ShowError("An unexpected error occurred. Please try again.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Handles pasting a full product key and splits it into segments.
    /// </summary>
    /// <param name="pastedText">The pasted text.</param>
    public void HandlePaste(string pastedText)
    {
        if (string.IsNullOrWhiteSpace(pastedText))
            return;

        // Remove any whitespace
        var cleaned = pastedText.Trim().ToUpperInvariant();

        // Check if it matches the format XXXX-XXXX-XXXX-XXXX
        var match = Regex.Match(cleaned, @"^([A-Z0-9]{4})-([A-Z0-9]{4})-([A-Z0-9]{4})-([A-Z0-9]{4})$");

        if (match.Success)
        {
            Segment1 = match.Groups[1].Value;
            Segment2 = match.Groups[2].Value;
            Segment3 = match.Groups[3].Value;
            Segment4 = match.Groups[4].Value;

            Log.Information("Product key auto-filled from paste");
        }
        else
        {
            // Try without hyphens
            var matchNoHyphens = Regex.Match(cleaned, @"^([A-Z0-9]{4})([A-Z0-9]{4})([A-Z0-9]{4})([A-Z0-9]{4})$");

            if (matchNoHyphens.Success)
            {
                Segment1 = matchNoHyphens.Groups[1].Value;
                Segment2 = matchNoHyphens.Groups[2].Value;
                Segment3 = matchNoHyphens.Groups[3].Value;
                Segment4 = matchNoHyphens.Groups[4].Value;

                Log.Information("Product key auto-filled from paste (no hyphens)");
            }
        }
    }

    /// <summary>
    /// Validates that a segment contains exactly 4 alphanumeric characters.
    /// </summary>
    private bool IsValidSegment(string segment)
    {
        return !string.IsNullOrWhiteSpace(segment) &&
               segment.Length == 4 &&
               Regex.IsMatch(segment, @"^[A-Z0-9]{4}$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Shows an error message to the user.
    /// </summary>
    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    /// <summary>
    /// Clears the error message.
    /// </summary>
    public void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    /// <summary>
    /// Event raised when activation succeeds.
    /// </summary>
    public event EventHandler<GuideViewer.Core.Models.UserRole>? ActivationSucceeded;
}
