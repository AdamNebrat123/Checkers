using Checkers.GameLogic;
using Checkers.ViewModels;
using Checkers.Views;

namespace Checkers
{
    public partial class AppShell : Shell
    {
        private readonly LoginViewModel _vm;
        public AppShell(LoginViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
            Routing.RegisterRoute(nameof(AIGameSetupPage), typeof(AIGameSetupPage));

            TryAutoLogin();
        }


        private async Task TryAutoLogin()
        {
            try
            {
                var email = await SecureStorage.GetAsync("Email");
                var password = await SecureStorage.GetAsync("Password");

                if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(password))
                    return;

                await _vm.TryAutoLogin(email, password);
            }
            catch (Exception)
            {
                await Shell.Current.GoToAsync("//LogIn");
            }
        }

    }
}
