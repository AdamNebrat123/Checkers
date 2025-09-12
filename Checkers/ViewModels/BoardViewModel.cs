using Checkers.Model;
using Checkers.Models;
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
                    UpdateMoveMarkers();
                }
            }
        }

        private readonly Board board;
        //private readonly GameRules rules;

        public BoardViewModel()
        {
            board = new Board();

            var temp = new List<SquareViewModel>();

            for (int row = 0; row < Board.Size; row++)
            {
                for (int col = 0; col < Board.Size; col++)
                {
                    var squareVM = new SquareViewModel(board.Squares[row, col], SquareSelected);
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

            // לוקחים את המהלכים מה־Piece עצמו
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


        private void MovePiece(SquareViewModel targetSquare)
        {
            if (SelectedSquare?.Piece == null) return;

            var piece = SelectedSquare.Piece;

            var moves = piece.GetPossibleMoves(board, board.Squares[SelectedSquare.Row, SelectedSquare.Column]);
            var move = moves.FirstOrDefault(m => m.To.Row == targetSquare.Row && m.To.Column == targetSquare.Column);

            if (move == null) return;

            // 1. הזזת החתיכה
            targetSquare.Piece = piece;
            SelectedSquare.Piece = null;

            // 2. הסרת חתיכה שנתפסה (אם קיימת)
            if (move.IsCapture && move.CapturedSquare != null)
            {
                var capturedVM = Squares.FirstOrDefault(s =>
                    s.Row == move.CapturedSquare.Row &&
                    s.Column == move.CapturedSquare.Column);

                if (capturedVM != null)
                    capturedVM.Piece = null;
            }

            // 3. קידום למלך
            if (piece is Man)
            {
                if ((piece.Color == PieceColor.White && targetSquare.Row == 0) ||
                    (piece.Color == PieceColor.Black && targetSquare.Row == Board.Size - 1))
                {
                    targetSquare.Piece = new King(piece.Color);
                }
            }

            // 4. ניקוי סמנים
            foreach (var sq in Squares)
                sq.HasMoveMarker = false;

            SelectedSquare = null;
        }
    }
}
