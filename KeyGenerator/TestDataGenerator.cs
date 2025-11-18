using GuideViewer.Data;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KeyGenerator;

/// <summary>
/// Generates comprehensive test data for GuideViewer application.
/// Use this for automated test setup and performance testing.
/// </summary>
public class TestDataGenerator
{
    private readonly DatabaseService _databaseService;
    private readonly GuideRepository _guideRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly ProgressRepository _progressRepository;
    private readonly Random _random = new();

    public TestDataGenerator(string databasePath)
    {
        _databaseService = new DatabaseService(databasePath);
        _guideRepository = new GuideRepository(_databaseService);
        _categoryRepository = new CategoryRepository(_databaseService);
        _progressRepository = new ProgressRepository(_databaseService);
    }

    /// <summary>
    /// Generates a complete test dataset with categories, guides, and progress.
    /// </summary>
    public void GenerateCompleteTestData(
        int categoryCount = 5,
        int guideCount = 50,
        int guidesWithProgress = 10,
        bool verbose = true)
    {
        if (verbose) Console.WriteLine("=== GuideViewer Test Data Generator ===\n");

        // Step 1: Create categories
        var categories = GenerateCategories(categoryCount, verbose);

        // Step 2: Create guides
        var guides = GenerateGuides(guideCount, categories, verbose);

        // Step 3: Create progress records
        GenerateProgress(guides.Take(guidesWithProgress).ToList(), verbose);

        if (verbose)
        {
            Console.WriteLine("\n=== Test Data Generation Complete ===");
            Console.WriteLine($"Total Categories: {categoryCount}");
            Console.WriteLine($"Total Guides: {guideCount}");
            Console.WriteLine($"Guides with Progress: {guidesWithProgress}");
        }
    }

    /// <summary>
    /// Generates test categories with unique names, icons, and colors.
    /// </summary>
    public List<Category> GenerateCategories(int count = 5, bool verbose = true)
    {
        if (verbose) Console.WriteLine($"Creating {count} categories...");

        var categories = new List<Category>();
        var categoryTemplates = new[]
        {
            new { Name = "Installation", Icon = "\uE74E", Color = "#FF6B6B" },
            new { Name = "Maintenance", Icon = "\uE90F", Color = "#4ECDC4" },
            new { Name = "Troubleshooting", Icon = "\uE8A7", Color = "#FFA07A" },
            new { Name = "Safety", Icon = "\uE7BA", Color = "#98D8C8" },
            new { Name = "Advanced", Icon = "\uE81E", Color = "#95E1D3" },
            new { Name = "Emergency", Icon = "\uE8C3", Color = "#FF0000" },
            new { Name = "Routine", Icon = "\uE8EF", Color = "#00CED1" },
            new { Name = "Inspection", Icon = "\uE7C3", Color = "#FFD700" },
            new { Name = "Repair", Icon = "\uE74C", Color = "#FF69B4" },
            new { Name = "Upgrade", Icon = "\uE895", Color = "#9370DB" }
        };

        for (int i = 0; i < Math.Min(count, categoryTemplates.Length); i++)
        {
            var template = categoryTemplates[i];
            var category = new Category
            {
                Name = template.Name,
                IconGlyph = template.Icon,
                Color = template.Color
            };

            _categoryRepository.Insert(category);
            categories.Add(category);

            if (verbose) Console.WriteLine($"  ✓ {category.Name} ({category.Color})");
        }

        // If more categories needed than templates, generate numbered ones
        for (int i = categoryTemplates.Length; i < count; i++)
        {
            var category = new Category
            {
                Name = $"Test Category {i + 1}",
                IconGlyph = "\uE81E",
                Color = GenerateRandomColor()
            };

            _categoryRepository.Insert(category);
            categories.Add(category);

            if (verbose) Console.WriteLine($"  ✓ {category.Name} ({category.Color})");
        }

        return categories;
    }

    /// <summary>
    /// Generates test guides with steps and realistic data.
    /// </summary>
    public List<Guide> GenerateGuides(int count, List<Category> categories, bool verbose = true)
    {
        if (verbose) Console.WriteLine($"\nCreating {count} guides...");

        var guides = new List<Guide>();
        var guideTitles = new[]
        {
            "Kitchen Sink Installation",
            "Water Heater Replacement",
            "Bathroom Faucet Repair",
            "HVAC System Maintenance",
            "Electrical Panel Upgrade",
            "Plumbing Pipe Repair",
            "Air Conditioner Service",
            "Furnace Filter Replacement",
            "Toilet Installation",
            "Garbage Disposal Repair",
            "Dishwasher Installation",
            "Washing Machine Hookup",
            "Dryer Vent Cleaning",
            "Ceiling Fan Installation",
            "Light Fixture Replacement",
            "Outlet Installation",
            "Circuit Breaker Replacement",
            "Thermostat Installation",
            "Smoke Detector Installation",
            "Carbon Monoxide Detector Setup",
            "Door Lock Installation",
            "Window Replacement",
            "Gutter Cleaning",
            "Roof Inspection",
            "Foundation Crack Repair",
            "Sump Pump Installation",
            "Water Softener Setup",
            "Garage Door Opener Repair",
            "Fence Installation",
            "Deck Maintenance"
        };

        for (int i = 0; i < count; i++)
        {
            // Select title (cycle through or generate)
            string title = i < guideTitles.Length
                ? guideTitles[i]
                : $"Test Guide #{i + 1:D3}";

            // Randomly select category
            var category = categories[_random.Next(categories.Count)];

            // Random number of steps (3-10)
            int stepCount = _random.Next(3, 11);

            var guide = new Guide
            {
                Title = title,
                Description = GenerateGuideDescription(title),
                Category = category.Name,
                EstimatedMinutes = _random.Next(15, 121), // 15-120 minutes
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 365)),
                UpdatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                Steps = new List<Step>()
            };

