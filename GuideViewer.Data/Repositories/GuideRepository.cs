using GuideViewer.Data.Entities;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GuideViewer.Data.Repositories;

/// <summary>
/// Repository for managing installation guides in LiteDB.
/// </summary>
public class GuideRepository : Repository<Guide>
{
    public GuideRepository(DatabaseService databaseService) : base(databaseService, "guides")
    {
    }

    /// <summary>
    /// Searches for guides by title, description, or category (case-insensitive).
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <returns>Matching guides ordered by title.</returns>
    public IEnumerable<Guide> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return GetAll();
        }

        var normalizedQuery = query.Trim().ToLowerInvariant();

        return Collection
            .Find(g =>
                g.Title.ToLower().Contains(normalizedQuery) ||
                g.Description.ToLower().Contains(normalizedQuery) ||
                g.Category.ToLower().Contains(normalizedQuery))
            .OrderBy(g => g.Title);
    }

    /// <summary>
    /// Gets all guides in a specific category.
    /// </summary>
    /// <param name="category">The category name.</param>
    /// <returns>Guides in the specified category ordered by title.</returns>
    public IEnumerable<Guide> GetByCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return Enumerable.Empty<Guide>();
        }

        return Collection
            .Find(g => g.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(g => g.Title);
    }

    /// <summary>
    /// Gets the most recently modified guides.
    /// </summary>
    /// <param name="count">The number of guides to retrieve.</param>
    /// <returns>Recently modified guides ordered by update date (newest first).</returns>
    public IEnumerable<Guide> GetRecentlyModified(int count = 10)
    {
        return Collection
            .Find(Query.All(Query.Descending))
            .OrderByDescending(g => g.UpdatedAt)
            .Take(count);
    }

    /// <summary>
    /// Gets all guides ordered by title.
    /// </summary>
    /// <returns>All guides ordered alphabetically by title.</returns>
    public new IEnumerable<Guide> GetAll()
    {
        return Collection.FindAll().OrderBy(g => g.Title);
    }

    /// <summary>
    /// Updates a guide and sets the UpdatedAt timestamp.
    /// </summary>
    /// <param name="entity">The guide to update.</param>
    /// <returns>True if the update succeeded.</returns>
    public override bool Update(Guide entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        return base.Update(entity);
    }

    /// <summary>
    /// Deletes a guide and all associated images from FileStorage.
    /// </summary>
    /// <param name="id">The guide ID.</param>
    /// <returns>True if the deletion succeeded.</returns>
    public override bool Delete(ObjectId id)
    {
        // Get the guide first to access image IDs
        var guide = GetById(id);
        if (guide == null)
        {
            return false;
        }

        // Delete all associated images from FileStorage
        var fileStorage = Database.FileStorage;
        foreach (var step in guide.Steps)
        {
            foreach (var imageId in step.ImageIds)
            {
                try
                {
                    fileStorage.Delete(imageId);
                }
                catch
                {
                    // Log error but continue deleting other images
                }
            }
        }

        // Delete the guide document
        return base.Delete(id);
    }

    /// <summary>
    /// Gets the count of guides in a specific category.
    /// </summary>
    /// <param name="category">The category name.</param>
    /// <returns>The number of guides in the category.</returns>
    public int GetCategoryCount(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return 0;
        }

        return Collection.Count(g => g.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all distinct categories used by guides.
    /// </summary>
    /// <returns>Distinct category names ordered alphabetically.</returns>
    public IEnumerable<string> GetDistinctCategories()
    {
        return Collection
            .FindAll()
            .Select(g => g.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c);
    }
}
