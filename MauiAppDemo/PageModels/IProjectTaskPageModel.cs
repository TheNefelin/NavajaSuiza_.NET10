using CommunityToolkit.Mvvm.Input;
using MauiAppDemo.Models;

namespace MauiAppDemo.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}