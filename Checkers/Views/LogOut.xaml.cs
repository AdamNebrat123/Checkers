using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace Checkers.Views;

public partial class LogOut : ContentPage
{

    public LogOut()
	{
		InitializeComponent();
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LoadingOverlay.IsVisible = true;


        await LogoutAsync();

        LoadingOverlay.IsVisible = false;
    }


    private async Task LogoutAsync()
    {
        try
        {
            SecureStorage.Remove("Email");
            SecureStorage.Remove("Password");

            // îòáø ìîñê ääúçáøåú
            Application.Current.MainPage = IPlatformApplication.Current.Services.GetRequiredService<AppShell>();
            await Shell.Current.GoToAsync("///MainPage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("ùâéàä", $"éöéàä ðëùìä: {ex.Message}", "àéùåø");
        }
    }
}