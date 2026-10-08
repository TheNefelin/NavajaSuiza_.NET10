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
    private readonly Mock<IReminderScheduler> _reminderSchedulerMock = new();
    private readonly Mock<ILogger<NoteEditorViewModel>> _loggerMock = new();

    private NoteEditorViewModel CreateSut(NoteListItem? parameter = null)
    {
        _languageServiceMock.Setup(s => s.GetString("NotesCreateNoteText")).Returns("Crear Nota");
        _languageServiceMock.Setup(s => s.GetString("NotesEditTitleText")).Returns("Editar nota");
        _languageServiceMock.Setup(s => s.GetString("NotesReminderNotificationBodyText")).Returns("Recordatorio de tu nota");

        _navigationServiceMock.Setup(n => n.TakeNavigationParameter()).Returns(parameter);

        return new NoteEditorViewModel(
            _repositoryMock.Object,
            _navigationServiceMock.Object,
            _languageServiceMock.Object,
            _reminderSchedulerMock.Object,
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
        Assert.Equal("Crear Nota", vm.PageTitle);
    }

    [Fact]
    public void Initialize_WithParameter_LoadsNoteIntoEditor()
    {
        var vm = CreateSut(new NoteListItem { Id = 7, Title = "Antes", Content = "Texto" });
        vm.Initialize();

        Assert.True(vm.IsEditing);
        Assert.Equal("Editar nota", vm.PageTitle);
        Assert.Equal("Antes", vm.TitleText);
        Assert.Equal("Texto", vm.ContentText);
    }

    [Fact]
    public void Initialize_WithParameterWithReminder_RestoresReminderControls()
    {
        var reminderAt = new DateTime(2026, 10, 15, 9, 30, 0);
        var vm = CreateSut(new NoteListItem { Id = 9, Title = "Con recordatorio", Content = "Texto", ReminderAt = reminderAt });
        vm.Initialize();

        Assert.True(vm.HasReminder);
        Assert.Equal(reminderAt.Date, vm.ReminderDate);
        Assert.Equal(reminderAt.TimeOfDay, vm.ReminderTime);
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
    public async Task Save_WithEmptyTitleAndContent_ShowsValidationAlert()
    {
        var vm = CreateSut();
        vm.Initialize();

        await vm.SaveCommand.ExecuteAsync(null);

        _navigationServiceMock.Verify(n => n.DisplayAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
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
            n.Id == 0 && n.Title == "Título" && n.Content == "Contenido"
            && n.CreatedAt != default)), Times.Once);
        _navigationServiceMock.Verify(n => n.PopAsync(), Times.Once);
    }

    [Fact]
    public async Task Save_ExistingNote_UpdatesInPlaceAndPreservesCreatedAt()
    {
        var createdAt = DateTime.UtcNow.AddDays(-3);
        var vm = CreateSut(new NoteListItem { Id = 7, Title = "Antes", Content = "Texto", CreatedAt = createdAt });
        vm.Initialize();
        vm.TitleText = "Después";
        vm.ContentText = "Texto nuevo";

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.Is<Note>(n =>
            n.Id == 7 && n.Title == "Después" && n.CreatedAt == createdAt)), Times.Once);
        _navigationServiceMock.Verify(n => n.PopAsync(), Times.Once);
    }

    [Fact]
    public async Task Save_WithFutureReminder_SchedulesNotification()
    {
        _reminderSchedulerMock.Setup(r => r.RequestPermissionAsync()).ReturnsAsync(true);
        var future = DateTime.Now.AddDays(1);
        var reminderAt = future.Date.Add(future.TimeOfDay);
        var vm = CreateSut();
        vm.Initialize();
        vm.TitleText = "Título";
        vm.ContentText = "Contenido";
        vm.HasReminder = true;
        vm.ReminderDate = future.Date;
        vm.ReminderTime = future.TimeOfDay;

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.Is<Note>(n => n.ReminderAt == reminderAt)), Times.Once);
        _reminderSchedulerMock.Verify(r => r.ScheduleAsync(ReminderKind.Note, 0, "Título", "Recordatorio de tu nota", reminderAt), Times.Once);
        _reminderSchedulerMock.Verify(r => r.CancelAsync(ReminderKind.Note, It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Save_WithPastReminder_ShowsAlertAndDoesNotSchedule()
    {
        var past = DateTime.Now.AddMinutes(-5);
        var vm = CreateSut();
        vm.Initialize();
        vm.TitleText = "Título";
        vm.ContentText = "Contenido";
        vm.HasReminder = true;
        vm.ReminderDate = past.Date;
        vm.ReminderTime = past.TimeOfDay;

        await vm.SaveCommand.ExecuteAsync(null);

        _navigationServiceMock.Verify(n => n.DisplayAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Note>()), Times.Never);
        _reminderSchedulerMock.Verify(r => r.ScheduleAsync(
            It.IsAny<ReminderKind>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Save_WithoutReminder_CancelsExisting()
    {
        var vm = CreateSut();
        vm.Initialize();
        vm.TitleText = "Título";
        vm.ContentText = "Contenido";

        await vm.SaveCommand.ExecuteAsync(null);

        _reminderSchedulerMock.Verify(r => r.CancelAsync(ReminderKind.Note, 0), Times.Once);
    }

    [Fact]
    public async Task Save_WhenPermissionDenied_ShowsAlertAndDoesNotSchedule()
    {
        _reminderSchedulerMock.Setup(r => r.RequestPermissionAsync()).ReturnsAsync(false);
        var future = DateTime.Now.AddDays(1);
        var vm = CreateSut();
        vm.Initialize();
        vm.TitleText = "Título";
        vm.ContentText = "Contenido";
        vm.HasReminder = true;
        vm.ReminderDate = future.Date;
        vm.ReminderTime = future.TimeOfDay;

        await vm.SaveCommand.ExecuteAsync(null);

        _navigationServiceMock.Verify(n => n.DisplayAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Note>()), Times.Never);
        _reminderSchedulerMock.Verify(r => r.ScheduleAsync(
            It.IsAny<ReminderKind>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }
}
