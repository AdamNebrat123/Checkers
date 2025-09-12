using Checkers.ViewModels;

namespace Checkers.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm; // MVVM binding

    }
}