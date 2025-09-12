using Checkers.ViewModels;

namespace Checkers.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm; // MVVM binding

    }
}