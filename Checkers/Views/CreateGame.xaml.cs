using Checkers.Data;
using Checkers.Models;
using Checkers.Utils;

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

        try
        {
            var gameService = new GameService();
            var newGame = new GameModel
            {
                GameId = Guid.NewGuid().ToString(),
                Host = "AVI KAMIL", // HARDCODED NOW!!! MUST BE CHANGED!!!!!!!!!!!!
                HostColor = playerColor,
                Guest = "",
                GuestColor = opponentColor,
                IsWhiteTurn = true,
                BoardState = BoardHelper.InitialBoardState(),
                Moves = new List<GameMove>(),
                CreatedAt = DateTime.UtcNow
            };

            await gameService.CreateGameAsync(newGame);

            // ניווט עם פרמטרים דרך Shell (כולל ה-ID כדי ש-WaitingRoom ידע לאיזה משחק להתחבר)
            await Shell.Current.GoToAsync($"WaitingRoom?gameId={newGame.GameId}&gameName={gameName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating game: {ex.Message}");
            await DisplayAlert("Error", "Failed to create game.", "OK");
        }
    }
}