using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.ViewModels;

public partial class NoteEditorViewModel : BaseViewModel
{
    private readonly INotesRepository _notesRepository;
    private readonly INavigationService _navigationService;
    private readonly ILanguageService _languageService;
    private readonly IReminderScheduler _reminderScheduler;
    private readonly ILogger<NoteEditorViewModel> _logger;

    private int _editingId;
    private DateTime _editingCreatedAt;

    [ObservableProperty]
    public partial string TitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ContentText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial string PageTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasReminder { get; set; }

    [ObservableProperty]
    public partial DateTime ReminderDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial TimeSpan ReminderTime { get; set; }

    public NoteEditorViewModel(
        INotesRepository notesRepository,
        INavigationService navigationService,
        ILanguageService languageService,
        IReminderScheduler reminderScheduler,
        ILogger<NoteEditorViewModel> logger)
    {
        _notesRepository = notesRepository;
        _navigationService = navigationService;
        _languageService = languageService;
        _reminderScheduler = reminderScheduler;
        _logger = logger;
    }

    public void Initialize()
    {
        var item = _navigationService.TakeNavigationParameter() as NoteListItem;

        IsEditing = item is not null;
        _editingId = item?.Id ?? 0;
        _editingCreatedAt = item?.CreatedAt ?? default;
        TitleText = item?.Title ?? string.Empty;
        ContentText = item?.Content ?? string.Empty;
        PageTitle = _languageService.GetString(IsEditing ? "NotesEditTitleText" : "NotesCreateNoteText");
        HasReminder = item?.ReminderAt is not null;
        ReminderDate = item?.ReminderAt is DateTime reminder ? reminder.Date : DateTime.Today;
        ReminderTime = item?.ReminderAt is DateTime reminderTime ? reminderTime.TimeOfDay : default;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(TitleText) && string.IsNullOrWhiteSpace(ContentText))
        {
            await _navigationService.DisplayAlertAsync(
                _languageService.GetString("NotesValidationTitleText"),
                _languageService.GetString("NotesValidationNoteText"),
                _languageService.GetString("CommonOkText"));
            return;
        }

        var reminderAt = HasReminder ? ReminderDate.Date.Add(ReminderTime) : (DateTime?)null;
        if (reminderAt is DateTime fireTime && fireTime <= DateTime.Now)
        {
            await _navigationService.DisplayAlertAsync(
                _languageService.GetString("NotesValidationTitleText"),
                _languageService.GetString("NotesReminderPastText"),
                _languageService.GetString("CommonOkText"));
            return;
        }

        if (HasReminder)
        {
            var granted = await _reminderScheduler.RequestPermissionAsync();
            if (!granted)
            {
                await _navigationService.DisplayAlertAsync(
                    _languageService.GetString("NotesValidationTitleText"),
                    _languageService.GetString("NotesReminderPermissionText"),
                    _languageService.GetString("CommonOkText"));
                return;
            }
        }

        try
        {
            IsBusy = true;

            var note = new Note
            {
                Id = _editingId,
                Title = TitleText.Trim(),
                Content = ContentText.Trim(),
                CreatedAt = IsEditing ? _editingCreatedAt : DateTime.UtcNow,
                ReminderAt = reminderAt
            };

            var savedId = await _notesRepository.SaveAsync(note);

            await UpsertReminderAsync(savedId, note);

            await _navigationService.PopAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar la nota");
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

    private async Task UpsertReminderAsync(int noteId, Note note)
    {
        try
        {
            if (note.ReminderAt is DateTime scheduledAt)
            {
                var notificationTitle = string.IsNullOrWhiteSpace(note.Title)
                    ? _languageService.GetString("NotesReminderNotificationTitleText")
                    : note.Title;

                await _reminderScheduler.ScheduleAsync(noteId, notificationTitle,
                    _languageService.GetString("NotesReminderNotificationBodyText"), scheduledAt);
            }
            else
            {
                await _reminderScheduler.CancelAsync(noteId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al programar el recordatorio de la nota {NoteId}", noteId);
        }
    }
}
