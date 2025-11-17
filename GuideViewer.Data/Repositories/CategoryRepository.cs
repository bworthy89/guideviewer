using GuideViewer.Data.Entities;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GuideViewer.Data.Repositories;

/// <summary>
/// Repository for managing guide categories in LiteDB.
/// </summary>
public class CategoryRepository : Repository<Category>
{
    public CategoryRepository(DatabaseService databaseService) : base(databaseService, "categories")
    {
    }

    /// <summary>
    /// Gets a category by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <returns>The category if found, otherwise null.</returns>
    public Category? GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return Collection.FindOne(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a category with the given name exists (case-insensitive).
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <returns>True if the category exists.</returns>
    public bool Exists(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return Collection.Exists(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a category with the given name exists, excluding a specific category ID.
    /// Useful for checking duplicates when editing a category.
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <param name="excludeId">The category ID to exclude from the check.</param>
    /// <returns>True if another category with the same name exists.</returns>
    public bool Exists(string name, ObjectId excludeId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return Collection.Exists(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
            !c.Id.Equals(excludeId));
    }

    /// <summary>
    /// Gets all categories ordered by name.
    /// </summary>
    /// <returns>All categories ordered alphabetically by name.</returns>
    public new IEnumerable<Category> GetAll()
    {
        return Collection.FindAll().OrderBy(c => c.Name);
    }

    /// <summary>
    /// Updates a category and sets the UpdatedAt timestamp.
    /// </summary>
    /// <param name="entity">The category to update.</param>
    /// <returns>True if the update succeeded.</returns>
    public override bool Update(Category entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        return base.Update(entity);
    }

    /// <summary>
    /// Inserts a new category if the name doesn't already exist.
    /// </summary>
    /// <param name="entity">The category to insert.</param>
    /// <returns>The inserted category ID, or null if a category with the same name exists.</returns>
    public ObjectId? InsertIfNotExists(Category entity)
    {
        if (Exists(entity.Name))
        {
            return null;
        }

        return Insert(entity);
    }

    /// <summary>
    /// Ensures a category exists by name, creating it if necessary.
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <returns>The existing or newly created category.</returns>
    public Category EnsureCategory(string name)
    {
        var existing = GetByName(name);
        if (existing != null)
        {
            return existing;
        }

        var newCategory = new Category
        {
            Name = name,
            Description = $"Auto-created category: {name}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var id = Insert(newCategory);
        newCategory.Id = id;
        return newCategory;
    }
}
