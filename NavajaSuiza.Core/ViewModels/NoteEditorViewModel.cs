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

    private Note? _editingNote;

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
        _editingNote = _navigationService.TakeNavigationParameter() as Note;
        IsEditing = _editingNote is not null;

        if (_editingNote is null)
        {
            TitleText = string.Empty;
            ContentText = string.Empty;
            PageTitle = _languageService.GetString("NotesNewTitleText");
        }
        else
        {
            TitleText = _editingNote.Title;
            ContentText = _editingNote.Content;
            PageTitle = _languageService.GetString("NotesEditTitleText");
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(TitleText) && string.IsNullOrWhiteSpace(ContentText))
            return;

        try
        {
            IsBusy = true;

            if (_editingNote is null)
            {
                var note = new Note
                {
                    Title = TitleText.Trim(),
                    Content = ContentText.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                await _notesRepository.SaveAsync(note);
            }
            else
            {
                _editingNote.Title = TitleText.Trim();
                _editingNote.Content = ContentText.Trim();
                await _notesRepository.SaveAsync(_editingNote);
            }

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