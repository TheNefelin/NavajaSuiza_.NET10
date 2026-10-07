namespace NavajaSuiza.Core.ViewModels;

public sealed class NotesSection : List<NotesListItem>
{
    public NotesSection(string name, IEnumerable<NotesListItem> items) : base(items)
    {
        Name = name;
    }

    public string Name { get; }
}
