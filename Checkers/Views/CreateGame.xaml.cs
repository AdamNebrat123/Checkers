namespace Checkers.Views;

public partial class CreateGame : ContentPage
{
    string playerColor = "White";
    string opponentColor = "Black";
    public CreateGame()
	{
		InitializeComponent();
	}
    private void OnSwitchClicked(object sender, EventArgs e)
    {
        // החלפת צבעים
        (playerColor, opponentColor) = (opponentColor, playerColor);

        PlayerImage.Source = playerColor == "White" ? "white_piece.png" : "black_piece.png";
        OpponentImage.Source = opponentColor == "White" ? "white_piece.png" : "black_piece.png";
    }
    private async void OnCreateGameClicked(object sender, EventArgs e)
    {
        string gameName = GameNameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(gameName))
        {
            await DisplayAlert("Error", "Please enter a game name.", "OK");
            return;
        }

        var selectedType = this.GetVisualTreeDescendants()
                               .OfType<RadioButton>()
                               .FirstOrDefault(r => r.GroupName == "GameType" && r.IsChecked);
        string gameType = selectedType?.Value?.ToString() ?? "Classic";

        // ניווט עם פרמטרים דרך Shell
        await Shell.Current.GoToAsync($"WaitingRoom?gameName={gameName}");
    }
}