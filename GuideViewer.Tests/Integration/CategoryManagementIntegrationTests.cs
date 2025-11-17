using FluentAssertions;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GuideViewer.Tests.Integration;

/// <summary>
/// Integration tests for category management including CRUD operations,
/// uniqueness validation, and guide associations.
/// </summary>
public class CategoryManagementIntegrationTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly CategoryRepository _categoryRepository;
    private readonly GuideRepository _guideRepository;

    public CategoryManagementIntegrationTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_category_integration_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _categoryRepository = new CategoryRepository(_databaseService);
        _guideRepository = new GuideRepository(_databaseService);
    }

    [Fact]
    public void CompleteCategoryWorkflow_CreateEditDeleteWithValidation_ShouldWorkEndToEnd()
    {
        // Act 1: Create category
        var category = new Category
        {
            Name = "Network Installation",
            Description = "Network setup and configuration guides",
            IconGlyph = "\uE968",
            Color = "#0078D4",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var categoryId = _categoryRepository.Insert(category);
        categoryId.Should().NotBe(ObjectId.Empty);

        // Assert 1: Verify category was created
        var retrieved = _categoryRepository.GetById(categoryId);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Network Installation");
        retrieved.IconGlyph.Should().Be("\uE968");
        retrieved.Color.Should().Be("#0078D4");

        // Act 2: Update category
        retrieved.Description = "Updated description";
        retrieved.Color = "#107C10"; // Change to green
        retrieved.UpdatedAt = DateTime.UtcNow;
        var updateResult = _categoryRepository.Update(retrieved);
        updateResult.Should().BeTrue();

        // Assert 2: Verify update
        var updated = _categoryRepository.GetById(categoryId);
        updated!.Description.Should().Be("Updated description");
        updated.Color.Should().Be("#107C10");

        // Act 3: Delete category (should succeed - no guides)
        var deleteResult = _categoryRepository.Delete(categoryId);
        deleteResult.Should().BeTrue();

        // Assert 3: Verify deletion
        var deleted = _categoryRepository.GetById(categoryId);
        deleted.Should().BeNull();
    }

    [Fact]
    public void CategoryWithGuides_ShouldBeDetectableBeforeDeletion()
    {
        // Arrange - Create category
        var category = new Category
        {
            Name = "Server Setup",
            Description = "Server installation guides",
            IconGlyph = "\uE9F3",
            Color = "#8764B8"
        };
        var categoryId = _categoryRepository.Insert(category);

        // Create multiple guides in this category
        var guide1 = new Guide
        {
            Title = "Install Windows Server",
            Description = "Windows Server installation",
            Category = category.Name,
            EstimatedMinutes = 60,
            CreatedBy = "Admin"
        };

        var guide2 = new Guide
        {
            Title = "Configure Active Directory",
            Description = "AD DS setup",
            Category = category.Name,
            EstimatedMinutes = 45,
            CreatedBy = "Admin"
        };

        _guideRepository.Insert(guide1);
        _guideRepository.Insert(guide2);

        // Act - Check if category is used
        var guidesInCategory = _guideRepository.GetByCategory(category.Name).ToList();

        // Assert - Category should be in use
        guidesInCategory.Should().HaveCount(2);

        // Business logic: Deletion should be prevented
        var canDelete = guidesInCategory.Count == 0;
        canDelete.Should().BeFalse();

        // Verify category still exists (simulating prevention of deletion)
        var categoryStillExists = _categoryRepository.GetById(categoryId);
        categoryStillExists.Should().NotBeNull();
    }

    [Fact]
    public void DuplicateCategoryName_ShouldBeDetectable()
    {
        // Arrange - Create first category
        var category1 = new Category
        {
            Name = "Hardware Maintenance",
            Description = "Hardware guides",
            IconGlyph = "\uE90F",
            Color = "#D13438"
        };
        var id1 = _categoryRepository.Insert(category1);

        // Act - Try to create duplicate
        var category2 = new Category
        {
            Name = "Hardware Maintenance", // Same name
            Description = "Different description",
            IconGlyph = "\uE7EE",
            Color = "#FF8C00"
        };

        // Assert - Check if duplicate exists before insertion
        var duplicateExists = _categoryRepository.Exists("Hardware Maintenance");
        duplicateExists.Should().BeTrue();

        // Business logic should prevent insertion
        // In UI, this would trigger validation error
    }

    [Fact]
    public void EnsureCategory_ShouldCreateIfNotExistsOrReturnExisting()
    {
        // Act 1: Ensure category that doesn't exist
        var newCategory = _categoryRepository.EnsureCategory("Software Deployment");
        newCategory.Should().NotBeNull();
        newCategory.Name.Should().Be("Software Deployment");
        var newId = newCategory.Id;

        // Act 2: Ensure same category again
        var existingCategory = _categoryRepository.EnsureCategory("Software Deployment");
        existingCategory.Should().NotBeNull();
        existingCategory.Id.Should().Be(newId); // Should return existing, not create new

        // Assert - Only one category should exist
        var allCategories = _categoryRepository.GetAll().Where(c => c.Name == "Software Deployment").ToList();
        allCategories.Should().ContainSingle();
    }

    [Fact]
    public void MultipleCategories_WithMultipleGuides_ShouldOrganizeCorrectly()
    {
        // Arrange - Create multiple categories
        var networkCategory = _categoryRepository.EnsureCategory("Network");
        var serverCategory = _categoryRepository.EnsureCategory("Server");
        var softwareCategory = _categoryRepository.EnsureCategory("Software");

        // Create guides in different categories
        var guides = new[]
        {
            new Guide { Title = "Network Guide 1", Category = "Network", EstimatedMinutes = 30, CreatedBy = "Admin" },
            new Guide { Title = "Network Guide 2", Category = "Network", EstimatedMinutes = 45, CreatedBy = "Admin" },
            new Guide { Title = "Server Guide 1", Category = "Server", EstimatedMinutes = 60, CreatedBy = "Admin" },
            new Guide { Title = "Software Guide 1", Category = "Software", EstimatedMinutes = 20, CreatedBy = "Admin" },
            new Guide { Title = "Software Guide 2", Category = "Software", EstimatedMinutes = 25, CreatedBy = "Admin" },
            new Guide { Title = "Software Guide 3", Category = "Software", EstimatedMinutes = 30, CreatedBy = "Admin" }
        };

        foreach (var guide in guides)
        {
            _guideRepository.Insert(guide);
        }

        // Act - Get guides by category
        var networkGuides = _guideRepository.GetByCategory("Network").ToList();
        var serverGuides = _guideRepository.GetByCategory("Server").ToList();
        var softwareGuides = _guideRepository.GetByCategory("Software").ToList();

        // Assert - Correct counts per category
        networkGuides.Should().HaveCount(2);
        serverGuides.Should().HaveCount(1);
        softwareGuides.Should().HaveCount(3);

        // Assert - Get distinct categories
        var distinctCategories = _guideRepository.GetDistinctCategories().ToList();
        distinctCategories.Should().HaveCount(3);
        distinctCategories.Should().Contain(new[] { "Network", "Server", "Software" });
    }

    [Fact]
    public void CategoryUpdate_ShouldNotAffectExistingGuides()
    {
        // Arrange - Create category and guide
        var category = new Category
        {
            Name = "Hardware",
            Description = "Hardware guides",
            IconGlyph = "\uE90F",
            Color = "#0078D4"
        };
        var categoryId = _categoryRepository.Insert(category);

        var guide = new Guide
        {
            Title = "Install RAM",
            Description = "RAM installation guide",
            Category = "Hardware",
            EstimatedMinutes = 15,
            CreatedBy = "Admin"
        };
        var guideId = _guideRepository.Insert(guide);

        // Act - Update category (change color and description)
        category.Color = "#107C10";
        category.Description = "Updated hardware guides";
        category.UpdatedAt = DateTime.UtcNow;
        _categoryRepository.Update(category);

        // Assert - Guide should still reference correct category
        var retrievedGuide = _guideRepository.GetById(guideId);
        retrievedGuide!.Category.Should().Be("Hardware");

        // Assert - Guide should still be findable by category
        var guidesInCategory = _guideRepository.GetByCategory("Hardware").ToList();
        guidesInCategory.Should().ContainSingle();
        guidesInCategory[0].Id.Should().Be(guideId);
    }

    [Fact]
    public void GetByName_ShouldBeCaseInsensitive()
    {
        // Arrange
        var category = new Category
        {
            Name = "Network Installation",
            Description = "Network guides",
            IconGlyph = "\uE968",
            Color = "#0078D4"
        };
        _categoryRepository.Insert(category);

        // Act - Try different case variations
        var result1 = _categoryRepository.GetByName("Network Installation");
        var result2 = _categoryRepository.GetByName("network installation");
        var result3 = _categoryRepository.GetByName("NETWORK INSTALLATION");

        // Assert - All should find the category
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result3.Should().NotBeNull();
        result1!.Name.Should().Be("Network Installation");
        result2!.Name.Should().Be("Network Installation");
        result3!.Name.Should().Be("Network Installation");
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
