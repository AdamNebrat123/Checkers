using Checkers.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Checkers.Views;

public partial class StatisticsPage : ContentPage, INotifyPropertyChanged
{
    private readonly ReplayService _replayService = ReplayService.GetInstance();

    private int totalGames;
    private int whiteGames;
    private int blackGames;

    public int TotalGames
    {
        get => totalGames;
        set { totalGames = value; OnPropertyChanged(); }
    }

    public int WhiteGames
    {
        get => whiteGames;
        set { whiteGames = value; OnPropertyChanged(); OnPropertyChanged(nameof(WhitePercentage)); }
    }

    public int BlackGames
    {
        get => blackGames;
        set { blackGames = value; OnPropertyChanged(); OnPropertyChanged(nameof(BlackPercentage)); }
    }

    public string WhitePercentage =>
        TotalGames == 0 ? "0%" : $"{(WhiteGames * 100) / TotalGames}%";

    public string BlackPercentage =>
        TotalGames == 0 ? "0%" : $"{(BlackGames * 100) / TotalGames}%";

    public StatisticsPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        string userId = Preferences.Get("UserId", "");
        if (string.IsNullOrEmpty(userId))
            return;

        var replays = await _replayService.GetAllReplaysForUserAsync(userId);

        TotalGames = replays.Count;
        WhiteGames = replays.Count(r => r.IsWhitePerspective);
        BlackGames = replays.Count(r => !r.IsWhitePerspective);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}