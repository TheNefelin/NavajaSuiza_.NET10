using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.ViewModels;

public partial class NotesViewModel : BaseViewModel
{
    private readonly INotesRepository _notesRepository;
    private readonly ITaskGroupsRepository _taskGroupsRepository;
    private readonly INavigationService _navigationService;
    private readonly ILanguageService _languageService;
    private readonly IReminderScheduler _reminderScheduler;
    private readonly ILogger<NotesViewModel> _logger;

    private List<Note> _allNotes = [];
    private List<TaskGroup> _allTaskGroups = [];

    public ObservableCollection<NotesSection> Sections { get; } = new();

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasNotes { get; set; }

    public NotesViewModel(
        INotesRepository notesRepository,
        ITaskGroupsRepository taskGroupsRepository,
        INavigationService navigationService,
        ILanguageService languageService,
        IReminderScheduler reminderScheduler,
        ILogger<NotesViewModel> logger)
    {
        _notesRepository = notesRepository;
        _taskGroupsRepository = taskGroupsRepository;
        _navigationService = navigationService;
        _languageService = languageService;
        _reminderScheduler = reminderScheduler;
        _logger = logger;
    }

    public async Task OnPageAppearingAsync()
    {
        await LoadNotesAsync();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    [RelayCommand]
    private async Task NavigateToNewNoteAsync()
    {
        await _navigationService.PushAsync("NoteEditorPage");
    }

    [RelayCommand]
    private async Task NavigateToNewTaskAsync()
    {
        await _navigationService.PushAsync("TaskEditorPage");
    }

    [RelayCommand]
    private async Task NavigateToEditNoteAsync(NoteListItem note)
    {
        await _navigationService.PushAsync("NoteEditorPage", note);
    }

    [RelayCommand]
    private async Task NavigateToEditTaskAsync(TaskGroupListItem task)
    {
        await _navigationService.PushAsync("TaskEditorPage", task);
    }

    [RelayCommand]
    private async Task DeleteNoteAsync(NoteListItem note)
    {
        if (note is null || !await ConfirmDeleteAsync())
            return;

        try
        {
            IsBusy = true;
            await _notesRepository.DeleteAsync(note.Id);
            await CancelReminderAsync(note.Id);
            await LoadNotesAsync();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex, "NotesErrorDeleteText", "Error al eliminar la nota");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteTaskAsync(TaskGroupListItem task)
    {
        if (task is null || !await ConfirmDeleteAsync())
            return;

        try
        {
            IsBusy = true;
            await _taskGroupsRepository.DeleteAsync(task.Id);
            await LoadNotesAsync();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex, "NotesErrorDeleteText", "Error al eliminar el grupo de tareas");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task<bool> ConfirmDeleteAsync() =>
        _navigationService.DisplayAlertConfirmAsync(
            _languageService.GetString("NotesDeleteTitleText"),
            _languageService.GetString("NotesDeleteConfirmationText"),
            _languageService.GetString("CommonYesText"),
            _languageService.GetString("CommonNoText"));

    private async Task CancelReminderAsync(int noteId)
    {
        try
        {
            await _reminderScheduler.CancelAsync(noteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar el recordatorio de la nota {NoteId}", noteId);
        }
    }

    private async Task HandleErrorAsync(Exception ex, string messageKey, string logMessage)
    {
        _logger.LogError(ex, logMessage);
        await _navigationService.DisplayAlertAsync(
            _languageService.GetString("NotesErrorTitleText"),
            _languageService.GetString(messageKey),
            _languageService.GetString("CommonOkText"));
    }

    private async Task LoadNotesAsync()
    {
        try
        {
            IsBusy = true;
            _allNotes = await _notesRepository.GetAllAsync();
            _allTaskGroups = await _taskGroupsRepository.GetAllAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex, "NotesErrorLoadText", "Error al cargar las notas");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var search = SearchText?.Trim() ?? string.Empty;

        var tasks = _allTaskGroups
            .Where(group => Matches(search, group.Title, group.Items.Select(item => item.Title)))
            .OrderByDescending(group => group.CreatedAt)
            .Select(CreateTaskListItem)
            .ToList();

        var notes = _allNotes
            .Where(note => Matches(search, note.Title, note.Content))
            .OrderByDescending(note => note.CreatedAt)
            .Select(note => (NotesListItem)new NoteListItem
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                ReminderAt = note.ReminderAt
            })
            .ToList();

        Sections.Clear();

        if (tasks.Count > 0)
            Sections.Add(new NotesSection(_languageService.GetString("NotesSectionTasksText"), tasks));

        if (notes.Count > 0)
            Sections.Add(new NotesSection(_languageService.GetString("NotesSectionNotesText"), notes));

        HasNotes = Sections.Count > 0;
    }

    private NotesListItem CreateTaskListItem(TaskGroup group) => new TaskGroupListItem
    {
        Id = group.Id,
        Title = group.Title,
        CreatedAt = group.CreatedAt,
        Items = group.Items
            .Select(item => new TaskItemListItem(item, () => _ = PersistTaskGroupAsync(group)))
            .ToList()
    };

    private async Task PersistTaskGroupAsync(TaskGroup group)
    {
        try
        {
            await _taskGroupsRepository.SaveAsync(group);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex, "NotesErrorSaveText", "Error al guardar el grupo de tareas");
        }
    }

    private static bool Matches(string search, string title, string content) =>
        string.IsNullOrWhiteSpace(search)
        || title.Contains(search, StringComparison.OrdinalIgnoreCase)
        || content.Contains(search, StringComparison.OrdinalIgnoreCase);

    private static bool Matches(string search, string title, IEnumerable<string> contents) =>
        string.IsNullOrWhiteSpace(search)
        || title.Contains(search, StringComparison.OrdinalIgnoreCase)
        || contents.Any(content => content.Contains(search, StringComparison.OrdinalIgnoreCase));
}
