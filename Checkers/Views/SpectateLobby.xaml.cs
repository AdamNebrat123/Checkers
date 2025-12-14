using Checkers.Data;
using Checkers.GameLogic;
using Checkers.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;

namespace Checkers.Views;

public partial class SpectateLobby : ContentPage
{
    private readonly GameRealtimeService _realtimeService = GameRealtimeService.GetInstance();
    private IDisposable? _subscription;
    public ObservableCollection<GameModel> AvailableGames { get; set; } = new();

    public ICommand SpectateWhiteGameCommand { get; }
    public ICommand SpectateBlackGameCommand { get; }

    public SpectateLobby()
    {
        InitializeComponent();
        BindingContext = this;

        SpectateWhiteGameCommand = new Command<GameModel>(async (game) => await OnJoinGame(game, true));
        SpectateBlackGameCommand = new Command<GameModel>(async (game) => await OnJoinGame(game, false));

        // טעינה ראשונית של כל המשחקים
        LoadAvailableGamesAsync();

        // מנוי לשינויים עתידיים
        _subscription = _realtimeService.SubscribeToAvailableGames(games =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var openGames = games.Where(g => !(string.IsNullOrEmpty(g.Guest))).OrderByDescending(g => g.CreatedAt);

                bool same = games.Count == AvailableGames.Count &&
                            !games.Except(AvailableGames).Any();
                if (same)
                    return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AvailableGames.Clear();
                    foreach (var game in openGames)
                        AvailableGames.Add(game);
                });
            });
        });

    }

    private async void LoadAvailableGamesAsync()
    {
        try
        {
            var allGames = await _realtimeService.GetAllGamesAsync();
            var openGames = allGames.Where(g => !(string.IsNullOrEmpty(g.Guest))).OrderByDescending(g => g.CreatedAt);

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

    private async Task OnJoinGame(GameModel game, bool isSpectatingWhite)
    {
        try
        {
            var spectatorSettings = new SpectatorSettings
            {
                GameId = game.GameId,
                IsWhitePerspective = isSpectatingWhite,
            };

            var wrapper = new ModeParametersWrapper
            {
                Mode = GameMode.Online.ToString(),
                Parameters = JsonSerializer.SerializeToElement(spectatorSettings)
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