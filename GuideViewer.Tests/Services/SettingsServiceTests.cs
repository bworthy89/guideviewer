using FluentAssertions;
using GuideViewer.Core.Models;
using GuideViewer.Core.Services;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using Xunit;

namespace GuideViewer.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly DatabaseService _databaseService;
    private readonly SettingsRepository _settingsRepository;
    private readonly SettingsService _settingsService;
    private readonly string _testDatabasePath;

    public SettingsServiceTests()
    {
        // Use a unique test database for each test run
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_settings_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _settingsRepository = new SettingsRepository(_databaseService);
        _settingsService = new SettingsService(_settingsRepository);
    }

    public void Dispose()
    {
        _databaseService?.Dispose();
        if (File.Exists(_testDatabasePath))
        {
            File.Delete(_testDatabasePath);
        }
    }

    [Fact]
    public void LoadSettings_WhenNoSettingsExist_ReturnsDefaultSettings()
    {
        // Act
        var settings = _settingsService.LoadSettings();

        // Assert
        settings.Should().NotBeNull();
        settings.Theme.Should().Be("System");
        settings.WindowWidth.Should().Be(1200);
        settings.WindowHeight.Should().Be(800);
        settings.ShowWelcomeScreen.Should().BeTrue();
    }

    [Fact]
    public void SaveSettings_ThenLoadSettings_ReturnsSavedSettings()
    {
        // Arrange
        var settings = new AppSettings
        {
            Theme = "Dark",
            WindowWidth = 1600,
            WindowHeight = 900,
            WindowX = 100,
            WindowY = 50,
            IsMaximized = true,
            ShowWelcomeScreen = false
        };

        // Act
        _settingsService.SaveSettings(settings);
        var loadedSettings = _settingsService.LoadSettings();

        // Assert
        loadedSettings.Theme.Should().Be("Dark");
        loadedSettings.WindowWidth.Should().Be(1600);
        loadedSettings.WindowHeight.Should().Be(900);
        loadedSettings.WindowX.Should().Be(100);
        loadedSettings.WindowY.Should().Be(50);
        loadedSettings.IsMaximized.Should().BeTrue();
        loadedSettings.ShowWelcomeScreen.Should().BeFalse();
    }

    [Fact]
    public void GetTheme_ReturnsCurrentTheme()
    {
        // Arrange
        var settings = new AppSettings { Theme = "Light" };
        _settingsService.SaveSettings(settings);

        // Act
        var theme = _settingsService.GetTheme();

        // Assert
        theme.Should().Be("Light");
    }

    [Fact]
    public void SetTheme_UpdatesTheme()
    {
        // Act
        _settingsService.SetTheme("Dark");
        var theme = _settingsService.GetTheme();

        // Assert
        theme.Should().Be("Dark");
    }

    [Fact]
    public void GetWindowState_ReturnsCurrentWindowState()
    {
        // Arrange
        var settings = new AppSettings
        {
            WindowWidth = 1920,
            WindowHeight = 1080,
            WindowX = 200,
            WindowY = 100,
            IsMaximized = true
        };
        _settingsService.SaveSettings(settings);

        // Act
        var (width, height, x, y, isMaximized) = _settingsService.GetWindowState();

        // Assert
        width.Should().Be(1920);
        height.Should().Be(1080);
        x.Should().Be(200);
        y.Should().Be(100);
        isMaximized.Should().BeTrue();
    }

    [Fact]
    public void SaveWindowState_UpdatesWindowState()
    {
        // Act
        _settingsService.SaveWindowState(1440, 900, 150, 75, false);
        var (width, height, x, y, isMaximized) = _settingsService.GetWindowState();

        // Assert
        width.Should().Be(1440);
        height.Should().Be(900);
        x.Should().Be(150);
        y.Should().Be(75);
        isMaximized.Should().BeFalse();
    }

    [Fact]
    public void GetValue_WhenKeyDoesNotExist_ReturnsDefaultValue()
    {
        // Act
        var value = _settingsService.GetValue<int>("NonExistentKey", 42);

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void SetValue_ThenGetValue_ReturnsSetValue()
    {
        // Arrange
        var testData = new TestData { Name = "Test", Count = 5 };

        // Act
        _settingsService.SetValue("TestKey", testData);
        var retrievedValue = _settingsService.GetValue<TestData>("TestKey");

        // Assert
        retrievedValue.Should().NotBeNull();
        retrievedValue!.Name.Should().Be("Test");
        retrievedValue.Count.Should().Be(5);
    }

    private class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    [Fact]
    public void SetValue_WithString_ThenGetValue_ReturnsString()
    {
        // Act
        _settingsService.SetValue("StringKey", "Hello World");
        var value = _settingsService.GetValue<string>("StringKey");

        // Assert
        value.Should().Be("Hello World");
    }

    [Fact]
    public void SetValue_WithInteger_ThenGetValue_ReturnsInteger()
    {
        // Act
        _settingsService.SetValue("IntKey", 12345);
        var value = _settingsService.GetValue<int>("IntKey");

        // Assert
        value.Should().Be(12345);
    }

    [Fact]
    public void SetValue_WithBoolean_ThenGetValue_ReturnsBoolean()
    {
        // Act
        _settingsService.SetValue("BoolKey", true);
        var value = _settingsService.GetValue<bool>("BoolKey");

        // Assert
        value.Should().BeTrue();
    }

    [Fact]
    public void LoadSettings_CalledMultipleTimes_UsesCachedValue()
    {
        // Arrange
        _settingsService.SetTheme("Dark");

        // Act
        var settings1 = _settingsService.LoadSettings();
        var settings2 = _settingsService.LoadSettings();

        // Assert
        settings1.Should().BeSameAs(settings2); // Should be the same instance (cached)
    }

    [Fact]
    public void SaveSettings_UpdatesCache()
    {
        // Arrange
        var settings1 = _settingsService.LoadSettings();
        settings1.Theme = "Light";

        // Act
        _settingsService.SaveSettings(settings1);
        var settings2 = _settingsService.LoadSettings();

        // Assert
        settings2.Theme.Should().Be("Light");
        settings2.Should().BeSameAs(settings1);
    }
}
