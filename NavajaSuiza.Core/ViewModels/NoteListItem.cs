namespace NavajaSuiza.Core.ViewModels;

public sealed class NoteListItem : NotesListItem
{
    public string Content { get; init; } = string.Empty;

    public DateTime? ReminderAt { get; init; }

    public bool HasReminder => ReminderAt.HasValue;
}
