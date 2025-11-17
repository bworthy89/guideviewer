using FluentAssertions;
using GuideViewer.Core.Services;
using GuideViewer.Data.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GuideViewer.Tests.Services;

/// <summary>
/// Unit tests for ImageStorageService.
/// </summary>
public class ImageStorageServiceTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly ImageStorageService _imageStorageService;

    public ImageStorageServiceTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_images_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _imageStorageService = new ImageStorageService(_databaseService);
    }

    [Fact]
    public async Task UploadImageAsync_WithValidImage_ShouldReturnFileId()
    {
        // Arrange
        var imageStream = CreateTestImageStream(1024); // 1KB
        var fileName = "test-image.png";

        // Act
        var fileId = await _imageStorageService.UploadImageAsync(imageStream, fileName);

        // Assert
        fileId.Should().NotBeNullOrEmpty();
        fileId.Should().StartWith("img_");
    }

    [Fact]
    public async Task UploadImageAsync_WithNullStream_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _imageStorageService.UploadImageAsync(null!, "test.png"));
    }

    [Fact]
    public async Task UploadImageAsync_WithEmptyFileName_ShouldThrowException()
    {
        // Arrange
        var imageStream = CreateTestImageStream(1024);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _imageStorageService.UploadImageAsync(imageStream, ""));
    }

    [Fact]
    public async Task UploadImageAsync_WithInvalidExtension_ShouldThrowException()
    {
        // Arrange
        var imageStream = CreateTestImageStream(1024);
        var fileName = "test-image.gif"; // GIF not supported

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _imageStorageService.UploadImageAsync(imageStream, fileName));

        exception.Message.Should().Contain("Invalid file format");
    }

    [Fact]
    public async Task UploadImageAsync_WithOversizedImage_ShouldThrowException()
    {
        // Arrange
        var imageStream = CreateTestImageStream(11 * 1024 * 1024); // 11MB (exceeds 10MB limit)
        var fileName = "large-image.png";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _imageStorageService.UploadImageAsync(imageStream, fileName));

        exception.Message.Should().Contain("exceeds maximum allowed size");
    }

    [Fact]
    public async Task GetImageAsync_WithExistingImage_ShouldReturnStream()
    {
        // Arrange
        var originalData = Encoding.UTF8.GetBytes("Test image data");
        var uploadStream = new MemoryStream(originalData);
        var fileId = await _imageStorageService.UploadImageAsync(uploadStream, "test.png");

        // Act
        var retrievedStream = await _imageStorageService.GetImageAsync(fileId);

        // Assert
        retrievedStream.Should().NotBeNull();
        using (var reader = new StreamReader(retrievedStream!))
        {
            var retrievedData = reader.ReadToEnd();
            retrievedData.Should().Be("Test image data");
        }
    }

    [Fact]
    public async Task GetImageAsync_WithNonExistentImage_ShouldReturnNull()
    {
        // Act
        var stream = await _imageStorageService.GetImageAsync("nonexistent_id");

        // Assert
        stream.Should().BeNull();
    }

    [Fact]
    public async Task GetImageAsync_WithEmptyFileId_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _imageStorageService.GetImageAsync(""));
    }

    [Fact]
    public async Task DeleteImageAsync_WithExistingImage_ShouldReturnTrue()
    {
        // Arrange
        var imageStream = CreateTestImageStream(1024);
        var fileId = await _imageStorageService.UploadImageAsync(imageStream, "test.png");

        // Act
        var deleted = await _imageStorageService.DeleteImageAsync(fileId);

        // Assert
        deleted.Should().BeTrue();

        // Verify deletion
        var retrievedStream = await _imageStorageService.GetImageAsync(fileId);
        retrievedStream.Should().BeNull();
    }

    [Fact]
    public async Task DeleteImageAsync_WithNonExistentImage_ShouldReturnFalse()
    {
        // Act
        var deleted = await _imageStorageService.DeleteImageAsync("nonexistent_id");

        // Assert
        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteImageAsync_WithEmptyFileId_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _imageStorageService.DeleteImageAsync(""));
    }

    [Fact]
    public async Task GetImageMetadataAsync_WithExistingImage_ShouldReturnMetadata()
    {
        // Arrange
        var imageStream = CreateTestImageStream(2048);
        var fileName = "test-image.jpg";
        var fileId = await _imageStorageService.UploadImageAsync(imageStream, fileName);

        // Act
        var metadata = await _imageStorageService.GetImageMetadataAsync(fileId);

        // Assert
        metadata.Should().NotBeNull();
        metadata!.FileId.Should().Be(fileId);
        metadata.FileName.Should().Be(fileName);
        metadata.SizeInBytes.Should().Be(2048);
        metadata.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromHours(24)); // Allow for timezone differences
    }

    [Fact]
    public async Task GetImageMetadataAsync_WithNonExistentImage_ShouldReturnNull()
    {
        // Act
        var metadata = await _imageStorageService.GetImageMetadataAsync("nonexistent_id");

        // Assert
        metadata.Should().BeNull();
    }

    [Fact]
    public async Task ValidateImageAsync_WithValidPngImage_ShouldReturnSuccess()
    {
        // Arrange
        var imageStream = CreateTestImageStream(1024);
        var fileName = "test.png";

        // Act
        var result = await _imageStorageService.ValidateImageAsync(imageStream, fileName);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateImageAsync_WithValidJpgImage_ShouldReturnSuccess()
    {
        // Arrange
        var imageStream = CreateTestImageStream(1024);
        var fileName = "test.jpg";

        // Act
        var result = await _imageStorageService.ValidateImageAsync(imageStream, fileName);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateImageAsync_WithValidJpegImage_ShouldReturnSuccess()
    {
        // Arrange
        var imageStream = CreateTestImageStream(1024);
        var fileName = "test.jpeg";

        // Act
        var result = await _imageStorageService.ValidateImageAsync(imageStream, fileName);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateImageAsync_WithValidBmpImage_ShouldReturnSuccess()
    {
        // Arrange
        var imageStream = CreateTestImageStream(1024);
        var fileName = "test.bmp";

        // Act
        var result = await _imageStorageService.ValidateImageAsync(imageStream, fileName);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateImageAsync_WithInvalidExtension_ShouldReturnFailure()
    {
        // Arrange
        var imageStream = CreateTestImageStream(1024);
        var fileName = "test.gif";

        // Act
        var result = await _imageStorageService.ValidateImageAsync(imageStream, fileName);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid file format");
    }

    [Fact]
    public async Task ValidateImageAsync_WithOversizedImage_ShouldReturnFailure()
    {
        // Arrange
        var imageStream = CreateTestImageStream(11 * 1024 * 1024); // 11MB
        var fileName = "large.png";

        // Act
        var result = await _imageStorageService.ValidateImageAsync(imageStream, fileName);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("exceeds maximum allowed size");
    }

    [Fact]
    public async Task ValidateImageAsync_WithNullStream_ShouldReturnFailure()
    {
        // Act
        var result = await _imageStorageService.ValidateImageAsync(null!, "test.png");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("null");
    }

    [Fact]
    public async Task ValidateImageAsync_WithEmptyFileName_ShouldReturnFailure()
    {
        // Arrange
        var imageStream = CreateTestImageStream(1024);

        // Act
        var result = await _imageStorageService.ValidateImageAsync(imageStream, "");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("empty");
    }

    [Fact]
    public async Task MultipleImages_CanBeStoredAndRetrieved()
    {
        // Arrange
        var image1 = CreateTestImageStream(1024);
        var image2 = CreateTestImageStream(2048);
        var image3 = CreateTestImageStream(512);

        // Act
        var fileId1 = await _imageStorageService.UploadImageAsync(image1, "image1.png");
        var fileId2 = await _imageStorageService.UploadImageAsync(image2, "image2.jpg");
        var fileId3 = await _imageStorageService.UploadImageAsync(image3, "image3.bmp");

        // Assert
        fileId1.Should().NotBe(fileId2);
        fileId2.Should().NotBe(fileId3);
        fileId1.Should().NotBe(fileId3);

        var retrieved1 = await _imageStorageService.GetImageAsync(fileId1);
        var retrieved2 = await _imageStorageService.GetImageAsync(fileId2);
        var retrieved3 = await _imageStorageService.GetImageAsync(fileId3);

        retrieved1.Should().NotBeNull();
        retrieved2.Should().NotBeNull();
        retrieved3.Should().NotBeNull();
    }

    private static MemoryStream CreateTestImageStream(int sizeInBytes)
    {
        var data = new byte[sizeInBytes];
        new Random().NextBytes(data);
        return new MemoryStream(data);
    }

    public void Dispose()
    {
        _databaseService?.Dispose();
        if (File.Exists(_testDatabasePath))
        {
            File.Delete(_testDatabasePath);
        }
    }
}
