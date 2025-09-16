using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Checkers.ViewModels
{
    public class HomePageViewModel
    {
        public ICommand GoToCreateGame { get; }
        public ICommand JoinRandomGame { get; }

        public HomePageViewModel()
        {
            GoToCreateGame = new Command(async () =>
            {
                await Shell.Current.GoToAsync("CreateGame");
            });

            JoinRandomGame = new Command(async () =>
            {
                //TODO
            });

        }
    }
}
