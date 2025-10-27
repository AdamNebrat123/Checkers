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
    private BoardViewModel viewModel;
    private readonly GameManagerViewModel _gameManager;

    public int Depth { get; set; }
    public string PlayerColor { get; set; }

    public GamePage(GameManagerViewModel gameManager)
    {
        InitializeComponent();
        _gameManager = gameManager;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        bool isWhite = PlayerColor == "White";
        var boardVM = new BoardViewModel(isWhite);
        var aiColor = isWhite ? PieceColor.Black : PieceColor.White;

        var aiManager = new AIManager(aiColor, Depth);
        var strategy = new AiGameStrategy(_gameManager, aiManager);
        var gameVM = new GameViewModel(strategy, _gameManager, boardVM);

        boardVM.SquareClicked += async square => await gameVM.HandleSquareSelectedAsync(square);

        BindingContext = boardVM;
        await gameVM.InitializeAsync();
    }
}