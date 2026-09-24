using Moq;
using NavajaSuiza.Core;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class AboutViewModelTests
{
    private readonly Mock<IThemeService> _themeServiceMock = new();
    private readonly Mock<IAppInfoService> _appInfoServiceMock = new();
    private readonly Mock<ILauncherService> _launcherServiceMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();

    private AboutViewModel CreateSut()
    {
        _appInfoServiceMock.Setup(s => s.Version).Returns("1.0");
        _appInfoServiceMock.Setup(s => s.Build).Returns("1");
        return new AboutViewModel(
            _themeServiceMock.Object,
            _appInfoServiceMock.Object,
            _launcherServiceMock.Object,
            _navigationServiceMock.Object);
    }

    [Fact]
    public void Constructor_CallsApplySavedTheme()
    {
        _themeServiceMock.Setup(s => s.ApplySavedTheme()).Returns(true);
        var vm = CreateSut();
        Assert.True(vm.IsDarkMode);
    }

    [Fact]
    public void IsDarkModeChange_CallsSaveThemePreference()
    {
        _themeServiceMock.Setup(s => s.ApplySavedTheme()).Returns(false);
        var vm = CreateSut();

        vm.IsDarkMode = true;

        _themeServiceMock.Verify(s => s.SaveThemePreference(true), Times.Once);
    }

    [Fact]
    public void VersionText_UsesAppInfoVersionAndBuild()
    {
        var vm = CreateSut();
        _appInfoServiceMock.Setup(s => s.Version).Returns("1.2.3");
        _appInfoServiceMock.Setup(s => s.Build).Returns("42");

        Assert.Equal("v1.2.3 (build 42)", vm.VersionText);
    }

    [Fact]
    public void HasDonationUrl_IsFalseWhenNotConfigured()
    {
        var vm = CreateSut();
        Assert.False(vm.HasDonationUrl);
    }

    [Fact]
    public void OpenDonationCommand_WhenNotConfigured_DoesNotOpen()
    {
        var vm = CreateSut();
        vm.OpenDonationCommand.Execute(null);

        _launcherServiceMock.Verify(s => s.OpenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OpenWebsiteCommand_OpensWebsiteUrl()
    {
        var vm = CreateSut();
        vm.OpenWebsiteCommand.Execute(null);

        _launcherServiceMock.Verify(s => s.OpenAsync(AppConstants.About.WebsiteUrl), Times.Once);
    }

    [Fact]
    public void OpenPrivacyCommand_OpensPrivacyUrl()
    {
        var vm = CreateSut();
        vm.OpenPrivacyCommand.Execute(null);

        _launcherServiceMock.Verify(s => s.OpenAsync(AppConstants.About.PrivacyUrl), Times.Once);
    }

    [Fact]
    public void OpenGuideCommand_NavigatesToGuidePage()
    {
        var vm = CreateSut();
        vm.OpenGuideCommand.Execute(null);

        _navigationServiceMock.Verify(s => s.PushAsync("GuidePage", null), Times.Once);
    }
}