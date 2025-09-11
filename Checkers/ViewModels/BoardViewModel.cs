using Checkers.Model;
using Checkers.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.ViewModel
{
    public class BoardViewModel : ViewModelBase
    {
        public ObservableCollection<SquareViewModel> Squares { get; }

        private SquareViewModel? selectedSquare;
        public SquareViewModel? SelectedSquare
        {
            get => selectedSquare;
            set
            {
                if (selectedSquare != value)
                {
                    selectedSquare = value;
                    OnPropertyChanged();
                }
            }
        }

        public BoardViewModel()
        {
            var board = new Board();
            var temp = new List<SquareViewModel>();

            for (int row = 0; row < Board.Size; row++)
                for (int col = 0; col < Board.Size; col++)
                    temp.Add(new SquareViewModel(board.Squares[row, col]));

            Squares = new ObservableCollection<SquareViewModel>(temp);
        }
    }
}
