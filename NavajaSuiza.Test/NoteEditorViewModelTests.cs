using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class NoteEditorViewModelTests
{
    private readonly Mock<INotesRepository> _repositoryMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<ILogger<NoteEditorViewModel>> _loggerMock = new();

    private NoteEditorViewModel CreateSut(bool withParameter = false)
    {
        _languageServiceMock.Setup(s => s.GetString("NotesNewTitleText")).Returns("Nueva nota");
        _languageServiceMock.Setup(s => s.GetString("NotesEditTitleText")).Returns("Editar nota");

        if (withParameter)
        {
            _navigationServiceMock.Setup(n => n.TakeNavigationParameter())
                .Returns(new Note { Id = 7, Title = "Antes", Content = "Texto" });
        }

        return new NoteEditorViewModel(
            _repositoryMock.Object,
            _navigationServiceMock.Object,
            _languageServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void Initialize_WithoutParameter_EntersCreateMode()
    {
        var vm = CreateSut();
        vm.Initialize();

        Assert.False(vm.IsEditing);
        Assert.Equal(string.Empty, vm.TitleText);
        Assert.Equal(string.Empty, vm.ContentText);
        Assert.Equal("Nueva nota", vm.PageTitle);
    }

    [Fact]
    public void Initialize_WithParameter_LoadsNoteIntoEditor()
    {
        var vm = CreateSut(withParameter: true);
        vm.Initialize();

        Assert.True(vm.IsEditing);
        Assert.Equal("Editar nota", vm.PageTitle);
        Assert.Equal("Antes", vm.TitleText);
        Assert.Equal("Texto", vm.ContentText);
    }

    [Fact]
    public async Task Save_WithEmptyTitleAndContent_DoesNotCallRepository()
    {
        var vm = CreateSut();
        vm.Initialize();

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Note>()), Times.Never);
    }

    [Fact]
    public async Task Save_NewNote_InsertsAndPops()
    {
        var vm = CreateSut();
        vm.Initialize();
        vm.TitleText = "Título";
        vm.ContentText = "Contenido";

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.Is<Note>(n =>
            n.Id == 0 && n.Title == "Título" && n.Content == "Contenido" && n.CreatedAt != default)), Times.Once);
        _navigationServiceMock.Verify(n => n.PopAsync(), Times.Once);
    }

    [Fact]
    public async Task Save_ExistingNote_UpdatesInPlaceAndPops()
    {
        var vm = CreateSut(withParameter: true);
        vm.Initialize();
        vm.TitleText = "Después";
        vm.ContentText = "Texto nuevo";

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.Is<Note>(n => n.Id == 7 && n.Title == "Después")), Times.Once);
        _navigationServiceMock.Verify(n => n.PopAsync(), Times.Once);
    }
}