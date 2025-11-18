using FluentAssertions;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using Xunit;

namespace GuideViewer.Tests.Performance;

/// <summary>
/// Performance benchmark tests for critical operations.
/// </summary>
public class PerformanceTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly GuideRepository _guideRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly PerformanceMonitoringService _performanceService;

    public PerformanceTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"perf_test_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _guideRepository = new GuideRepository(_databaseService);
        _categoryRepository = new CategoryRepository(_databaseService);
        _performanceService = new PerformanceMonitoringService();

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create test categories
        var categories = new[]
        {
            new Category { Name = "Installation", IconGlyph = "Install", Color = "#FF0000" },
            new Category { Name = "Maintenance", IconGlyph = "Wrench", Color = "#00FF00" },
            new Category { Name = "Troubleshooting", IconGlyph = "Alert", Color = "#0000FF" }
        };

        foreach (var category in categories)
        {
            _categoryRepository.Insert(category);
        }

        // Create 100 test guides for performance testing
        for (int i = 0; i < 100; i++)
        {
            var guide = new Guide
            {
                Title = $"Test Guide {i}",
                Description = $"Performance test guide number {i}",
                Category = categories[i % 3].Name,
                EstimatedMinutes = 30 + (i % 60),
                CreatedAt = DateTime.Now.AddDays(-i),
                UpdatedAt = DateTime.Now.AddDays(-i / 2),
                Steps = new List<Step>
                {
                    new Step { Order = 1, Title = $"Step 1", Content = "Test content" },
                    new Step { Order = 2, Title = $"Step 2", Content = "Test content" },
                    new Step { Order = 3, Title = $"Step 3", Content = "Test content" }
                }
            };

            _guideRepository.Insert(guide);
        }
    }

    [Fact]
    public void GuideListLoad_Should_CompleteUnder500ms()
    {
        // Arrange & Act
        using (var measurement = _performanceService.MeasureOperation("GuideListLoad"))
        {
            var guides = _guideRepository.GetAll();
            guides.Should().HaveCount(100);
        }

        // Assert
        var metrics = _performanceService.GetMetricsByOperation("GuideListLoad");
        metrics.Should().ContainSingle();
        metrics.First().DurationMs.Should().BeLessThan(500, "guide list should load in under 500ms");
    }

    [Fact]
    public void GuideSearch_Should_CompleteUnder200ms()
    {
        // Arrange & Act
        using (var measurement = _performanceService.MeasureOperation("GuideSearch"))
        {
            var guides = _guideRepository.GetAll()
                .Where(g => g.Title.Contains("50"))
                .ToList();
            guides.Should().NotBeEmpty();
        }

        // Assert
        var metrics = _performanceService.GetMetricsByOperation("GuideSearch");
        metrics.Should().ContainSingle();
        metrics.First().DurationMs.Should().BeLessThan(200, "search should complete in under 200ms");
    }

    [Fact]
    public void GuideById_Should_CompleteUnder100ms()
    {
        // Arrange
        var allGuides = _guideRepository.GetAll();
        var testGuide = allGuides.First();

        // Act
        using (var measurement = _performanceService.MeasureOperation("DatabaseQuery"))
        {
            var guide = _guideRepository.GetById(testGuide.Id);
            guide.Should().NotBeNull();
        }

        // Assert
        var metrics = _performanceService.GetMetricsByOperation("DatabaseQuery");
        metrics.Should().ContainSingle();
        metrics.First().DurationMs.Should().BeLessThan(100, "single guide retrieval should complete in under 100ms");
    }

    [Fact]
    public void GuidesByCategory_Should_CompleteUnder200ms()
    {
        // Arrange & Act
        using (var measurement = _performanceService.MeasureOperation("GuideSearch"))
        {
            var guides = _guideRepository.GetAll()
                .Where(g => g.Category == "Installation")
                .ToList();
            guides.Should().NotBeEmpty();
        }

        // Assert
        var metrics = _performanceService.GetMetricsByOperation("GuideSearch");
        metrics.Should().ContainSingle();
        metrics.First().DurationMs.Should().BeLessThan(200, "category filter should complete in under 200ms");
    }

    [Fact]
    public void BulkGuideCreation_Should_CompleteUnder2Seconds()
    {
        // Arrange
        var newGuides = Enumerable.Range(0, 50).Select(i => new Guide
        {
            Title = $"Bulk Guide {i}",
            Description = "Bulk test",
            Category = "Installation",
            EstimatedMinutes = 30,
            Steps = new List<Step>
            {
                new Step { Order = 1, Title = "Step 1", Content = "Content" }
            }
        }).ToList();

        // Act
        using (var measurement = _performanceService.MeasureOperation("BulkInsert"))
        {
            foreach (var guide in newGuides)
            {
                _guideRepository.Insert(guide);
            }
        }

        // Assert
        var metrics = _performanceService.GetMetricsByOperation("BulkInsert");
        metrics.Should().ContainSingle();
        metrics.First().DurationMs.Should().BeLessThan(2000, "50 guide insertions should complete in under 2 seconds");
    }

    [Fact]
    public void MemoryUsage_Should_RemainUnder200MB()
    {
        // Act - Load all guides multiple times
        for (int i = 0; i < 5; i++)
        {
            var guides = _guideRepository.GetAll();
            guides.Should().NotBeEmpty();
        }

        // Assert
        var memoryUsage = _performanceService.GetCurrentMemoryUsage();
        var memoryUsageMB = memoryUsage / (1024.0 * 1024.0);

        memoryUsageMB.Should().BeLessThan(200, "memory usage should stay under 200MB");
    }

    [Fact]
    public void PerformanceMonitoring_Should_DetectSlowOperations()
    {
        // Arrange
        var slowOperationDetected = false;
        _performanceService.SlowOperationDetected += (sender, metric) =>
        {
            slowOperationDetected = true;
        };

        // Act - Simulate slow operation
        _performanceService.SetPerformanceTarget("SlowOperation", 10); // 10ms target
        using (var measurement = _performanceService.MeasureOperation("SlowOperation"))
        {
            Thread.Sleep(50); // Sleep for 50ms
        }

        // Assert
        slowOperationDetected.Should().BeTrue("slow operation should be detected");
        var slowOps = _performanceService.GetSlowOperations();
        slowOps.Should().ContainSingle(op => op.OperationName == "SlowOperation");
    }

    [Fact]
    public void PerformanceMetrics_Should_CalculateAverageDuration()
    {
        // Arrange & Act
        for (int i = 0; i < 5; i++)
        {
            using (var measurement = _performanceService.MeasureOperation("TestOperation"))
            {
                Thread.Sleep(10);
            }
        }

        // Assert
        var averageDuration = _performanceService.GetAverageDuration("TestOperation");
        averageDuration.Should().BeGreaterThan(0);
        averageDuration.Should().BeInRange(5, 50, "average should be reasonable");
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
