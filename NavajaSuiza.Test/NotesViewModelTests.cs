using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class NotesViewModelTests
{
    private readonly Mock<INotesRepository> _repositoryMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<ILogger<NotesViewModel>> _loggerMock = new();

    private NotesViewModel CreateSut()
    {
        _languageServiceMock.Setup(s => s.GetString(It.IsAny<string>())).Returns("test");
        return new NotesViewModel(
            _repositoryMock.Object,
            _navigationServiceMock.Object,
            _languageServiceMock.Object,
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
        Assert.Equal([2, 1], vm.Notes.Select(n => n.Id).ToArray());
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

        await vm.DeleteNoteCommand.ExecuteAsync(note);

        _repositoryMock.Verify(r => r.DeleteAsync(3), Times.Once);
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

        await vm.DeleteNoteCommand.ExecuteAsync(note);

        _repositoryMock.Verify(r => r.DeleteAsync(3), Times.Never);
    }

    [Fact]
    public async Task SearchText_FiltersNotes()
    {
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([
            new Note { Id = 1, Title = "Compra", Content = "Leche", CreatedAt = DateTime.UtcNow },
            new Note { Id = 2, Title = "Tarea", Content = "Estudiar", CreatedAt = DateTime.UtcNow }
        ]);

        var vm = CreateSut();
        await vm.OnPageAppearingAsync();

        vm.SearchText = "leche";

        Assert.Single(vm.Notes);
        Assert.Equal(1, vm.Notes[0].Id);
    }

    [Fact]
    public async Task NavigateToNewNote_NavigatesWithoutParameter()
    {
        var vm = CreateSut();

        await vm.NavigateToNewNoteCommand.ExecuteAsync(null);

        _navigationServiceMock.Verify(n => n.PushAsync("NoteEditorPage", It.Is<object?>(p => p == null)), Times.Once);
    }

    [Fact]
    public async Task NavigateToEditNote_NavigatesWithSelectedNote()
    {
        var note = new Note { Id = 5, Title = "TituloX", Content = "ContenidoX", CreatedAt = DateTime.UtcNow };
        var vm = CreateSut();

        await vm.NavigateToEditNoteCommand.ExecuteAsync(note);

        _navigationServiceMock.Verify(n => n.PushAsync("NoteEditorPage", note), Times.Once);
    }
}