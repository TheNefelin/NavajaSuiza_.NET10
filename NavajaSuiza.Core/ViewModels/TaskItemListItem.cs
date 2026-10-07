using CommunityToolkit.Mvvm.ComponentModel;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.ViewModels;

public sealed partial class TaskItemListItem : ObservableObject
{
    private readonly TaskItem _source;
    private readonly Action? _onCompletedChanged;
    private bool _suppressNotification;

    public TaskItemListItem(TaskItem source, Action? onCompletedChanged = null)
    {
        _source = source;
        _onCompletedChanged = onCompletedChanged;

        _suppressNotification = true;
        IsCompleted = source.IsCompleted;
        _suppressNotification = false;
    }

    public int Id => _source.Id;

    public string Title => _source.Title;

    public TaskImportance Importance => _source.Importance;

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    partial void OnIsCompletedChanged(bool value)
    {
        if (_suppressNotification)
            return;

        _source.IsCompleted = value;
        _onCompletedChanged?.Invoke();
    }
}
