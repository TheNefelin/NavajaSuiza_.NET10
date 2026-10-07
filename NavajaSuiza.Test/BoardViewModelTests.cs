using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class BoardViewModelTests
{
    private readonly Mock<ILogger<BoardViewModel>> _loggerMock = new();
    private readonly Mock<IBoardImageExporter> _exporterMock = new();

    private BoardViewModel CreateSut() => new(_loggerMock.Object, _exporterMock.Object);

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

    [Fact]
    public async Task SaveCommand_ExportsStrokesAndBoardColor()
    {
        _exporterMock.Setup(e => e.ExportAsync(
                It.IsAny<IReadOnlyList<BoardStroke>>(),
                It.IsAny<IReadOnlyList<BoardText>>(),
                It.IsAny<string>()))
            .ReturnsAsync(BoardExportResult.Saved);

        var vm = CreateSut();
        vm.SetBoardColorCommand.Execute("#17171B");
        vm.StartStroke(10f, 20f);
        vm.AddPoint(30f, 40f);

        await vm.SaveCommand.ExecuteAsync(null);

        _exporterMock.Verify(e => e.ExportAsync(
            It.Is<IReadOnlyList<BoardStroke>>(s => s.Count == 1 && s[0].Points.Count == 2),
            It.Is<IReadOnlyList<BoardText>>(t => t.Count == 0),
            "#17171B"), Times.Once);
        Assert.Equal(BoardExportResult.Saved, vm.LastExportResult);
    }

    [Fact]
    public async Task SaveCommand_OnEmptyBoard_DoesNotExportAndFlagsFailed()
    {
        _exporterMock.Setup(e => e.ExportAsync(
                It.IsAny<IReadOnlyList<BoardStroke>>(),
                It.IsAny<IReadOnlyList<BoardText>>(),
                It.IsAny<string>()))
            .ReturnsAsync(BoardExportResult.Saved);

        var vm = CreateSut();
        await vm.SaveCommand.ExecuteAsync(null);

        _exporterMock.Verify(e => e.ExportAsync(
            It.IsAny<IReadOnlyList<BoardStroke>>(),
            It.IsAny<IReadOnlyList<BoardText>>(),
            It.IsAny<string>()), Times.Never);
        Assert.Equal(BoardExportResult.Failed, vm.LastExportResult);
    }

    [Fact]
    public async Task SaveCommand_WhenExporterUnavailable_SetsNotAvailable()
    {
        _exporterMock.Setup(e => e.ExportAsync(
                It.IsAny<IReadOnlyList<BoardStroke>>(),
                It.IsAny<IReadOnlyList<BoardText>>(),
                It.IsAny<string>()))
            .ReturnsAsync(BoardExportResult.NotAvailable);

        var vm = CreateSut();
        vm.StartStroke(10f, 20f);

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal(BoardExportResult.NotAvailable, vm.LastExportResult);
    }

    [Fact]
    public async Task SaveCommand_WhenExporterFails_SetsFailed()
    {
        _exporterMock.Setup(e => e.ExportAsync(
                It.IsAny<IReadOnlyList<BoardStroke>>(),
                It.IsAny<IReadOnlyList<BoardText>>(),
                It.IsAny<string>()))
            .ReturnsAsync(BoardExportResult.Failed);

        var vm = CreateSut();
        vm.StartStroke(10f, 20f);

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal(BoardExportResult.Failed, vm.LastExportResult);
    }

    [Fact]
    public async Task SaveCommand_WithOnlyText_ExportsWithoutStrokes()
    {
        _exporterMock.Setup(e => e.ExportAsync(
                It.IsAny<IReadOnlyList<BoardStroke>>(),
                It.IsAny<IReadOnlyList<BoardText>>(),
                It.IsAny<string>()))
            .ReturnsAsync(BoardExportResult.Saved);

        var vm = CreateSut();
        vm.ToggleTextModeCommand.Execute(null);
        vm.AddText("Hola", 10f, 20f);

        await vm.SaveCommand.ExecuteAsync(null);

        _exporterMock.Verify(e => e.ExportAsync(
            It.Is<IReadOnlyList<BoardStroke>>(s => s.Count == 0),
            It.Is<IReadOnlyList<BoardText>>(t => t.Count == 1 && t[0].Content == "Hola"),
            It.IsAny<string>()), Times.Once);
        Assert.Equal(BoardExportResult.Saved, vm.LastExportResult);
    }

    [Fact]
    public void ToggleTextModeCommand_FlipsTextMode()
    {
        var vm = CreateSut();

        Assert.False(vm.IsTextModeActive);

        vm.ToggleTextModeCommand.Execute(null);
        Assert.True(vm.IsTextModeActive);

        vm.ToggleTextModeCommand.Execute(null);
        Assert.False(vm.IsTextModeActive);
    }

    [Fact]
    public void AddText_InTextMode_AddsTextWithPenColor()
    {
        var vm = CreateSut();
        vm.SelectColorCommand.Execute("#E53935");
        vm.ToggleTextModeCommand.Execute(null);

        var added = vm.AddText("Hola", 15f, 25f);

        var text = Assert.Single(vm.Texts);
        Assert.True(added);
        Assert.Equal("Hola", text.Content);
        Assert.Equal(15f, text.X);
        Assert.Equal(25f, text.Y);
        Assert.Equal("#E53935", text.ColorHex);
        Assert.Equal(BoardDefaults.FontSize, text.FontSize);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddText_WithBlankContent_DoesNothing(string content)
    {
        var vm = CreateSut();
        vm.ToggleTextModeCommand.Execute(null);

        var added = vm.AddText(content, 10f, 20f);

        Assert.False(added);
        Assert.Empty(vm.Texts);
    }

    [Fact]
    public void AddText_WhenTextModeIsOff_DoesNothing()
    {
        var vm = CreateSut();

        var added = vm.AddText("Hola", 10f, 20f);

        Assert.False(added);
        Assert.Empty(vm.Texts);
    }

    [Fact]
    public void StartStroke_InTextMode_DoesNotAddStroke()
    {
        var vm = CreateSut();
        vm.ToggleTextModeCommand.Execute(null);

        vm.StartStroke(10f, 20f);
        vm.AddPoint(30f, 40f);

        Assert.Empty(vm.Strokes);
    }

    [Fact]
    public void AddPoint_InTextMode_DoesNothing()
    {
        var vm = CreateSut();
        vm.StartStroke(10f, 20f);
        vm.ToggleTextModeCommand.Execute(null);

        vm.AddPoint(30f, 40f);

        Assert.Single(vm.Strokes);
        Assert.Single(vm.Strokes[0].Points);
    }

    [Fact]
    public void UndoCommand_AfterText_RemovesTheTextNotTheStroke()
    {
        var vm = CreateSut();
        vm.StartStroke(10f, 20f);
        vm.ToggleTextModeCommand.Execute(null);
        vm.AddText("Hola", 30f, 40f);

        vm.UndoCommand.Execute(null);

        Assert.Empty(vm.Texts);
        Assert.Single(vm.Strokes);
    }

    [Fact]
    public void UndoCommand_AfterStroke_RemovesTheStrokeNotTheText()
    {
        var vm = CreateSut();
        vm.ToggleTextModeCommand.Execute(null);
        vm.AddText("Hola", 30f, 40f);
        vm.ToggleTextModeCommand.Execute(null);
        vm.StartStroke(10f, 20f);

        vm.UndoCommand.Execute(null);

        Assert.Empty(vm.Strokes);
        Assert.Single(vm.Texts);
    }

    [Fact]
    public void ClearCommand_RemovesStrokesTextsAndHistory()
    {
        var vm = CreateSut();
        vm.StartStroke(10f, 20f);
        vm.ToggleTextModeCommand.Execute(null);
        vm.AddText("Hola", 30f, 40f);

        vm.ClearCommand.Execute(null);

        Assert.Empty(vm.Strokes);
        Assert.Empty(vm.Texts);

        // Si el historial no se limpiara, Undo todavia tendria entradas.
        vm.UndoCommand.Execute(null);
        Assert.Empty(vm.Strokes);
        Assert.Empty(vm.Texts);
    }

    [Fact]
    public void SetBoardColor_OnDarkBoardWithDarkText_SwitchesTextToReadableColor()
    {
        var vm = CreateSut();
        vm.SelectColorCommand.Execute("#1F1F1F");
        vm.ToggleTextModeCommand.Execute(null);
        vm.AddText("Hola", 10f, 20f);
        Assert.Equal("#1F1F1F", vm.Texts[0].ColorHex);

        vm.SetBoardColorCommand.Execute("#17171B");

        Assert.Equal("#FDD835", vm.Texts[0].ColorHex);
    }

    [Fact]
    public void SetBoardColor_KeepsTextColorWithEnoughContrast()
    {
        var vm = CreateSut();
        vm.SetBoardColorCommand.Execute("#17171B");
        vm.SelectColorCommand.Execute("#E53935");
        vm.ToggleTextModeCommand.Execute(null);
        vm.AddText("Hola", 10f, 20f);

        vm.SetBoardColorCommand.Execute("#17171B");

        Assert.Equal("#E53935", vm.Texts[0].ColorHex);
    }
}