using SQLite;

namespace NavajaSuiza_.NET10.Services.Implementations;

public sealed class NotesDatabase
{
    public NotesDatabase()
    {
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "navajasuiza.db3");
        Connection = new SQLiteAsyncConnection(databasePath);
        Connection.CreateTableAsync<NoteEntity>().Wait();
        Connection.CreateTableAsync<TaskGroupEntity>().Wait();
        Connection.CreateTableAsync<TaskItemEntity>().Wait();
    }

    public SQLiteAsyncConnection Connection { get; }
}
