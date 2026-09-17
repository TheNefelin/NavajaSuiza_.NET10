using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Interfaces;

public interface INotesRepository
{
    Task<List<Note>> GetAllAsync();

    Task<Note?> GetByIdAsync(int id);

    Task<int> SaveAsync(Note note);

    Task DeleteAsync(int id);
}