using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Checkers.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ICommand GoToLoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }


        public MainViewModel()
        {
            GoToLoginCommand = new Command(async () =>
            {
                // Absolute route to LoginPage
                await Shell.Current.GoToAsync("//LoginPage");
            });

            GoToRegisterCommand = new Command(async () =>
            {
                // Absolute route to RegisterPage
                await Shell.Current.GoToAsync("//RegisterPage");
            });


        }
    }
}
