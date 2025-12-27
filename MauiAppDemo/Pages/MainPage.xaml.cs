using MauiAppDemo.Models;
using MauiAppDemo.PageModels;

namespace MauiAppDemo.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}