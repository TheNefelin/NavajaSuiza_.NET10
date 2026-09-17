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
    private readonly INavigationService _navigationService;
    private readonly ILanguageService _languageService;
    private readonly ILogger<NotesViewModel> _logger;

    private List<Note>? _allNotes;

    public ObservableCollection<Note> Notes { get; } = new();

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasNotes { get; set; }

    public NotesViewModel(
        INotesRepository notesRepository,
        INavigationService navigationService,
        ILanguageService languageService,
        ILogger<NotesViewModel> logger)
    {
        _notesRepository = notesRepository;
        _navigationService = navigationService;
        _languageService = languageService;
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
    private async Task NavigateToEditNoteAsync(Note note)
    {
        await _navigationService.PushAsync("NoteEditorPage", note);
    }

    [RelayCommand]
    private async Task DeleteNoteAsync(Note note)
    {
        if (note is null)
            return;

        var confirmed = await _navigationService.DisplayAlertConfirmAsync(
            _languageService.GetString("NotesDeleteTitleText"),
            _languageService.GetString("NotesDeleteConfirmationText"),
            _languageService.GetString("CommonYesText"),
            _languageService.GetString("CommonNoText"));

        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            await _notesRepository.DeleteAsync(note.Id);
            await LoadNotesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la nota");
            await _navigationService.DisplayAlertAsync(
                _languageService.GetString("NotesErrorTitleText"),
                _languageService.GetString("NotesErrorDeleteText"),
                _languageService.GetString("CommonOkText"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadNotesAsync()
    {
        try
        {
            IsBusy = true;
            _allNotes = await _notesRepository.GetAllAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar las notas");
            await _navigationService.DisplayAlertAsync(
                _languageService.GetString("NotesErrorTitleText"),
                _languageService.GetString("NotesErrorLoadText"),
                _languageService.GetString("CommonOkText"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var source = _allNotes ?? [];

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            source = source
                .Where(n => n.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                            || n.Content.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        Notes.Clear();

        foreach (var note in source.OrderByDescending(n => n.CreatedAt))
            Notes.Add(note);

        HasNotes = Notes.Count > 0;
    }
}