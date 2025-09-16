using Checkers.Model;
using Checkers.Models;
using Checkers.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

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
                    UpdateMoveMarkers();
                }
            }
        }

        private readonly Board board;
        private readonly bool whitePerspective;

        public BoardViewModel(bool whitePerspective = true)
        {
            this.whitePerspective = whitePerspective;
            board = new Board();

            var temp = new List<SquareViewModel>();

            for (int row = 0; row < Board.Size; row++)
            {
                for (int col = 0; col < Board.Size; col++)
                {
                    int displayRow = whitePerspective ? row : Board.Size - 1 - row;
                    int displayCol = whitePerspective ? col : Board.Size - 1 - col;

                    var squareVM = new SquareViewModel(board.Squares[displayRow, displayCol], SquareSelected);
                    temp.Add(squareVM);
                }
            }

            Squares = new ObservableCollection<SquareViewModel>(temp);
        }

        private void UpdateMoveMarkers()
        {
            foreach (var sq in Squares)
                sq.HasMoveMarker = false;

            if (SelectedSquare?.Piece == null) return;

            var moves = SelectedSquare.Piece.GetPossibleMoves(board, board.Squares[SelectedSquare.Row, SelectedSquare.Column]);

            foreach (var move in moves)
            {
                var targetVM = Squares.FirstOrDefault(s => s.Row == move.To.Row && s.Column == move.To.Column);
                if (targetVM != null)
                    targetVM.HasMoveMarker = true;
            }
        }

        public void SquareSelected(SquareViewModel squareVM)
        {
            if (squareVM.HasMoveMarker)
                MovePiece(squareVM);
            else
                SelectedSquare = squareVM;
        }

        private async void MovePiece(SquareViewModel targetSquare)
        {
            if (SelectedSquare?.Piece == null) return;

            var piece = SelectedSquare.Piece;

            var moves = piece.GetPossibleMoves(board, board.Squares[SelectedSquare.Row, SelectedSquare.Column]);
            var move = moves.FirstOrDefault(m => m.To.Row == targetSquare.Row && m.To.Column == targetSquare.Column);

            if (move == null) return;

            // הסרת סמני מהלך מיד
            foreach (var sq in Squares)
                sq.HasMoveMarker = false;

            SelectedSquare = null;

            var currentVM = Squares.First(s => s.Row == move.From.Row && s.Column == move.From.Column);

            foreach (var step in move.SquaresPath)
            {
                // מחפשים את ה־VM של הצעד הבא
                var nextVM = Squares.First(s => s.Row == step.Row && s.Column == step.Column);

                // בודקים אם צעד זה עובר מעל חתיכה שנתפסה
                var capture = move.Captures.FirstOrDefault(c => c.Landing.Row == step.Row && c.Landing.Column == step.Column);
                if (capture.Captured != null)
                {
                    var capturedVM = Squares.First(s => s.Row == capture.Captured.Row && s.Column == capture.Captured.Column);
                    capturedVM.Piece = null;
                }

                // הזזת החתיכה
                currentVM.Piece = null;
                nextVM.Piece = piece;
                nextVM.RaisePieceImageChanged();
                currentVM = nextVM;

                await Task.Delay(700); // דיליי בין כל צעד
            }

            // קידום למלך
            if (piece is Man)
            {
                if ((piece.Color == PieceColor.White && currentVM.Row == 0) ||
                    (piece.Color == PieceColor.Black && currentVM.Row == Board.Size - 1))
                {
                    currentVM.Piece = new King(piece.Color);
                }
            }
        }
    }
}
