using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using SQLite;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class SqliteNotesRepository : INotesRepository
{
    private readonly SQLiteAsyncConnection _database;

    public SqliteNotesRepository()
    {
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "navajasuiza.db3");
        _database = new SQLiteAsyncConnection(databasePath);
        _database.CreateTableAsync<NoteEntity>().Wait();
    }

    public async Task<List<Note>> GetAllAsync()
    {
        var entities = await _database.Table<NoteEntity>().ToListAsync();
        return entities.Select(FromEntity).ToList();
    }

    public async Task<Note?> GetByIdAsync(int id)
    {
        var entity = await _database.FindAsync<NoteEntity>(id);
        return entity is null ? null : FromEntity(entity);
    }

    public async Task<int> SaveAsync(Note note)
    {
        var entity = ToEntity(note);

        if (note.Id == 0)
        {
            await _database.InsertAsync(entity);
            return entity.Id;
        }

        await _database.UpdateAsync(entity);
        return entity.Id;
    }

    public async Task DeleteAsync(int id)
    {
        await _database.DeleteAsync<NoteEntity>(id);
    }

    private static NoteEntity ToEntity(Note note) => new()
    {
        Id = note.Id,
        Title = note.Title,
        Content = note.Content,
        CreatedAt = note.CreatedAt
    };

    private static Note FromEntity(NoteEntity entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Content = entity.Content,
        CreatedAt = entity.CreatedAt
    };
}