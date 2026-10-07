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

    public NoteEditorViewModel(
        INotesRepository notesRepository,
        INavigationService navigationService,
        ILanguageService languageService,
        ILogger<NoteEditorViewModel> logger)
    {
        _notesRepository = notesRepository;
        _navigationService = navigationService;
        _languageService = languageService;
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
        PageTitle = _languageService.GetString(IsEditing ? "NotesEditTitleText" : "NotesCreateNoteTitleText");
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(TitleText) && string.IsNullOrWhiteSpace(ContentText))
            return;

        try
        {
            IsBusy = true;

            var note = new Note
            {
                Id = _editingId,
                Title = TitleText.Trim(),
                Content = ContentText.Trim(),
                CreatedAt = IsEditing ? _editingCreatedAt : DateTime.UtcNow
            };

            await _notesRepository.SaveAsync(note);

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
}
