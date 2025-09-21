namespace Checkers.Views;

public partial class AIGameSetupPage : ContentPage
{
    string playerColor = "White";
    string opponentColor = "Black";


    public AIGameSetupPage()
	{
		InitializeComponent();
	}

    private void OnSwitchClicked(object sender, EventArgs e)
    {
        // החלפה פשוטה בין הצבעים
        (playerColor, opponentColor) = (opponentColor, playerColor);

        PlayerImage.Source = playerColor == "White" ? "white_piece.png" : "black_piece.png";
        OpponentImage.Source = opponentColor == "White" ? "white_piece.png" : "black_piece.png";
    }

    private async void OnPlayClicked(object sender, EventArgs e)
    {
        // שליפת רמת הקושי
        var selected = this.GetVisualTreeDescendants()
                           .OfType<RadioButton>()
                           .FirstOrDefault(r => r.IsChecked);
        string difficulty = selected?.Value?.ToString() ?? "Medium";

        // מיפוי רמת קושי ל-depth
        int depth = difficulty switch
        {
            "Easy" => 1,
            "Medium" => 3,
            "Hard" => 6,
            _ => 3
        };

        await Shell.Current.GoToAsync($"{nameof(GamePage)}?depth={depth}&playerColor={playerColor}");
    }

    private async void OnGoToGamePage_Clicked(object sender, EventArgs e)
    {
        int depth = 6;
        await Shell.Current.GoToAsync($"{nameof(GamePage)}?depth={depth}&playerColor={playerColor}");
    }
}