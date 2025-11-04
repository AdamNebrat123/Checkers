using Checkers.Data;
using Checkers.GameLogic;
using Checkers.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;

namespace Checkers.Views;

public partial class CheckersLobby : ContentPage
{
    private readonly GameRealtimeService _realtimeService = GameRealtimeService.GetInstance();
    private IDisposable? _subscription;
    public ObservableCollection<GameModel> AvailableGames { get; set; } = new();

    public ICommand JoinGameCommand { get; }

    public CheckersLobby()
    {
        InitializeComponent();
        BindingContext = this;

        JoinGameCommand = new Command<GameModel>(async (game) => await OnJoinGame(game));

        // טעינה ראשונית של כל המשחקים
        LoadAvailableGamesAsync();

        // מנוי לשינויים עתידיים
        _subscription = _realtimeService.SubscribeToAvailableGames(games =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AvailableGames.Clear();
                foreach (var game in games.OrderByDescending(g => g.CreatedAt))
                    AvailableGames.Add(game);
            });
        });

    }

    private async void LoadAvailableGamesAsync()
    {
        try
        {
            var allGames = await _realtimeService.GetAllGamesAsync();
            var openGames = allGames.Where(g => string.IsNullOrEmpty(g.Guest)).OrderByDescending(g => g.CreatedAt);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AvailableGames.Clear();
                foreach (var game in openGames)
                    AvailableGames.Add(game);
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load games: {ex.Message}", "OK");
        }
    }

    private async Task OnJoinGame(GameModel game)
    {
        try
        {
            // נעדכן את המשחק עם האורח
            game.Guest = "Guest123"; // אפשר להחליף כשיהיה לך שם משתמש אמיתי
            await _realtimeService.UpdateGameAsync(game);

            // נגדיר את ההגדרות בדיוק כמו ש־Host עושה
            var onlineSettings = new OnlineSettings
            {
                GameId = game.GameId,
                IsLocalPlayerWhite = game.GuestColor == "White"
            };

            var wrapper = new ModeParametersWrapper
            {
                Mode = GameMode.Online.ToString(),
                Parameters = JsonSerializer.SerializeToElement(onlineSettings)
            };

            await Shell.Current.GoToAsync(nameof(GamePage), new Dictionary<string, object>
            {
                { "wrapper", wrapper }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to join game: {ex.Message}", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _subscription?.Dispose();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        
    }
}
