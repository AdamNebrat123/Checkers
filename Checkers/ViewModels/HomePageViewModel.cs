using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Checkers.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        public ICommand PlayVsAI { get; }
        public ICommand GoToCreateGame { get; }
        public ICommand JoinRandomGame { get; }


        private string _currentUserName;

        public string CurrentUserName
        {
            get => _currentUserName;
            set
            {
                _currentUserName = value;
                OnPropertyChanged();
            }
        }
        public HomePageViewModel()
        {

            CurrentUserName = Preferences.Get("UserName", string.Empty);

            PlayVsAI = new Command(async () =>
            {
                await Shell.Current.GoToAsync("AIGameSetupPage");
            });

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
