using Checkers.ViewModel;

namespace Checkers.View;

public partial class GamePage : ContentPage
{
    private readonly BoardViewModel viewModel;
    public GamePage()
	{
		InitializeComponent();
        viewModel = new BoardViewModel();
        BindingContext = viewModel;

    }
}