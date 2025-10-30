using Checkers.GameLogic;
using Checkers.Model;
using Checkers.Models;
using Checkers.ViewModel;
using Checkers.ViewModels;

namespace Checkers.Views;



[QueryProperty(nameof(Depth), "depth")]
[QueryProperty(nameof(PlayerColor), "playerColor")]
public partial class GamePage : ContentPage
{
    private readonly GameViewModel _gameViewModel;

    public int Depth { get; set; }
    public string PlayerColor { get; set; }

    public GamePage(GameViewModel gameViewModel)
    {
        InitializeComponent();
        _gameViewModel = gameViewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        bool isWhite = PlayerColor == "White";

        // Let GameViewModel create the BoardViewModel internally
        _gameViewModel.InitializeBoard(isWhite);

        // Create AI strategy and set it on the injected GameViewModel
        var strategy = new AiGameStrategy(_gameViewModel.GameManager, Depth, isWhite);
        _gameViewModel.SetStrategy(strategy);

        _gameViewModel.BoardVM.SquareClicked += async square => await _gameViewModel.HandleSquareSelectedAsync(square);

        // Bind the page to the overall GameViewModel
        BindingContext = _gameViewModel;
        await _gameViewModel.InitializeAsync();
    }
}