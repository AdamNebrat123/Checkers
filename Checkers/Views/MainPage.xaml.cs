using Checkers.Views;
using Checkers.ViewModels;

namespace Checkers.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel viewModel;

        public MainPage(MainViewModel vm)
        {
            InitializeComponent();
            viewModel = vm;
            BindingContext = viewModel;

        }
        private async void OnGoToGamePage_Clicked(object sender, EventArgs e)
        {
            int depth = 6;
            await Shell.Current.GoToAsync($"{nameof(GamePage)}?depth={depth}");
        }

        private async void SkipLogIn_Clicked(object sender, EventArgs e)
        {
            // Navigate to authenticated page
            Application.Current.MainPage = new AuthenticatedShell();
            await Shell.Current.GoToAsync("///HomePage");
        }
    }

}
