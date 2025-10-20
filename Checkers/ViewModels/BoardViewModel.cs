using Checkers.Model;
using Checkers.Models;
using Checkers.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Checkers.ViewModel
{
    public class BoardViewModel : ViewModelBase
    {
        public ObservableCollection<SquareViewModel> Squares { get; }
        public readonly GameManagerViewModel gameManager;
        private readonly Board board;

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

        public BoardViewModel(GameManagerViewModel gameManager)
        {
            this.gameManager = gameManager;
            board = new Board();

            Squares = new ObservableCollection<SquareViewModel>(
                Enumerable.Range(0, Board.Size)
                    .SelectMany(row => Enumerable.Range(0, Board.Size)
                        .Select(col => new SquareViewModel(board.Squares[row, col], SquareSelected)))
            );
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
            {
                _ = MovePieceAsync(squareVM);
            }
            else
            {
                SelectedSquare = squareVM;
            }
        }

        // אירוע שמודיע ל־GameViewModel על מהלך שהושלם
        public event Func<GameMove, Task>? OnPlayerMoveCompleted;

        private async Task MovePieceAsync(SquareViewModel targetSquare)
        {
            if (SelectedSquare?.Piece == null) return;

            var piece = SelectedSquare.Piece;
            var moves = piece.GetPossibleMoves(board, board.Squares[SelectedSquare.Row, SelectedSquare.Column]);
            var move = moves.FirstOrDefault(m => m.To.Row == targetSquare.Row && m.To.Column == targetSquare.Column);
            if (move == null) return;

            foreach (var sq in Squares) sq.HasMoveMarker = false;
            SelectedSquare = null;

            var currentVM = Squares.First(s => s.Row == move.From.Row && s.Column == move.From.Column);

            foreach (var step in move.SquaresPath)
            {
                var nextVM = Squares.First(s => s.Row == step.Row && s.Column == step.Column);

                var capture = move.Captures.FirstOrDefault(c => c.Landing.Row == step.Row && c.Landing.Column == step.Column);
                if (capture != default && capture.Captured != null)
                {
                    var capturedVM = Squares.First(s => s.Row == capture.Captured.Row && s.Column == capture.Captured.Column);
                    capturedVM.Piece = null;
                }

                currentVM.Piece = null;
                nextVM.Piece = piece;
                nextVM.RaisePieceImageChanged();
                currentVM = nextVM;

                await Task.Delay(700);
            }

            if (piece is Man && ((piece.Color == PieceColor.White && currentVM.Row == 0) || (piece.Color == PieceColor.Black && currentVM.Row == Board.Size - 1)))
            {
                currentVM.Piece = new King(piece.Color);
            }

            if (OnPlayerMoveCompleted != null)
            {
                await OnPlayerMoveCompleted.Invoke(new GameMove
                {
                    fromRow = move.From.Row,
                    fromCol = move.From.Column,
                    toRow = move.To.Row,
                    toCol = move.To.Column,
                    captures = move.Captures
                    .Where(c => c.Captured != null)
                    .Select(c => (c.Captured.Row, c.Captured.Column))
                    .ToList(),
                    isWhiteTurn = gameManager.IsWhiteTurn
                });
            }
        }

        public async Task ApplyMove(GameMove move)
        {
            var fromVM = Squares.FirstOrDefault(s => s.Row == move.fromRow && s.Column == move.fromCol);
            var toVM = Squares.FirstOrDefault(s => s.Row == move.toRow && s.Column == move.toCol);
            if (fromVM == null || toVM == null || fromVM.Piece == null) return;

            var piece = fromVM.Piece;

            if (move.captures != null)
            {
                foreach (var cap in move.captures)
                {
                    var capturedVM = Squares.FirstOrDefault(s => s.Row == cap.row && s.Column == cap.col);
                    if (capturedVM != null) capturedVM.Piece = null;
                }
            }

            fromVM.Piece = null;
            toVM.Piece = piece;
            toVM.RaisePieceImageChanged();

            if (piece is Man && ((piece.Color == PieceColor.White && toVM.Row == 0) || (piece.Color == PieceColor.Black && toVM.Row == Board.Size - 1)))
            {
                toVM.Piece = new King(piece.Color);
            }
        }
    }
}
