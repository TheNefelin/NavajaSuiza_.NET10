using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class TaskEditorViewModelTests
{
    private readonly Mock<ITaskGroupsRepository> _repositoryMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<ILogger<TaskEditorViewModel>> _loggerMock = new();

    private TaskEditorViewModel CreateSut(TaskGroupListItem? parameter = null)
    {
        _languageServiceMock.Setup(s => s.GetString("NotesCreateTaskText")).Returns("Crear Tareas");
        _languageServiceMock.Setup(s => s.GetString("NotesEditTaskTitleText")).Returns("Editar tarea");

        _navigationServiceMock.Setup(n => n.TakeNavigationParameter()).Returns(parameter);

        return new TaskEditorViewModel(
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
        Assert.Equal("Crear Tareas", vm.PageTitle);
        Assert.Empty(vm.TaskItems);
        Assert.Equal(TaskImportance.Low, vm.SelectedImportance);
    }

    [Fact]
    public void Initialize_WithParameter_LoadsGroupAndItems()
    {
        var vm = CreateSut(new TaskGroupListItem
        {
            Id = 7,
            Title = "Grupo",
            CreatedAt = DateTime.UtcNow,
            Items = [new TaskItemListItem(new TaskItem { Id = 42, Title = "Primera", Importance = TaskImportance.Medium })]
        });
        vm.Initialize();

        Assert.True(vm.IsEditing);
        Assert.Equal("Editar tarea", vm.PageTitle);
        Assert.Equal("Grupo", vm.TitleText);
        var item = Assert.Single(vm.TaskItems);
        Assert.Equal(42, item.Id);
        Assert.Equal("Primera", item.Title);
        Assert.Equal(TaskImportance.Medium, item.Importance);
    }

    [Fact]
    public void AddTask_WithTitle_AddsItemWithSelectedImportanceAndClearsInput()
    {
        var vm = CreateSut();
        vm.Initialize();
        vm.NewTaskTitle = "Comprar leche";
        vm.SelectedImportance = TaskImportance.High;

        vm.AddTaskCommand.Execute(null);

        var item = Assert.Single(vm.TaskItems);
        Assert.Equal("Comprar leche", item.Title);
        Assert.Equal(TaskImportance.High, item.Importance);
        Assert.False(item.IsCompleted);
        Assert.Equal(string.Empty, vm.NewTaskTitle);
    }

    [Fact]
    public void AddTask_WithBlankTitle_DoesNotAddItem()
    {
        var vm = CreateSut();
        vm.Initialize();
        vm.NewTaskTitle = "   ";

        vm.AddTaskCommand.Execute(null);

        Assert.Empty(vm.TaskItems);
    }

    [Fact]
    public async Task DeleteTask_WhenConfirmed_RemovesItem()
    {
        _navigationServiceMock.Setup(n => n.DisplayAlertConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        var vm = CreateSut();
        vm.Initialize();
        vm.NewTaskTitle = "Tarea";
        vm.AddTaskCommand.Execute(null);
        var item = Assert.Single(vm.TaskItems);

        await vm.DeleteTaskCommand.ExecuteAsync(item);

        Assert.Empty(vm.TaskItems);
    }

    [Fact]
    public async Task DeleteTask_WhenCancelled_KeepsItem()
    {
        _navigationServiceMock.Setup(n => n.DisplayAlertConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        var vm = CreateSut();
        vm.Initialize();
        vm.NewTaskTitle = "Tarea";
        vm.AddTaskCommand.Execute(null);
        var item = Assert.Single(vm.TaskItems);

        await vm.DeleteTaskCommand.ExecuteAsync(item);

        Assert.Single(vm.TaskItems);
    }

    [Fact]
    public async Task Save_WithEmptyTitle_DoesNotCallRepository()
    {
        var vm = CreateSut();
        vm.Initialize();

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<TaskGroup>()), Times.Never);
    }

    [Fact]
    public async Task Save_WithEmptyTitle_ShowsValidationAlert()
    {
        var vm = CreateSut();
        vm.Initialize();

        await vm.SaveCommand.ExecuteAsync(null);

        _navigationServiceMock.Verify(n => n.DisplayAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Save_NewGroup_PersistsGroupWithItemsAndPops()
    {
        var vm = CreateSut();
        vm.Initialize();
        vm.TitleText = "Grupo";
        vm.NewTaskTitle = "Comprar leche";
        vm.SelectedImportance = TaskImportance.High;
        vm.AddTaskCommand.Execute(null);

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.Is<TaskGroup>(g =>
            g.Id == 0 && g.Title == "Grupo" && g.CreatedAt != default
            && g.Items.Count == 1
            && g.Items[0].Title == "Comprar leche"
            && g.Items[0].Importance == TaskImportance.High)), Times.Once);
        _navigationServiceMock.Verify(n => n.PopAsync(), Times.Once);
    }

    [Fact]
    public async Task Save_ExistingGroup_UpdatesWithId()
    {
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var vm = CreateSut(new TaskGroupListItem { Id = 7, Title = "Antes", CreatedAt = createdAt });
        vm.Initialize();
        vm.TitleText = "Después";

        await vm.SaveCommand.ExecuteAsync(null);

        _repositoryMock.Verify(r => r.SaveAsync(It.Is<TaskGroup>(g =>
            g.Id == 7 && g.Title == "Después" && g.CreatedAt == createdAt)), Times.Once);
    }
}
