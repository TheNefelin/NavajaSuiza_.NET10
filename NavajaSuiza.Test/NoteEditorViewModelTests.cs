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

    private NoteEditorViewModel CreateSut(Note? parameter = null)
    {
        _languageServiceMock.Setup(s => s.GetString("NotesCreateNoteTitleText")).Returns("Crear Nota");
        _languageServiceMock.Setup(s => s.GetString("NotesEditTitleText")).Returns("Editar nota");
        _languageServiceMock.Setup(s => s.GetString("NotesCreateTaskTitleText")).Returns("Crear Tareas");
        _languageServiceMock.Setup(s => s.GetString("NotesEditTaskTitleText")).Returns("Editar tarea");

        if (parameter is not null)
        {
            _navigationServiceMock.Setup(n => n.TakeNavigationParameter()).Returns(parameter);
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
        vm.Initialize(isTask: false);

        Assert.False(vm.IsEditing);
        Assert.Equal(string.Empty, vm.TitleText);
        Assert.Equal(string.Empty, vm.ContentText);
        Assert.Equal("Crear Nota", vm.PageTitle);
    }

    [Fact]
    public void Initialize_AsTaskWithoutParameter_UsesNewTaskTitle()
    {
        var vm = CreateSut();
        vm.Initialize(isTask: true);

        Assert.False(vm.IsEditing);
        Assert.Equal("Crear Tareas", vm.PageTitle);
    }

    [Fact]
    public void Initialize_WithParameter_LoadsNoteIntoEditor()
    {
        var vm = CreateSut(new Note { Id = 7, Title = "Antes", Content = "Texto" });
        vm.Initialize(isTask: false);

        Assert.True(vm.IsEditing);
        Assert.Equal("Editar nota", vm.PageTitle);
        Assert.Equal("Antes", vm.TitleText);
        Assert.Equal("Texto", vm.ContentText);
    }

    [Fact]
    public void Initialize_WithTaskParameter_UsesEditTaskTitle()
    {
        var vm = CreateSut(new Note { Id = 7, Title = "Tarea", Content = "Texto", IsTask = true });
        vm.Initialize(isTask: true);

        Assert.True(vm.IsEditing);
        Assert.Equal("Editar tarea", vm.PageTitle);
    }

    [Fact]
    public async Task Save_WithEmptyTitleAndContent_DoesNotCallRepository()
    {
        var vm = CreateSut();
        vm.Initialize(isTask: false);

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Note>()), Times.Never);
    }

    [Fact]
    public async Task Save_NewNote_InsertsWithoutIsTaskAndPops()
    {
        var vm = CreateSut();
        vm.Initialize(isTask: false);
        vm.TitleText = "Título";
        vm.ContentText = "Contenido";

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.Is<Note>(n =>
            n.Id == 0 && n.Title == "Título" && n.Content == "Contenido"
            && n.CreatedAt != default && !n.IsTask)), Times.Once);
        _navigationServiceMock.Verify(n => n.PopAsync(), Times.Once);
    }

    [Fact]
    public async Task Save_NewTask_InsertsWithIsTask()
    {
        var vm = CreateSut();
        vm.Initialize(isTask: true);
        vm.TitleText = "Tarea";
        vm.ContentText = "Contenido";

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.Is<Note>(n =>
            n.Id == 0 && n.Title == "Tarea" && n.IsTask)), Times.Once);
    }

    [Fact]
    public async Task Save_ExistingNote_UpdatesInPlaceAndPops()
    {
        var vm = CreateSut(new Note { Id = 7, Title = "Antes", Content = "Texto" });
        vm.Initialize(isTask: false);
        vm.TitleText = "Después";
        vm.ContentText = "Texto nuevo";

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.Is<Note>(n => n.Id == 7 && n.Title == "Después")), Times.Once);
        _navigationServiceMock.Verify(n => n.PopAsync(), Times.Once);
    }

    [Fact]
    public async Task Save_ExistingTask_PreservesIsTask()
    {
        var vm = CreateSut(new Note { Id = 7, Title = "Antes", Content = "Texto", IsTask = true });
        vm.Initialize(isTask: true);
        vm.TitleText = "Después";

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.Is<Note>(n => n.Id == 7 && n.IsTask)), Times.Once);
    }
}
