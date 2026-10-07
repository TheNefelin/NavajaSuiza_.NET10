namespace NavajaSuiza.Core.ViewModels;

public sealed class TaskGroupListItem : NotesListItem
{
    public IReadOnlyList<TaskItemListItem> Items { get; init; } = [];

    public string CreatedAtText => CreatedAt.ToString("dd-MM-yyyy");
}
