using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.ViewModels;

public partial class TaskEditorViewModel : BaseViewModel
{
    private readonly ITaskGroupsRepository _taskGroupsRepository;
    private readonly INavigationService _navigationService;
    private readonly ILanguageService _languageService;
    private readonly ILogger<TaskEditorViewModel> _logger;

    private int _editingId;
    private DateTime _editingCreatedAt;

    [ObservableProperty]
    public partial string TitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewTaskTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial TaskImportance SelectedImportance { get; set; } = TaskImportance.Low;

    [ObservableProperty]
    public partial ObservableCollection<TaskItem> TaskItems { get; set; } = new();

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial string PageTitle { get; set; } = string.Empty;

    public TaskEditorViewModel(
        ITaskGroupsRepository taskGroupsRepository,
        INavigationService navigationService,
        ILanguageService languageService,
        ILogger<TaskEditorViewModel> logger)
    {
        _taskGroupsRepository = taskGroupsRepository;
        _navigationService = navigationService;
        _languageService = languageService;
        _logger = logger;
    }

    public void Initialize()
    {
        var item = _navigationService.TakeNavigationParameter() as TaskGroupListItem;

        IsEditing = item is not null;
        _editingId = item?.Id ?? 0;
        _editingCreatedAt = item?.CreatedAt ?? default;
        TitleText = item?.Title ?? string.Empty;
        PageTitle = _languageService.GetString(IsEditing ? "NotesEditTaskTitleText" : "NotesCreateTaskText");

        NewTaskTitle = string.Empty;
        SelectedImportance = TaskImportance.Low;
        TaskItems.Clear();

        if (item is not null)
        {
            foreach (var task in item.Items)
                TaskItems.Add(new TaskItem
                {
                    Id = task.Id,
                    Title = task.Title,
                    IsCompleted = task.IsCompleted,
                    Importance = task.Importance
                });
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(TitleText))
        {
            await _navigationService.DisplayAlertAsync(
                _languageService.GetString("NotesValidationTitleText"),
                _languageService.GetString("NotesValidationTaskText"),
                _languageService.GetString("CommonOkText"));
            return;
        }

        try
        {
            IsBusy = true;

            var group = new TaskGroup
            {
                Id = _editingId,
                Title = TitleText.Trim(),
                CreatedAt = IsEditing ? _editingCreatedAt : DateTime.UtcNow,
                Items = TaskItems.ToList()
            };

            await _taskGroupsRepository.SaveAsync(group);

            await _navigationService.PopAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar el grupo de tareas");
            await _navigationService.DisplayAlertAsync(
                _languageService.GetString("NotesErrorTitleText"),
                _languageService.GetString("NotesErrorSaveText"),
                _languageService.GetString("CommonOkText"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddTask()
    {
        var title = NewTaskTitle.Trim();

        if (string.IsNullOrWhiteSpace(title))
            return;

        TaskItems.Add(new TaskItem
        {
            Title = title,
            Importance = SelectedImportance
        });

        NewTaskTitle = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteTaskAsync(TaskItem? taskItem)
    {
        if (taskItem is null)
            return;

        var confirmed = await _navigationService.DisplayAlertConfirmAsync(
            _languageService.GetString("NotesDeleteTitleText"),
            _languageService.GetString("NotesDeleteConfirmationText"),
            _languageService.GetString("CommonYesText"),
            _languageService.GetString("CommonNoText"));

        if (confirmed)
            TaskItems.Remove(taskItem);
    }
}
