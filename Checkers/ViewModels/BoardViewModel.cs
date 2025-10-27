using Checkers.GameLogic;
using Checkers.Model;
using Checkers.Models;
using Checkers.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Checkers.ViewModel
{
    public class BoardViewModel : ViewModelBase
    {
        public ObservableCollection<SquareViewModel> Squares { get; }
        public readonly Board Board;
        private readonly bool whitePerspective;
        private SquareViewModel? selectedSquare;
        public SquareViewModel? SelectedSquare
        {
            get => selectedSquare;
            set
            {
                selectedSquare = value;
                OnPropertyChanged();
                UpdateMoveMarkers();
            }
        }

        public BoardViewModel(bool whitePerspective = true)
        {
            this.whitePerspective = whitePerspective;
            Board = new Board();
            var temp = new List<SquareViewModel>();
            for (int row = 0; row < Board.Size; row++)
            {
                for (int col = 0; col < Board.Size; col++)
                {
                    int displayRow = whitePerspective ? row : Board.Size - 1 - row;
                    int displayCol = whitePerspective ? col : Board.Size - 1 - col;
                    var squareVM = new SquareViewModel(Board.Squares[displayRow, displayCol], null);
                    temp.Add(squareVM);
                }
            }
            Squares = new ObservableCollection<SquareViewModel>(temp);
        }

        public void UpdateMoveMarkers()
        {
            foreach (var sq in Squares)
                sq.HasMoveMarker = false;
            if (SelectedSquare?.Piece == null)
                return;
            var moves = SelectedSquare.Piece.GetPossibleMoves(Board, Board.Squares[SelectedSquare.Row, SelectedSquare.Column]);
            foreach (var move in moves)
            {
                var targetVM = Squares.FirstOrDefault(s => s.Row == move.To.Row && s.Column == move.To.Column);
                if (targetVM != null)
                    targetVM.HasMoveMarker = true;
            }
        }
    }
}
