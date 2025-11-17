using FluentAssertions;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GuideViewer.Tests.Repositories;

/// <summary>
/// Unit tests for CategoryRepository.
/// </summary>
public class CategoryRepositoryTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly CategoryRepository _categoryRepository;

    public CategoryRepositoryTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_categories_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _categoryRepository = new CategoryRepository(_databaseService);
    }

    [Fact]
    public void Insert_ShouldAddCategoryToDatabase()
    {
        // Arrange
        var category = new Category
        {
            Name = "Installation",
            Description = "Installation guides",
            IconGlyph = "\uE8F1",
            Color = "#0078D4"
        };

        // Act
        var id = _categoryRepository.Insert(category);

        // Assert
        id.Should().NotBe(ObjectId.Empty);

        var retrieved = _categoryRepository.GetById(id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Installation");
        retrieved.Description.Should().Be("Installation guides");
        retrieved.IconGlyph.Should().Be("\uE8F1");
        retrieved.Color.Should().Be("#0078D4");
    }

    [Fact]
    public void Update_ShouldModifyExistingCategory()
    {
        // Arrange
        var category = new Category
        {
            Name = "Original Name",
            Description = "Original Description"
        };
        var id = _categoryRepository.Insert(category);

        // Act
        category.Name = "Updated Name";
        category.Description = "Updated Description";
        var result = _categoryRepository.Update(category);

        // Assert
        result.Should().BeTrue();

        var retrieved = _categoryRepository.GetById(id);
        retrieved!.Name.Should().Be("Updated Name");
        retrieved.Description.Should().Be("Updated Description");
        retrieved.UpdatedAt.Should().BeAfter(retrieved.CreatedAt);
    }

    [Fact]
    public void Delete_ShouldRemoveCategory()
    {
        // Arrange
        var category = new Category { Name = "To Delete" };
        var id = _categoryRepository.Insert(category);

        // Act
        var result = _categoryRepository.Delete(id);

        // Assert
        result.Should().BeTrue();
        _categoryRepository.GetById(id).Should().BeNull();
    }

    [Fact]
    public void GetAll_ShouldReturnAllCategoriesOrderedByName()
    {
        // Arrange
        _categoryRepository.Insert(new Category { Name = "Zebra" });
        _categoryRepository.Insert(new Category { Name = "Alpha" });
        _categoryRepository.Insert(new Category { Name = "Beta" });

        // Act
        var categories = _categoryRepository.GetAll().ToList();

        // Assert
        categories.Should().HaveCount(3);
        categories[0].Name.Should().Be("Alpha");
        categories[1].Name.Should().Be("Beta");
        categories[2].Name.Should().Be("Zebra");
    }

    [Fact]
    public void GetByName_WithExactMatch_ShouldReturnCategory()
    {
        // Arrange
        _categoryRepository.Insert(new Category { Name = "Installation" });

        // Act
        var result = _categoryRepository.GetByName("Installation");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Installation");
    }

    [Fact]
    public void GetByName_IsCaseInsensitive()
    {
        // Arrange
        _categoryRepository.Insert(new Category { Name = "Installation" });

        // Act
        var result1 = _categoryRepository.GetByName("INSTALLATION");
        var result2 = _categoryRepository.GetByName("installation");
        var result3 = _categoryRepository.GetByName("InStAlLaTiOn");

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result3.Should().NotBeNull();
    }

    [Fact]
    public void GetByName_WithNonExistent_ShouldReturnNull()
    {
        // Act
        var result = _categoryRepository.GetByName("NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Exists_WithExistingCategory_ShouldReturnTrue()
    {
        // Arrange
        _categoryRepository.Insert(new Category { Name = "Installation" });

        // Act
        var result = _categoryRepository.Exists("Installation");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Exists_IsCaseInsensitive()
    {
        // Arrange
        _categoryRepository.Insert(new Category { Name = "Installation" });

        // Act
        var result = _categoryRepository.Exists("installation");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Exists_WithNonExistent_ShouldReturnFalse()
    {
        // Act
        var result = _categoryRepository.Exists("NonExistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Exists_WithExcludeId_ShouldIgnoreSpecifiedCategory()
    {
        // Arrange
        var category = new Category { Name = "Installation" };
        var id = _categoryRepository.Insert(category);
        category.Id = id;

        // Act - checking if "Installation" exists, but excluding the one we just inserted
        var result = _categoryRepository.Exists("Installation", id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Exists_WithExcludeId_ShouldDetectOtherDuplicates()
    {
        // Arrange
        var category1 = new Category { Name = "Installation" };
        var id1 = _categoryRepository.Insert(category1);
        category1.Id = id1;

        var category2 = new Category { Name = "Installation" };
        // Note: This would normally fail due to unique index, but for testing we'll use a different approach
        // In reality, the unique index on Name in DatabaseService prevents this

        // Act
        var result = _categoryRepository.Exists("Installation", ObjectId.NewObjectId());

        // Assert
        result.Should().BeTrue(); // Should find category1
    }

    [Fact]
    public void InsertIfNotExists_WithNewCategory_ShouldInsert()
    {
        // Arrange
        var category = new Category { Name = "NewCategory" };

        // Act
        var id = _categoryRepository.InsertIfNotExists(category);

        // Assert
        id.Should().NotBeNull();
        _categoryRepository.GetById(id!).Should().NotBeNull();
    }

    [Fact]
    public void InsertIfNotExists_WithExistingCategory_ShouldReturnNull()
    {
        // Arrange
        _categoryRepository.Insert(new Category { Name = "Existing" });
        var duplicate = new Category { Name = "Existing" };

        // Act
        var id = _categoryRepository.InsertIfNotExists(duplicate);

        // Assert
        id.Should().BeNull();
    }

    [Fact]
    public void EnsureCategory_WithExisting_ShouldReturnExisting()
    {
        // Arrange
        var original = new Category { Name = "Installation", Description = "Original" };
        _categoryRepository.Insert(original);

        // Act
        var result = _categoryRepository.EnsureCategory("Installation");

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be("Original");
    }

    [Fact]
    public void EnsureCategory_WithNew_ShouldCreateAndReturn()
    {
        // Act
        var result = _categoryRepository.EnsureCategory("NewCategory");

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("NewCategory");
        result.Description.Should().Contain("Auto-created");

        // Verify it was actually saved
        var retrieved = _categoryRepository.GetByName("NewCategory");
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public void Category_ShouldHaveDefaultValues()
    {
        // Arrange
        var category = new Category();

        // Assert
        category.Name.Should().Be(string.Empty);
        category.Description.Should().Be(string.Empty);
        category.IconGlyph.Should().Be("\uE8F1"); // Default document icon
        category.Color.Should().Be("#0078D4"); // Default Windows blue
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        category.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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
