using Microsoft.Maui.Controls;

namespace Checkers.Views;

[QueryProperty(nameof(GameName), "gameName")]
public partial class WaitingRoom : ContentPage
{
    public string GameName { get; set; }
    public string GameType { get; set; }

    public WaitingRoom()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // עדכון GameCodeLabel עם הערך שהתקבל
        GameCodeLabel.Text = GameName ?? "GAME1234";

    }

    private async void OnCancelWaitingClicked(object sender, EventArgs e)
    {
        // חזרה לעמוד הקודם או דף ראשי
        await Shell.Current.GoToAsync("..");
    }
}
