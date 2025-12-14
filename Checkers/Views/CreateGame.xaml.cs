using Checkers.Data;
using Checkers.Models;
using Checkers.Utils;

namespace Checkers.Views;

public partial class CreateGame : ContentPage
{
    string playerColor = "White";
    string opponentColor = "Black";


    private readonly GameService _gameService = GameService.GetInstance();


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

        try
        {
            string currentUserName = Preferences.Get("UserName", "Guest");
            var newGame = new GameModel
            {
                GameId = Guid.NewGuid().ToString(),
                Host = currentUserName,
                HostColor = playerColor,
                Guest = "",
                GuestColor = opponentColor,
                IsWhiteTurn = true,
                BoardState = BoardHelper.InitialBoardState(),
                Move = new GameMove(),
                CreatedAt = DateTime.UtcNow,
                TimerTimeInMinutes = GetSelectedTimerMinutes()
            };

            await _gameService.CreateGameAsync(newGame);

            // ניווט עם פרמטרים דרך Shell (כולל ה-ID כדי ש-WaitingRoom ידע לאיזה משחק להתחבר)
            await Shell.Current.GoToAsync($"WaitingRoom?gameId={newGame.GameId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating game: {ex.Message}");
            await DisplayAlert("Error", "Failed to create game.", "OK");
        }
    }
    private int GetSelectedTimerMinutes()
    {
        if (TimerPicker.SelectedItem == null)
            return 600; // ברירת מחדל 10 דקות

        string selected = TimerPicker.SelectedItem.ToString();
        return selected switch
        {
            "30 minutes" => 30,
            "10 minutes" => 10,
            "5 minutes" => 5,
            "1 minute" => 1,
            "10 seconds" => 10/60,
            _ => 10
        };
    }

}