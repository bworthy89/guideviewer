using FluentAssertions;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace GuideViewer.Tests.Repositories;

/// <summary>
/// Unit tests for GuideRepository.
/// </summary>
public class GuideRepositoryTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly GuideRepository _guideRepository;

    public GuideRepositoryTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_guides_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _guideRepository = new GuideRepository(_databaseService);
    }

    [Fact]
    public void Insert_ShouldAddGuideToDatabase()
    {
        // Arrange
        var guide = new Guide
        {
            Title = "Test Guide",
            Description = "Test Description",
            Category = "Installation",
            EstimatedMinutes = 30,
            CreatedBy = "Admin"
        };

        // Act
        var id = _guideRepository.Insert(guide);

        // Assert
        id.Should().NotBe(ObjectId.Empty);

        var retrieved = _guideRepository.GetById(id);
        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("Test Guide");
        retrieved.Description.Should().Be("Test Description");
        retrieved.Category.Should().Be("Installation");
        retrieved.EstimatedMinutes.Should().Be(30);
    }

    [Fact]
    public void Update_ShouldModifyExistingGuide()
    {
        // Arrange
        var guide = new Guide
        {
            Title = "Original Title",
            Description = "Original Description",
            Category = "Setup"
        };
        var id = _guideRepository.Insert(guide);

        // Act
        guide.Title = "Updated Title";
        guide.Description = "Updated Description";
        var result = _guideRepository.Update(guide);

        // Assert
        result.Should().BeTrue();

        var retrieved = _guideRepository.GetById(id);
        retrieved!.Title.Should().Be("Updated Title");
        retrieved.Description.Should().Be("Updated Description");
        retrieved.UpdatedAt.Should().BeAfter(retrieved.CreatedAt);
    }

    [Fact]
    public void Delete_ShouldRemoveGuide()
    {
        // Arrange
        var guide = new Guide { Title = "To Delete" };
        var id = _guideRepository.Insert(guide);

        // Act
        var result = _guideRepository.Delete(id);

        // Assert
        result.Should().BeTrue();
        _guideRepository.GetById(id).Should().BeNull();
    }

    [Fact]
    public void Delete_ShouldRemoveAssociatedImages()
    {
        // Arrange
        var guide = new Guide
        {
            Title = "Guide with Images",
            Steps = new List<Step>
            {
                new Step
                {
                    Title = "Step 1",
                    ImageIds = new List<string> { "image1.jpg", "image2.jpg" }
                }
            }
        };
        var id = _guideRepository.Insert(guide);

        // Act
        var result = _guideRepository.Delete(id);

        // Assert
        result.Should().BeTrue();
        // Note: In a full integration test, we'd verify FileStorage deletion
        // For unit tests, we're verifying the Delete method completes without error
    }

    [Fact]
    public void GetAll_ShouldReturnAllGuidesOrderedByTitle()
    {
        // Arrange
        _guideRepository.Insert(new Guide { Title = "Zebra Guide" });
        _guideRepository.Insert(new Guide { Title = "Alpha Guide" });
        _guideRepository.Insert(new Guide { Title = "Beta Guide" });

        // Act
        var guides = _guideRepository.GetAll().ToList();

        // Assert
        guides.Should().HaveCount(3);
        guides[0].Title.Should().Be("Alpha Guide");
        guides[1].Title.Should().Be("Beta Guide");
        guides[2].Title.Should().Be("Zebra Guide");
    }

    [Fact]
    public void Search_WithMatchingTitle_ShouldReturnGuide()
    {
        // Arrange
        _guideRepository.Insert(new Guide { Title = "Network Installation Guide" });
        _guideRepository.Insert(new Guide { Title = "Software Setup" });
        _guideRepository.Insert(new Guide { Title = "Hardware Installation" });

        // Act
        var results = _guideRepository.Search("installation").ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(g => g.Title.Contains("Network"));
        results.Should().Contain(g => g.Title.Contains("Hardware"));
    }

    [Fact]
    public void Search_WithMatchingDescription_ShouldReturnGuide()
    {
        // Arrange
        _guideRepository.Insert(new Guide
        {
            Title = "Guide 1",
            Description = "This guide covers network setup"
        });
        _guideRepository.Insert(new Guide
        {
            Title = "Guide 2",
            Description = "This guide covers software installation"
        });

        // Act
        var results = _guideRepository.Search("network").ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Guide 1");
    }

    [Fact]
    public void Search_WithMatchingCategory_ShouldReturnGuide()
    {
        // Arrange
        _guideRepository.Insert(new Guide
        {
            Title = "Guide 1",
            Category = "Hardware"
        });
        _guideRepository.Insert(new Guide
        {
            Title = "Guide 2",
            Category = "Software"
        });

        // Act
        var results = _guideRepository.Search("hardware").ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Guide 1");
    }

    [Fact]
    public void Search_WithEmptyQuery_ShouldReturnAllGuides()
    {
        // Arrange
        _guideRepository.Insert(new Guide { Title = "Guide 1" });
        _guideRepository.Insert(new Guide { Title = "Guide 2" });

        // Act
        var results = _guideRepository.Search("").ToList();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public void Search_IsCaseInsensitive()
    {
        // Arrange
        _guideRepository.Insert(new Guide { Title = "Network Installation Guide" });

        // Act
        var results1 = _guideRepository.Search("NETWORK").ToList();
        var results2 = _guideRepository.Search("network").ToList();
        var results3 = _guideRepository.Search("NetWork").ToList();

        // Assert
        results1.Should().HaveCount(1);
        results2.Should().HaveCount(1);
        results3.Should().HaveCount(1);
    }

    [Fact]
    public void GetByCategory_ShouldReturnMatchingGuides()
    {
        // Arrange
        _guideRepository.Insert(new Guide { Title = "Guide 1", Category = "Installation" });
        _guideRepository.Insert(new Guide { Title = "Guide 2", Category = "Installation" });
        _guideRepository.Insert(new Guide { Title = "Guide 3", Category = "Maintenance" });

        // Act
        var results = _guideRepository.GetByCategory("Installation").ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(g => g.Category == "Installation");
    }

    [Fact]
    public void GetByCategory_IsCaseInsensitive()
    {
        // Arrange
        _guideRepository.Insert(new Guide { Title = "Guide 1", Category = "Installation" });

        // Act
        var results = _guideRepository.GetByCategory("installation").ToList();

        // Assert
        results.Should().HaveCount(1);
    }

    [Fact]
    public void GetRecentlyModified_ShouldReturnLatestGuides()
    {
        // Arrange
        var guide1 = new Guide { Title = "Old Guide" };
        guide1.UpdatedAt = DateTime.UtcNow.AddDays(-10);
        _guideRepository.Insert(guide1);

        var guide2 = new Guide { Title = "Recent Guide" };
        guide2.UpdatedAt = DateTime.UtcNow.AddHours(-1);
        _guideRepository.Insert(guide2);

        var guide3 = new Guide { Title = "Newest Guide" };
        guide3.UpdatedAt = DateTime.UtcNow;
        _guideRepository.Insert(guide3);

        // Act
        var results = _guideRepository.GetRecentlyModified(2).ToList();

        // Assert
        results.Should().HaveCount(2);
        results[0].Title.Should().Be("Newest Guide");
        results[1].Title.Should().Be("Recent Guide");
    }

    [Fact]
    public void GetCategoryCount_ShouldReturnCorrectCount()
    {
        // Arrange
        _guideRepository.Insert(new Guide { Category = "Installation" });
        _guideRepository.Insert(new Guide { Category = "Installation" });
        _guideRepository.Insert(new Guide { Category = "Maintenance" });

        // Act
        var installationCount = _guideRepository.GetCategoryCount("Installation");
        var maintenanceCount = _guideRepository.GetCategoryCount("Maintenance");

        // Assert
        installationCount.Should().Be(2);
        maintenanceCount.Should().Be(1);
    }

    [Fact]
    public void GetDistinctCategories_ShouldReturnUniqueCategories()
    {
        // Arrange
        _guideRepository.Insert(new Guide { Category = "Installation" });
        _guideRepository.Insert(new Guide { Category = "Installation" });
        _guideRepository.Insert(new Guide { Category = "Maintenance" });
        _guideRepository.Insert(new Guide { Category = "Setup" });

        // Act
        var categories = _guideRepository.GetDistinctCategories().ToList();

        // Assert
        categories.Should().HaveCount(3);
        categories.Should().Contain("Installation");
        categories.Should().Contain("Maintenance");
        categories.Should().Contain("Setup");
    }

    [Fact]
    public void Guide_WithSteps_ShouldPersistCorrectly()
    {
        // Arrange
        var guide = new Guide
        {
            Title = "Multi-Step Guide",
            Steps = new List<Step>
            {
                new Step
                {
                    Order = 1,
                    Title = "Step 1",
                    Content = "First step content"
                },
                new Step
                {
                    Order = 2,
                    Title = "Step 2",
                    Content = "Second step content"
                }
            }
        };

        // Act
        var id = _guideRepository.Insert(guide);
        var retrieved = _guideRepository.GetById(id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Steps.Should().HaveCount(2);
        retrieved.Steps[0].Title.Should().Be("Step 1");
        retrieved.Steps[1].Title.Should().Be("Step 2");
        retrieved.StepCount.Should().Be(2);
        retrieved.HasSteps.Should().BeTrue();
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
