using Checkers.ViewModel;
using Checkers.ViewModels;

namespace Checkers.Views;

public partial class GamePage : ContentPage
{
    private readonly BoardViewModel viewModel;
    public GamePage(GameManagerViewModel gameManager)
	{
		InitializeComponent();
        viewModel = new BoardViewModel(gameManager, true);
        BindingContext = viewModel;

    }
}