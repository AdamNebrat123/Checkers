using Checkers.Data;
using Checkers.GameLogic;
using Checkers.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;

namespace Checkers.Views;

public partial class ReplaysPage : ContentPage
{
    private readonly ReplayService _replayService = ReplayService.GetInstance();

    public ObservableCollection<GameReplay> Replays { get; } = new();

    public ICommand ReplayCommand { get; }
    public ICommand ClearHistoryCommand { get; }
    public ICommand DeleteReplayCommand { get; }


    public ReplaysPage()
    {
        InitializeComponent();
        BindingContext = this;

        ReplayCommand = new Command<GameReplay>(OnReplaySelected);
        ClearHistoryCommand = new Command(async () => await OnClearHistory());
        DeleteReplayCommand = new Command<GameReplay>(async (replay) => await OnDeleteReplay(replay));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadReplaysAsync();
    }

    private async Task LoadReplaysAsync()
    {
        string userId = Preferences.Get("UserId", "");

        if (string.IsNullOrEmpty(userId))
            return;

        var replays = await _replayService.GetAllReplaysForUserAsync(userId);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Replays.Clear();
            foreach (var replay in replays.OrderByDescending(r => r.CreatedAt))
                Replays.Add(replay);
        });
    }

    private async void OnReplaySelected(GameReplay replay)
    {
        var replaySettings = new ReplaySettings
        {
            GameReplay = replay,
            GameId = replay.GameId,
            IsWhitePerspective = replay.IsWhitePerspective,
        };
        var wrapper = new ModeParametersWrapper
        {
            Mode = GameMode.Replay.ToString(),
            Parameters = JsonSerializer.SerializeToElement(replaySettings)
        };

        await Shell.Current.GoToAsync(nameof(GamePage), new Dictionary<string, object>
        {
            { "wrapper", wrapper }
        });
    }

    private async Task OnDeleteReplay(GameReplay replay)
    {
        string userId = Preferences.Get("UserId", "");
        if (string.IsNullOrEmpty(userId))
            return;

        await _replayService.DeleteReplayAsync(userId, replay.GameId);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Replays.Remove(replay);
        });
    }

    private async Task OnClearHistory()
    {
        bool confirm = await DisplayAlert(
            "Clear History",
            "This will permanently delete all your replays. Are you sure?",
            "Yes",
            "Cancel");

        if (!confirm)
            return;

        string userId = Preferences.Get("UserId", "");
        if (string.IsNullOrEmpty(userId))
            return;

        await _replayService.DeleteAllReplaysForUserAsync(userId);
        Replays.Clear();
    }

}