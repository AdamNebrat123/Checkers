using Checkers.Views;

namespace Checkers;

public partial class AuthenticatedShell : Shell
{
	public AuthenticatedShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("CreateGame", typeof(CreateGame));
        Routing.RegisterRoute("WaitingRoom", typeof(WaitingRoom));
    }
}