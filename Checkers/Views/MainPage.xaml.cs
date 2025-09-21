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
        private async void PlayVSai_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("AIGameSetupPage");
        }

        private async void SkipLogIn_Clicked(object sender, EventArgs e)
        {
            // Navigate to authenticated page
            Application.Current.MainPage = new AuthenticatedShell();
            await Shell.Current.GoToAsync("///HomePage");
        }
    }

}
