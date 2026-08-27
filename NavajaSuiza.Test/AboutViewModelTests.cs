using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class AboutViewModelTests
{
    private readonly Mock<IThemeService> _themeServiceMock = new();

    [Fact]
    public void Constructor_CallsApplySavedTheme()
    {
        _themeServiceMock.Setup(s => s.ApplySavedTheme()).Returns(true);
        var vm = new AboutViewModel(_themeServiceMock.Object);
        Assert.True(vm.IsDarkMode);
    }

    [Fact]
    public void IsDarkModeChange_CallsSaveThemePreference()
    {
        _themeServiceMock.Setup(s => s.ApplySavedTheme()).Returns(false);
        var vm = new AboutViewModel(_themeServiceMock.Object);

        vm.IsDarkMode = true;

        _themeServiceMock.Verify(s => s.SaveThemePreference(true), Times.Once);
    }
}
