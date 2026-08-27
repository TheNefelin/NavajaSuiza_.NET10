using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class BaseViewModelTests
{
    [Fact]
    public void IsBusy_RaisesPropertyChanged()
    {
        var vm = new TestViewModel();
        var changed = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BaseViewModel.IsBusy))
                changed = true;
        };

        vm.IsBusy = true;

        Assert.True(changed);
        Assert.True(vm.IsBusy);
    }

    [Fact]
    public void IsLoading_RaisesPropertyChanged()
    {
        var vm = new TestViewModel();
        var changed = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BaseViewModel.IsLoading))
                changed = true;
        };

        vm.IsLoading = true;

        Assert.True(changed);
        Assert.True(vm.IsLoading);
    }

    [Fact]
    public void Title_RaisesPropertyChanged()
    {
        var vm = new TestViewModel();
        var changed = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BaseViewModel.Title))
                changed = true;
        };

        vm.Title = "Test";

        Assert.True(changed);
        Assert.Equal("Test", vm.Title);
    }

    [Fact]
    public void Title_DefaultValueIsEmpty()
    {
        var vm = new TestViewModel();
        Assert.Equal(string.Empty, vm.Title);
    }

    [Fact]
    public void Cleanup_DoesNotThrow()
    {
        var vm = new TestViewModel();
        var ex = Record.Exception(() => vm.Cleanup());
        Assert.Null(ex);
    }

    private class TestViewModel : BaseViewModel { }
}
