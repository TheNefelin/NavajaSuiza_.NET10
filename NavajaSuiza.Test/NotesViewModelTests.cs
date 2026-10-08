using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class NotesViewModelTests
{
    private readonly Mock<INotesRepository> _repositoryMock = new();
    private readonly Mock<ITaskGroupsRepository> _taskGroupsRepositoryMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<IReminderScheduler> _reminderSchedulerMock = new();
    private readonly Mock<ILogger<NotesViewModel>> _loggerMock = new();

    public NotesViewModelTests()
    {
        _languageServiceMock.Setup(s => s.GetString(It.IsAny<string>())).Returns("test");
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Note>());
        _taskGroupsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskGroup>());
    }

    private NotesViewModel CreateSut()
    {
        return new NotesViewModel(
            _repositoryMock.Object,
            _taskGroupsRepositoryMock.Object,
            _navigationServiceMock.Object,
            _languageServiceMock.Object,
            _reminderSchedulerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task OnPageAppearing_LoadsNotesSortedByCreatedAtDescending()
    {
        var older = new Note { Id = 1, Title = "Old", CreatedAt = DateTime.UtcNow.AddDays(-2) };
        var newer = new Note { Id = 2, Title = "New", CreatedAt = DateTime.UtcNow };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([older, newer]);

        var vm = CreateSut();
        await vm.OnPageAppearingAsync();

        Assert.True(vm.HasNotes);
        Assert.Equal([2, 1], vm.Sections.SelectMany(s => s).Select(n => n.Id).ToArray());
    }

    [Fact]
    public async Task OnPageAppearing_SeparatesTasksAndNotesSections()
    {
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([
            new Note { Id = 1, Title = "Nota", CreatedAt = DateTime.UtcNow }
        ]);
        _taskGroupsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([
            new TaskGroup { Id = 2, Title = "Tarea", CreatedAt = DateTime.UtcNow }
        ]);

        var vm = CreateSut();
        _languageServiceMock.Setup(s => s.GetString("NotesSectionTasksText")).Returns("Tareas");
        _languageServiceMock.Setup(s => s.GetString("NotesSectionNotesText")).Returns("Notas");

        await vm.OnPageAppearingAsync();

        Assert.Equal(2, vm.Sections.Count);
        Assert.Equal("Tareas", vm.Sections[0].Name);
        Assert.IsType<TaskGroupListItem>(Assert.Single(vm.Sections[0]));
        Assert.Equal(2, vm.Sections[0][0].Id);
        Assert.Equal("Notas", vm.Sections[1].Name);
        Assert.IsType<NoteListItem>(Assert.Single(vm.Sections[1]));
        Assert.Equal(1, vm.Sections[1][0].Id);
    }

    [Fact]
    public async Task DeleteNote_WhenConfirmed_DeletesAndReloads()
    {
        var note = new Note { Id = 3, Title = "Borrar", CreatedAt = DateTime.UtcNow };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([note]);
        _navigationServiceMock.Setup(n => n.DisplayAlertConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var vm = CreateSut();
        await vm.OnPageAppearingAsync();
        var item = (NoteListItem)vm.Sections.SelectMany(s => s).Single();

        await vm.DeleteNoteCommand.ExecuteAsync(item);

        _repositoryMock.Verify(r => r.DeleteAsync(3), Times.Once);
    }

    [Fact]
    public async Task DeleteNote_WhenConfirmed_CancelsReminder()
    {
        var note = new Note { Id = 3, Title = "Borrar", CreatedAt = DateTime.UtcNow };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([note]);
        _navigationServiceMock.Setup(n => n.DisplayAlertConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var vm = CreateSut();
        await vm.OnPageAppearingAsync();
        var item = (NoteListItem)vm.Sections.SelectMany(s => s).Single();

        await vm.DeleteNoteCommand.ExecuteAsync(item);

        _reminderSchedulerMock.Verify(r => r.CancelAsync(3), Times.Once);
    }

    [Fact]
    public async Task DeleteNote_WhenCancelled_DoesNotDelete()
    {
        var note = new Note { Id = 3, Title = "Borrar", CreatedAt = DateTime.UtcNow };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([note]);
        _navigationServiceMock.Setup(n => n.DisplayAlertConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var vm = CreateSut();
        await vm.OnPageAppearingAsync();
        var item = (NoteListItem)vm.Sections.SelectMany(s => s).Single();

        await vm.DeleteNoteCommand.ExecuteAsync(item);

        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTask_WhenConfirmed_DeletesTaskGroupAndReloads()
    {
        _taskGroupsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([
            new TaskGroup { Id = 9, Title = "Grupo", CreatedAt = DateTime.UtcNow }
        ]);
        _navigationServiceMock.Setup(n => n.DisplayAlertConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var vm = CreateSut();
        await vm.OnPageAppearingAsync();
        var item = (TaskGroupListItem)vm.Sections.SelectMany(s => s).Single();

        await vm.DeleteTaskCommand.ExecuteAsync(item);

        _taskGroupsRepositoryMock.Verify(r => r.DeleteAsync(9), Times.Once);
    }

    [Fact]
    public async Task SearchText_FiltersNotesByContent()
    {
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([
            new Note { Id = 1, Title = "Compra", Content = "Leche", CreatedAt = DateTime.UtcNow },
            new Note { Id = 2, Title = "Otra", Content = "Estudiar", CreatedAt = DateTime.UtcNow }
        ]);

        var vm = CreateSut();
        await vm.OnPageAppearingAsync();

        vm.SearchText = "leche";

        var filtered = vm.Sections.SelectMany(s => s).ToList();
        Assert.Single(filtered);
        Assert.Equal(1, filtered[0].Id);
    }

    [Fact]
    public async Task SearchText_FiltersTaskGroupsByItemTitle()
    {
        _taskGroupsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([
            new TaskGroup
            {
                Id = 5,
                Title = "Semana",
                CreatedAt = DateTime.UtcNow,
                Items = [new TaskItem { Title = "Comprar leche" }]
            }
        ]);

        var vm = CreateSut();
        await vm.OnPageAppearingAsync();

        vm.SearchText = "leche";

        Assert.Single(vm.Sections.SelectMany(s => s));
    }

    [Fact]
    public async Task NavigateToNewNote_NavigatesWithoutParameter()
    {
        var vm = CreateSut();

        await vm.NavigateToNewNoteCommand.ExecuteAsync(null);

        _navigationServiceMock.Verify(n => n.PushAsync("NoteEditorPage", It.Is<object?>(p => p == null)), Times.Once);
    }

    [Fact]
    public async Task NavigateToEditNote_NavigatesWithSelectedItem()
    {
        var item = new NoteListItem { Id = 5, Title = "TituloX", Content = "ContenidoX", CreatedAt = DateTime.UtcNow };
        var vm = CreateSut();

        await vm.NavigateToEditNoteCommand.ExecuteAsync(item);

        _navigationServiceMock.Verify(n => n.PushAsync("NoteEditorPage", item), Times.Once);
    }

    [Fact]
    public async Task NavigateToNewTask_NavigatesToTaskEditorPageWithoutParameter()
    {
        var vm = CreateSut();

        await vm.NavigateToNewTaskCommand.ExecuteAsync(null);

        _navigationServiceMock.Verify(n => n.PushAsync("TaskEditorPage", It.Is<object?>(p => p == null)), Times.Once);
    }

    [Fact]
    public async Task NavigateToEditTask_NavigatesToTaskEditorPage()
    {
        var item = new TaskGroupListItem { Id = 8, Title = "Tarea", CreatedAt = DateTime.UtcNow };
        var vm = CreateSut();

        await vm.NavigateToEditTaskCommand.ExecuteAsync(item);

        _navigationServiceMock.Verify(n => n.PushAsync("TaskEditorPage", item), Times.Once);
    }
}
