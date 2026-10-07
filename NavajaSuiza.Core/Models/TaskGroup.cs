namespace NavajaSuiza.Core.Models;

public class TaskGroup
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<TaskItem> Items { get; set; } = [];
}
