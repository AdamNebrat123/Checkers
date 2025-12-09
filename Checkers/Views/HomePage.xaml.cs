using Checkers.ViewModels;

namespace Checkers.Views;

public partial class HomePage : ContentPage
{
	private readonly HomePageViewModel homePageViewModel;
    public HomePage(HomePageViewModel viewModel)
	{
		InitializeComponent();
		this.homePageViewModel = viewModel;
		BindingContext = homePageViewModel;

    }
}