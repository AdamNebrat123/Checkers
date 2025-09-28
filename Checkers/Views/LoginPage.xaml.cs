using Checkers.ViewModels;

namespace Checkers.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        try
        {
            InitializeComponent();
            BindingContext = vm; // MVVM binding
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load page: {ex.Message}", "OK");
        }


    }


}