using FluentAssertions;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GuideViewer.Tests.Integration;

/// <summary>
/// Integration tests for the complete guide workflow including CRUD operations,
/// image storage, and cross-repository interactions.
/// </summary>
public class GuideWorkflowIntegrationTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly GuideRepository _guideRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly IImageStorageService _imageStorageService;

    public GuideWorkflowIntegrationTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_integration_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _guideRepository = new GuideRepository(_databaseService);
        _categoryRepository = new CategoryRepository(_databaseService);
        _imageStorageService = new ImageStorageService(_databaseService);
    }

    [Fact]
    public async Task CompleteGuideWorkflow_CreateEditViewDelete_ShouldWorkEndToEnd()
    {
        // Arrange - Create category first
        var category = new Category
        {
            Name = "Network Installation",
            Description = "Network setup guides",
            IconGlyph = "\uE968",
            Color = "#0078D4",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var categoryId = _categoryRepository.Insert(category);
        categoryId.Should().NotBe(ObjectId.Empty);

        // Act 1: Create guide with steps
        var guide = new Guide
        {
            Title = "Configure Network Switch",
            Description = "Step-by-step guide to configure a network switch",
            Category = category.Name,
            EstimatedMinutes = 45,
            CreatedBy = "Admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Steps = new List<Step>
            {
                new Step
                {
                    Order = 1,
                    Title = "Connect to switch console",
                    Content = "{\\rtf1 Connect serial cable to console port}",
                    CreatedAt = DateTime.UtcNow
                },
                new Step
                {
                    Order = 2,
                    Title = "Configure basic settings",
                    Content = "{\\rtf1 Set hostname and management IP}",
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        var guideId = _guideRepository.Insert(guide);
        guideId.Should().NotBe(ObjectId.Empty);

        // Assert 1: Verify guide was created
        var retrievedGuide = _guideRepository.GetById(guideId);
        retrievedGuide.Should().NotBeNull();
        retrievedGuide!.Title.Should().Be("Configure Network Switch");
        retrievedGuide.Steps.Should().HaveCount(2);
        retrievedGuide.Steps[0].Order.Should().Be(1);
        retrievedGuide.Steps[1].Order.Should().Be(2);

        // Act 2: Add image to first step
        var testImageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG header
        string imageFileId;
        using (var imageStream = new MemoryStream(testImageBytes))
        {
            imageFileId = await _imageStorageService.UploadImageAsync(imageStream, "test_image.png");
            imageFileId.Should().NotBeNullOrEmpty();
            retrievedGuide.Steps[0].ImageIds.Add(imageFileId);
            _guideRepository.Update(retrievedGuide);
        }

        // Assert 2: Verify image was added
        var guideWithImage = _guideRepository.GetById(guideId);
        guideWithImage!.Steps[0].ImageIds.Should().ContainSingle();
        guideWithImage!.Steps[0].ImageIds[0].Should().Be(imageFileId);

        // Act 3: Search for the guide
        var searchResults = _guideRepository.Search("network switch");
        searchResults.Should().ContainSingle();
        searchResults.First().Id.Should().Be(guideId);

        // Act 4: Filter by category
        var categoryResults = _guideRepository.GetByCategory(category.Name);
        categoryResults.Should().ContainSingle();
        categoryResults.First().Id.Should().Be(guideId);

        // Act 5: Update guide (add third step)
        guideWithImage.Steps.Add(new Step
        {
            Order = 3,
            Title = "Save configuration",
            Content = "{\\rtf1 Write configuration to NVRAM}",
            CreatedAt = DateTime.UtcNow
        });
        guideWithImage.UpdatedAt = DateTime.UtcNow;
        var updateResult = _guideRepository.Update(guideWithImage);
        updateResult.Should().BeTrue();

        // Assert 5: Verify update
        var updatedGuide = _guideRepository.GetById(guideId);
        updatedGuide!.Steps.Should().HaveCount(3);
        updatedGuide.Steps[2].Title.Should().Be("Save configuration");

        // Act 6: Delete guide
        var deleteResult = _guideRepository.Delete(guideId);
        deleteResult.Should().BeTrue();

        // Assert 6: Verify deletion
        var deletedGuide = _guideRepository.GetById(guideId);
        deletedGuide.Should().BeNull();

        // Assert 7: Verify image was deleted with guide
        var deletedImageStream = await _imageStorageService.GetImageAsync(imageFileId);
        deletedImageStream.Should().BeNull();
    }

    [Fact]
    public void MultipleGuides_WithSameCategory_ShouldFilterCorrectly()
    {
        // Arrange - Create category
        var category = _categoryRepository.EnsureCategory("Server Setup");

        // Create multiple guides in same category
        var guide1 = new Guide
        {
            Title = "Install Windows Server",
            Description = "Windows Server installation guide",
            Category = "Server Setup",
            EstimatedMinutes = 60,
            CreatedBy = "Admin"
        };

        var guide2 = new Guide
        {
            Title = "Configure Active Directory",
            Description = "AD DS setup guide",
            Category = "Server Setup",
            EstimatedMinutes = 45,
            CreatedBy = "Admin"
        };

        var guide3 = new Guide
        {
            Title = "Install Network Driver",
            Description = "Driver installation guide",
            Category = "Hardware Maintenance",
            EstimatedMinutes = 15,
            CreatedBy = "Admin"
        };

        // Act
        var id1 = _guideRepository.Insert(guide1);
        var id2 = _guideRepository.Insert(guide2);
        var id3 = _guideRepository.Insert(guide3);

        // Assert - Filter by category
        var serverGuides = _guideRepository.GetByCategory("Server Setup");
        serverGuides.Should().HaveCount(2);
        serverGuides.Select(g => g.Id).Should().Contain(new[] { id1, id2 });

        // Assert - Search across guides
        var searchResults = _guideRepository.Search("server");
        searchResults.Should().HaveCount(2); // Windows Server + AD DS
    }

    [Fact]
    public void CategoryDeletion_WithExistingGuides_ShouldBePreventable()
    {
        // Arrange
        var category = new Category
        {
            Name = "Software Deployment",
            Description = "Software installation guides",
            IconGlyph = "\uECAA",
            Color = "#107C10"
        };
        var categoryId = _categoryRepository.Insert(category);

        var guide = new Guide
        {
            Title = "Deploy Office 365",
            Description = "Office 365 deployment guide",
            Category = category.Name,
            EstimatedMinutes = 30,
            CreatedBy = "Admin"
        };
        _guideRepository.Insert(guide);

        // Act - Check if category is used
        var guidesInCategory = _guideRepository.GetByCategory(category.Name).ToList();

        // Assert - Category should not be deletable
        guidesInCategory.Should().HaveCount(1);

        // Business logic: Category deletion should check this count
        // and prevent deletion if > 0
        var canDelete = guidesInCategory.Count == 0;
        canDelete.Should().BeFalse();
    }

    [Fact]
    public async Task GuideWithMultipleImages_ShouldHandleAllImages()
    {
        // Arrange
        var guide = new Guide
        {
            Title = "Hardware Installation",
            Description = "Install server hardware",
            Category = "Hardware Maintenance",
            EstimatedMinutes = 120,
            CreatedBy = "Admin",
            Steps = new List<Step>
            {
                new Step { Order = 1, Title = "Step 1", Content = "{\\rtf1 Content}" },
                new Step { Order = 2, Title = "Step 2", Content = "{\\rtf1 Content}" },
                new Step { Order = 3, Title = "Step 3", Content = "{\\rtf1 Content}" }
            }
        };

        var guideId = _guideRepository.Insert(guide);
        var retrievedGuide = _guideRepository.GetById(guideId)!;

        // Act - Add images to all steps
        var imageIds = new List<string>();
        for (int i = 0; i < retrievedGuide.Steps.Count; i++)
        {
            var testImageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            using var imageStream = new MemoryStream(testImageBytes);
            var uploadedImageId = await _imageStorageService.UploadImageAsync(imageStream, $"image_{i}.png");
            uploadedImageId.Should().NotBeNullOrEmpty();
            retrievedGuide.Steps[i].ImageIds.Add(uploadedImageId);
            imageIds.Add(uploadedImageId);
        }

        _guideRepository.Update(retrievedGuide);

        // Assert - All images should be stored
        var guideWithImages = _guideRepository.GetById(guideId)!;
        guideWithImages.Steps.Should().AllSatisfy(step =>
            step.ImageIds.Should().ContainSingle()
        );

        // Assert - All images should be retrievable
        foreach (var imageId in imageIds)
        {
            var imageStream = await _imageStorageService.GetImageAsync(imageId);
            imageStream.Should().NotBeNull();
            imageStream?.Dispose();
        }

        // Act - Delete guide
        _guideRepository.Delete(guideId);

        // Assert - All images should be deleted
        foreach (var imageId in imageIds)
        {
            var imageStream = await _imageStorageService.GetImageAsync(imageId);
            imageStream.Should().BeNull();
        }
    }

    [Fact]
    public void GetRecentlyModified_ShouldReturnInCorrectOrder()
    {
        // Arrange - Create guides with different update times
        var guide1 = new Guide
        {
            Title = "Old Guide",
            Description = "Created first",
            Category = "Installation",
            EstimatedMinutes = 30,
            CreatedBy = "Admin",
            CreatedAt = DateTime.UtcNow.AddHours(-3),
            UpdatedAt = DateTime.UtcNow.AddHours(-3)
        };

        var guide2 = new Guide
        {
            Title = "Recent Guide",
            Description = "Created last",
            Category = "Installation",
            EstimatedMinutes = 30,
            CreatedBy = "Admin",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var guide3 = new Guide
        {
            Title = "Middle Guide",
            Description = "Created in between",
            Category = "Installation",
            EstimatedMinutes = 30,
            CreatedBy = "Admin",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            UpdatedAt = DateTime.UtcNow.AddHours(-2)
        };

        // Act
        var id1 = _guideRepository.Insert(guide1);
        var id2 = _guideRepository.Insert(guide2);
        var id3 = _guideRepository.Insert(guide3);

        var recentGuides = _guideRepository.GetRecentlyModified(3).ToList();

        // Assert - Should be ordered by UpdatedAt descending
        recentGuides.Should().HaveCount(3);
        recentGuides[0].Id.Should().Be(id2); // Most recent
        recentGuides[1].Id.Should().Be(id3); // Middle
        recentGuides[2].Id.Should().Be(id1); // Oldest
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
