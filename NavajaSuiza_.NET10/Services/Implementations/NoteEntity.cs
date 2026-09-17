using SQLite;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class NoteEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}