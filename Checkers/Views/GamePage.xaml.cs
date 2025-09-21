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

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        bool isWhite = PlayerColor == "White";
        viewModel = new BoardViewModel(_gameManager, Depth, isWhite);
        BindingContext = viewModel;
    }
}