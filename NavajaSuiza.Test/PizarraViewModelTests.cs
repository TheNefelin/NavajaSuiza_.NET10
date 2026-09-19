using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class PizarraViewModelTests
{
    private readonly Mock<ILogger<PizarraViewModel>> _loggerMock = new();

    private PizarraViewModel CreateSut() => new(_loggerMock.Object);

    [Fact]
    public void Strokes_StartsEmpty()
    {
        var vm = CreateSut();
        Assert.Empty(vm.Strokes);
    }

    [Fact]
    public void SelectedColor_DefaultsToRed()
    {
        var vm = CreateSut();
        Assert.Equal("#E53935", vm.SelectedColor);
    }

    [Fact]
    public void StrokeWidth_DefaultsTo4()
    {
        var vm = CreateSut();
        Assert.Equal(4d, vm.StrokeWidth);
    }

    [Fact]
    public void BoardColor_DefaultsToLightBoard()
    {
        var vm = CreateSut();
        Assert.Equal("#F2F2EE", vm.BoardColor);
    }

    [Fact]
    public void StartStroke_AddsStrokeWithCurrentColorAndWidth()
    {
        var vm = CreateSut();
        vm.SetColorCommand.Execute("#E53935");
        vm.StrokeWidth = 12d;

        vm.StartStroke(10f, 20f);

        var stroke = Assert.Single(vm.Strokes);
        Assert.Equal("#E53935", stroke.ColorHex);
        Assert.Equal(12f, stroke.Width);
        Assert.Single(stroke.Points);
    }

    [Fact]
    public void AddPoint_AppendsToCurrentStroke()
    {
        var vm = CreateSut();
        vm.StartStroke(10f, 20f);
        vm.AddPoint(30f, 40f);
        vm.AddPoint(50f, 60f);

        var stroke = Assert.Single(vm.Strokes);
        Assert.Equal(3, stroke.Points.Count);
        Assert.Equal(30f, stroke.Points[1].X);
        Assert.Equal(60f, stroke.Points[2].Y);
    }

    [Fact]
    public void AddPoint_WithNoStrokes_DoesNothing()
    {
        var vm = CreateSut();
        vm.AddPoint(10f, 20f);
        Assert.Empty(vm.Strokes);
    }

    [Fact]
    public void EndStroke_DoesNotThrow()
    {
        var vm = CreateSut();
        vm.StartStroke(10f, 20f);
        vm.EndStroke();
        Assert.Single(vm.Strokes);
    }

    [Fact]
    public void ClearCommand_RemovesAllStrokes()
    {
        var vm = CreateSut();
        vm.StartStroke(10f, 20f);
        vm.ClearCommand.Execute(null);
        Assert.Empty(vm.Strokes);
    }

    [Fact]
    public void UndoCommand_RemovesLastStroke()
    {
        var vm = CreateSut();
        vm.StartStroke(10f, 20f);
        vm.StartStroke(30f, 40f);

        vm.UndoCommand.Execute(null);

        var stroke = Assert.Single(vm.Strokes);
        Assert.Equal(new[] { 10f }, stroke.Points.Select(p => p.X));
    }

    [Fact]
    public void UndoCommand_OnEmptyCollection_DoesNothing()
    {
        var vm = CreateSut();
        vm.UndoCommand.Execute(null);
        Assert.Empty(vm.Strokes);
    }

    [Fact]
    public void SetColorCommand_UpdatesSelectedColor()
    {
        var vm = CreateSut();
        vm.SetColorCommand.Execute("#1E88E5");
        Assert.Equal("#1E88E5", vm.SelectedColor);
    }

    [Fact]
    public void OpenColorPickerCommand_SetsPickerVisible()
    {
        var vm = CreateSut();
        vm.OpenColorPickerCommand.Execute(null);
        Assert.True(vm.IsColorPickerVisible);
    }

    [Fact]
    public void CloseColorPickerCommand_ClearsPickerVisible()
    {
        var vm = CreateSut();
        vm.OpenColorPickerCommand.Execute(null);
        vm.CloseColorPickerCommand.Execute(null);
        Assert.False(vm.IsColorPickerVisible);
    }

    [Fact]
    public void SelectColorCommand_UpdatesColorAndClosesPicker()
    {
        var vm = CreateSut();
        vm.OpenColorPickerCommand.Execute(null);

        vm.SelectColorCommand.Execute("#43A047");

        Assert.Equal("#43A047", vm.SelectedColor);
        Assert.False(vm.IsColorPickerVisible);
    }

    [Fact]
    public void SetBoardColorCommand_UpdatesBoardColorAndFiresCanvasChanged()
    {
        var vm = CreateSut();
        var fired = 0;
        vm.CanvasChanged += () => fired++;

        vm.SetBoardColorCommand.Execute("#17171B");

        Assert.Equal("#17171B", vm.BoardColor);
        Assert.Equal(1, fired);
    }

    [Fact]
    public void SetBoardColor_OnDarkBoardWithDarkPen_SwitchesToLightPen()
    {
        var vm = CreateSut();
        vm.SetColorCommand.Execute("#1F1F1F");

        vm.SetBoardColorCommand.Execute("#17171B");

        Assert.Equal("#FDD835", vm.SelectedColor);
    }

    [Fact]
    public void SetBoardColor_OnLightBoardWithLightPen_SwitchesToDarkPen()
    {
        var vm = CreateSut();
        vm.SetColorCommand.Execute("#FFFFFF");

        vm.SetBoardColorCommand.Execute("#F2F2EE");

        Assert.Equal("#1F1F1F", vm.SelectedColor);
    }

    [Fact]
    public void SetBoardColor_OnDarkBoardWithVisiblePen_KeepsColor()
    {
        var vm = CreateSut();

        vm.SetBoardColorCommand.Execute("#17171B");

        Assert.Equal("#E53935", vm.SelectedColor);
    }

    [Fact]
    public void SetBoardColor_OnLightBoardWithVisiblePen_KeepsColor()
    {
        var vm = CreateSut();

        vm.SetBoardColorCommand.Execute("#F2F2EE");

        Assert.Equal("#E53935", vm.SelectedColor);
    }

    [Fact]
    public void StartStroke_AndAddPoint_FireCanvasChanged()
    {
        var vm = CreateSut();
        var fired = 0;
        vm.CanvasChanged += () => fired++;

        vm.StartStroke(10f, 20f);
        vm.AddPoint(30f, 40f);

        Assert.Equal(2, fired);
    }

    [Fact]
    public void AddPoint_WithNoStrokes_DoesNotFireCanvasChanged()
    {
        var vm = CreateSut();
        var fired = 0;
        vm.CanvasChanged += () => fired++;

        vm.AddPoint(10f, 20f);

        Assert.Equal(0, fired);
    }
}