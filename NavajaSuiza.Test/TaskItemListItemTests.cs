using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class TaskItemListItemTests
{
    [Fact]
    public void Constructor_CopiesSourceValues()
    {
        var source = new TaskItem { Id = 5, Title = "Comprar", IsCompleted = true, Importance = TaskImportance.High };

        var item = new TaskItemListItem(source);

        Assert.Equal(5, item.Id);
        Assert.Equal("Comprar", item.Title);
        Assert.Equal(TaskImportance.High, item.Importance);
        Assert.True(item.IsCompleted);
    }

    [Fact]
    public void Constructor_DoesNotInvokeCallback()
    {
        var invoked = false;
        var source = new TaskItem { IsCompleted = true };

        _ = new TaskItemListItem(source, () => invoked = true);

        Assert.False(invoked);
    }

    [Fact]
    public void IsCompleted_Set_UpdatesSourceAndInvokesCallback()
    {
        var invoked = false;
        var source = new TaskItem { Id = 5, IsCompleted = false };
        var item = new TaskItemListItem(source, () => invoked = true);

        item.IsCompleted = true;

        Assert.True(source.IsCompleted);
        Assert.True(invoked);
    }
}
