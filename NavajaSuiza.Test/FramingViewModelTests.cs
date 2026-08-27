using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class FramingViewModelTests
{
    private readonly Mock<ILogger<FramingViewModel>> _loggerMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<IImagePickerService> _imagePickerServiceMock = new();

    private FramingViewModel CreateSut()
    {
        _languageServiceMock.Setup(s => s.GetString(It.IsAny<string>())).Returns("Test");
        return new FramingViewModel(_loggerMock.Object, _languageServiceMock.Object, _imagePickerServiceMock.Object);
    }

    [Fact]
    public void DefaultAspectRatio_Is11()
    {
        var vm = CreateSut();
        Assert.Equal(AppConstants.Framing.DefaultAspectRatio, vm.AspectRatio);
    }

    [Fact]
    public void DefaultAspectMode_IsAspectFill()
    {
        var vm = CreateSut();
        Assert.Equal(AppConstants.Framing.DefaultAspectMode, vm.AspectMode);
    }

    [Fact]
    public void DefaultCanvasBackground_IsBlur()
    {
        var vm = CreateSut();
        Assert.Equal(AppConstants.Framing.DefaultCanvasBackground, vm.CanvasBackground);
    }

    [Fact]
    public void DefaultBlurIntensity_Is13()
    {
        var vm = CreateSut();
        Assert.Equal(AppConstants.Framing.DefaultBlurIntensity, vm.BlurIntensity);
    }

    [Fact]
    public void SetAspectRatio_UpdatesRatio()
    {
        var vm = CreateSut();
        vm.SetAspectRatioCommand.Execute("4:5");
        Assert.Equal("4:5", vm.AspectRatio);
    }

    [Fact]
    public void SetAspectRatio_UpdatesHeight()
    {
        var vm = CreateSut();
        vm.SetCanvasWidth(100);
        vm.SetAspectRatioCommand.Execute("4:5");
        Assert.Equal(125, vm.CanvasHeight);
    }

    [Fact]
    public void SetAspectMode_UpdatesMode()
    {
        var vm = CreateSut();
        vm.SetAspectModeCommand.Execute("AspectFit");
        Assert.Equal("AspectFit", vm.AspectMode);
    }

    [Fact]
    public void SetCanvasBackground_WhenBlur_SetsTransparent()
    {
        var vm = CreateSut();
        vm.SetCanvasBackgroundCommand.Execute(AppConstants.Framing.DefaultCanvasBackground);
        Assert.Equal("Transparent", vm.CanvasBackgroundColor);
    }

    [Fact]
    public void SetCanvasBackground_WhenColor_SetsColor()
    {
        var vm = CreateSut();
        vm.SetCanvasBackgroundCommand.Execute("Red");
        Assert.Equal("Red", vm.CanvasBackgroundColor);
        Assert.Equal(0.0, vm.CanvasBackgroundOpacity);
    }

    [Fact]
    public void SetCanvasWidth_UpdatesWidthAndHeight()
    {
        var vm = CreateSut();
        vm.SetCanvasWidth(200);
        Assert.Equal(200, vm.CanvasWidth);
        Assert.Equal(200, vm.CanvasHeight);
    }

    [Fact]
    public async Task LoadImage_CallsPickerService()
    {
        _imagePickerServiceMock.Setup(s => s.PickImageAsync(It.IsAny<string>())).ReturnsAsync("/path/image.png");
        var vm = CreateSut();

        await vm.LoadImageCommand.ExecuteAsync(null);

        Assert.Equal("/path/image.png", vm.LoadedImage);
    }

    [Fact]
    public async Task LoadImage_WhenCancelled_SetsNull()
    {
        _imagePickerServiceMock.Setup(s => s.PickImageAsync(It.IsAny<string>())).ReturnsAsync((string?)null);
        var vm = CreateSut();

        await vm.LoadImageCommand.ExecuteAsync(null);

        Assert.Null(vm.LoadedImage);
    }
}
