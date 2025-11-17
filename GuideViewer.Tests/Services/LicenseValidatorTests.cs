using FluentAssertions;
using GuideViewer.Core.Models;
using GuideViewer.Core.Services;
using Xunit;

namespace GuideViewer.Tests.Services;

public class LicenseValidatorTests
{
    private readonly LicenseValidator _validator;

    public LicenseValidatorTests()
    {
        _validator = new LicenseValidator();
    }

    [Fact]
    public void ValidateProductKey_WithEmptyKey_ReturnsInvalid()
    {
        // Act
        var result = _validator.ValidateProductKey("");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot be empty");
    }

    [Fact]
    public void ValidateProductKey_WithInvalidFormat_ReturnsInvalid()
    {
        // Act
        var result = _validator.ValidateProductKey("INVALID");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid product key format");
    }

    [Fact]
    public void ValidateProductKey_WithInvalidPrefix_ReturnsInvalid()
    {
        // Arrange
        var invalidKey = "X000-0000-0000-0000"; // X is not a valid prefix

        // Act
        var result = _validator.ValidateProductKey(invalidKey);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Unrecognized role prefix");
    }

    [Fact]
    public void GenerateProductKey_ForAdmin_StartsWithA()
    {
        // Act
        var key = _validator.GenerateProductKey(UserRole.Admin);

        // Assert
        key.Should().StartWith("A");
        key.Should().MatchRegex(@"^A\w{3}-\w{4}-\w{4}-\w{4}$");
    }

    [Fact]
    public void GenerateProductKey_ForTechnician_StartsWithT()
    {
        // Act
        var key = _validator.GenerateProductKey(UserRole.Technician);

        // Assert
        key.Should().StartWith("T");
        key.Should().MatchRegex(@"^T\w{3}-\w{4}-\w{4}-\w{4}$");
    }

    [Fact]
    public void GenerateProductKey_ProducesValidKey()
    {
        // Act
        var adminKey = _validator.GenerateProductKey(UserRole.Admin);
        var adminResult = _validator.ValidateProductKey(adminKey);

        var techKey = _validator.GenerateProductKey(UserRole.Technician);
        var techResult = _validator.ValidateProductKey(techKey);

        // Assert
        adminResult.IsValid.Should().BeTrue();
        adminResult.Role.Should().Be(UserRole.Admin);

        techResult.IsValid.Should().BeTrue();
        techResult.Role.Should().Be(UserRole.Technician);
    }

    [Fact]
    public void ValidateProductKey_WithGeneratedKey_ReturnsValidWithCorrectRole()
    {
        // Arrange
        var generatedKey = _validator.GenerateProductKey(UserRole.Admin);

        // Act
        var result = _validator.ValidateProductKey(generatedKey);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Role.Should().Be(UserRole.Admin);
        result.ProductKey.Should().Be(generatedKey);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ValidateProductKey_WithTamperedChecksum_ReturnsInvalid()
    {
        // Arrange
        var validKey = _validator.GenerateProductKey(UserRole.Admin);
        var tamperedKey = validKey[..^4] + "0000"; // Replace checksum with zeros

        // Act
        var result = _validator.ValidateProductKey(tamperedKey);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Checksum verification failed");
    }

    [Fact]
    public void ValidateProductKey_AcceptsKeyWithoutDashes()
    {
        // Arrange
        var keyWithDashes = _validator.GenerateProductKey(UserRole.Technician);
        var keyWithoutDashes = keyWithDashes.Replace("-", "");

        // Act
        var result = _validator.ValidateProductKey(keyWithoutDashes);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Role.Should().Be(UserRole.Technician);
    }

    [Fact]
    public void ValidateProductKey_IsCaseInsensitive()
    {
        // Arrange
        var upperKey = _validator.GenerateProductKey(UserRole.Admin);
        var lowerKey = upperKey.ToLowerInvariant();

        // Act
        var upperResult = _validator.ValidateProductKey(upperKey);
        var lowerResult = _validator.ValidateProductKey(lowerKey);

        // Assert
        upperResult.IsValid.Should().BeTrue();
        lowerResult.IsValid.Should().BeTrue();
        upperResult.Role.Should().Be(lowerResult.Role);
    }

    [Fact]
    public void GenerateProductKey_ProducesUniqueKeys()
    {
        // Act
        var keys = new HashSet<string>();
        for (int i = 0; i < 100; i++)
        {
            keys.Add(_validator.GenerateProductKey(UserRole.Admin));
            keys.Add(_validator.GenerateProductKey(UserRole.Technician));
        }

        // Assert
        keys.Should().HaveCount(200); // All keys should be unique
    }
}
