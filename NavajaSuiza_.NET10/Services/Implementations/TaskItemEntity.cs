using SQLite;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class TaskItemEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int TaskGroupId { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public int Importance { get; set; }

    public int Position { get; set; }
}
