using Checkers.GameLogic;
using Checkers.Views;

namespace Checkers
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
            Routing.RegisterRoute(nameof(AIGameSetupPage), typeof(AIGameSetupPage));
            Routing.RegisterRoute(nameof(GamePage<AiSettings>), typeof(GamePage<AiSettings>));
            Routing.RegisterRoute(nameof(GamePage<OnlineSettings>), typeof(GamePage<OnlineSettings>));
        }
    }
}