            // Generate steps
            for (int j = 1; j <= stepCount; j++)
            {
                guide.Steps.Add(new Step
                {
                    Id = ObjectId.NewObjectId(),
                    Order = j,
                    Title = $"Step {j}: {GenerateStepTitle()}",
                    Content = GenerateStepContent(j)
                });
            }

            _guideRepository.Insert(guide);
            guides.Add(guide);

            if (verbose && (i + 1) % 10 == 0)
                Console.WriteLine($"  ✓ Created {i + 1}/{count} guides...");
        }

        if (verbose) Console.WriteLine($"  ✓ All {count} guides created!");
        return guides;
    }

    /// <summary>
    /// Generates progress records for guides with various completion states.
    /// </summary>
    public void GenerateProgress(List<Guide> guides, bool verbose = true)
    {
        if (verbose) Console.WriteLine($"\nCreating progress for {guides.Count} guides...");

        // Assume user ID 1 (first activated user)
        ObjectId userId = ObjectId.NewObjectId();

        foreach (var guide in guides)
        {
            // Random completion percentage (0%, 25%, 50%, 75%, 100%)
            var completionStates = new[] { 0, 0.25, 0.5, 0.75, 1.0 };
            var completionPercent = completionStates[_random.Next(completionStates.Length)];

            var stepsCompleted = (int)(guide.Steps.Count * completionPercent);

            var progress = new Progress
            {
                UserId = userId,
                GuideId = guide.Id,
                CurrentStepIndex = stepsCompleted < guide.Steps.Count ? stepsCompleted : guide.Steps.Count - 1,
                CompletedSteps = new List<bool>(),
                StartedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 30)),
                LastAccessedAt = DateTime.UtcNow.AddHours(-_random.Next(1, 48)),
                TimeSpentSeconds = _random.Next(300, 7200) // 5 min - 2 hours
            };

            // Mark steps as completed
            for (int i = 0; i < guide.Steps.Count; i++)
            {
                progress.CompletedSteps.Add(i < stepsCompleted);
            }

            // If all steps completed, mark as completed
            if (completionPercent >= 1.0)
            {
                progress.CompletedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 7));
            }

            _progressRepository.Insert(progress);

            if (verbose)
            {
                var status = completionPercent >= 1.0 ? "Complete" : $"{completionPercent * 100:F0}%";
                Console.WriteLine($"  ✓ {guide.Title}: {status}");
            }
        }
    }

    /// <summary>
    /// Generates a realistic guide description.
    /// </summary>
    private string GenerateGuideDescription(string title)
    {
        var templates = new[]
        {
            $"This comprehensive guide will walk you through {title.ToLower()}. Follow each step carefully to ensure proper installation and safety.",
            $"Learn how to complete {title.ToLower()} efficiently and safely. This guide includes detailed instructions and safety tips.",
            $"Step-by-step instructions for {title.ToLower()}. Suitable for both beginners and experienced technicians.",
            $"Complete {title.ToLower()} following best practices. This guide covers all necessary steps and safety precautions.",
            $"Professional guide for {title.ToLower()}. Includes troubleshooting tips and common mistakes to avoid."
        };

        return templates[_random.Next(templates.Length)];
    }

    /// <summary>
    /// Generates a random step title.
    /// </summary>
    private string GenerateStepTitle()
    {
        var actions = new[]
        {
            "Gather Tools and Materials",
            "Prepare the Work Area",
            "Turn Off Main Supply",
            "Remove Old Component",
            "Install New Component",
            "Connect Wiring",
            "Test Functionality",
            "Secure All Connections",
            "Clean Up Work Area",
            "Perform Final Inspection",
            "Document Completion",
            "Verify Safety Measures",
            "Apply Protective Coating",
            "Calibrate Settings",
            "Run Diagnostic Tests"
        };

        return actions[_random.Next(actions.Length)];
    }

    /// <summary>
    /// Generates realistic step content.
    /// </summary>
    private string GenerateStepContent(int stepNumber)
    {
        var templates = new[]
        {
            $"In this step, you will need to carefully follow the instructions provided. " +
            $"Ensure all safety equipment is in place before beginning. " +
            $"Take your time and verify each action before proceeding to the next.",

            $"Begin by identifying the necessary components for this step. " +
            $"Refer to the diagram if provided. Use appropriate tools and techniques. " +
            $"Double-check all measurements and connections.",

            $"This is a critical step that requires attention to detail. " +
            $"Follow manufacturer specifications closely. " +
            $"If you encounter any issues, refer to the troubleshooting section.",

            $"Carefully execute this step following all safety protocols. " +
            $"Verify that previous steps have been completed correctly. " +
            $"Proceed methodically and avoid rushing through this process.",

            $"Complete this step by following the outlined procedure. " +
            $"Use proper protective equipment at all times. " +
            $"Ensure the work area is clear and well-lit before starting."
        };

        return templates[_random.Next(templates.Length)];
    }

    /// <summary>
    /// Generates a random hex color.
    /// </summary>
    private string GenerateRandomColor()
    {
        return $"#{_random.Next(0x1000000):X6}";
    }

    /// <summary>
    /// Deletes all test data from the database.
    /// </summary>
    public void ClearAllData(bool verbose = true)
    {
        if (verbose) Console.WriteLine("Clearing all test data...");

        var allGuides = _guideRepository.GetAll();
        foreach (var guide in allGuides)
        {
            _guideRepository.Delete(guide.Id);
        }

        var allCategories = _categoryRepository.GetAll();
        foreach (var category in allCategories)
        {
            _categoryRepository.Delete(category.Id);
        }

        var allProgress = _progressRepository.GetAll();
        foreach (var progress in allProgress)
        {
            _progressRepository.Delete(progress.Id);
        }

        if (verbose) Console.WriteLine("✓ All test data cleared!");
    }

    /// <summary>
    /// Displays statistics about current database content.
    /// </summary>
    public void ShowStatistics()
    {
        var categoryCount = _categoryRepository.GetAll().Count();
        var guideCount = _guideRepository.GetAll().Count();
        var progressCount = _progressRepository.GetAll().Count();
        var totalSteps = _guideRepository.GetAll().Sum(g => g.Steps.Count);

        Console.WriteLine("\n=== Database Statistics ===");
        Console.WriteLine($"Categories: {categoryCount}");
        Console.WriteLine($"Guides: {guideCount}");
        Console.WriteLine($"Total Steps: {totalSteps}");
        Console.WriteLine($"Progress Records: {progressCount}");
        Console.WriteLine($"Average Steps per Guide: {(guideCount > 0 ? totalSteps / (double)guideCount : 0):F1}");
    }
}

