namespace NavajaSuiza.Core.ViewModels;

public abstract class NotesListItem
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }
}