/// <summary>
/// Example usage and menu for TestDataGenerator.
/// Run this from KeyGenerator project main method.
/// </summary>
public static class TestDataGeneratorExample
{
    public static void Main(string[] args)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GuideViewer",
            "data.db"
        );

        var generator = new TestDataGenerator(dbPath);

        Console.WriteLine("=== GuideViewer Test Data Generator ===\n");
        Console.WriteLine("Database: " + dbPath);
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("\nOptions:");
            Console.WriteLine("1. Generate Minimal Test Data (5 cats, 10 guides)");
            Console.WriteLine("2. Generate Standard Test Data (5 cats, 50 guides)");
            Console.WriteLine("3. Generate Large Test Data (10 cats, 100 guides)");
            Console.WriteLine("4. Generate Custom Test Data");
            Console.WriteLine("5. Show Statistics");
            Console.WriteLine("6. Clear All Data");
            Console.WriteLine("7. Exit");
            Console.Write("\nSelect option: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        generator.GenerateCompleteTestData(5, 10, 5);
                        break;
                    case "2":
                        generator.GenerateCompleteTestData(5, 50, 10);
                        break;
                    case "3":
                        generator.GenerateCompleteTestData(10, 100, 20);
                        break;
                    case "4":
                        GenerateCustomData(generator);
                        break;
                    case "5":
                        generator.ShowStatistics();
                        break;
                    case "6":
                        Console.Write("Are you sure? This will delete ALL data (y/n): ");
                        if (Console.ReadLine()?.ToLower() == "y")
                        {
                            generator.ClearAllData();
                        }
                        break;
                    case "7":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void GenerateCustomData(TestDataGenerator generator)
    {
        Console.Write("Number of categories: ");
        int categoryCount = int.Parse(Console.ReadLine() ?? "5");

        Console.Write("Number of guides: ");
        int guideCount = int.Parse(Console.ReadLine() ?? "10");

        Console.Write("Number of guides with progress: ");
        int progressCount = int.Parse(Console.ReadLine() ?? "5");

        generator.GenerateCompleteTestData(categoryCount, guideCount, progressCount);
    }
}
