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
        private readonly GameManagerViewModel gameManager;
        private AIManager aiManager;
        public ObservableCollection<SquareViewModel> Squares { get; }

        private SquareViewModel? selectedSquare;
        public SquareViewModel? SelectedSquare
        {
            get => selectedSquare;
            set
            {
                    selectedSquare = value;
                    OnPropertyChanged();

                    if (SelectedSquare?.Piece == null)
                    {
                        UpdateMoveMarkers();
                        return;
                    }

                    var piece = SelectedSquare.Piece;

                    if (!gameManager.CanMove(piece))
                        return;

                    UpdateMoveMarkers();
            }
        }

        private readonly Board board;
        private readonly bool whitePerspective;

        public BoardViewModel(GameManagerViewModel gameManager, int depth, bool whitePerspective = true )
        {
            aiManager = new AIManager(PieceColor.Black, depth);
            this.gameManager = gameManager;
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

            if (!gameManager.CanMove(piece))
                return;

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

            await PlayerMoved();
        }

        private async Task ExecuteMove(Move move, SquareViewModel fromVM)
        {
            var piece = fromVM.Piece!;
            var currentVM = fromVM;

            // הסרת סמני מהלך
            foreach (var sq in Squares)
                sq.HasMoveMarker = false;

            foreach (var step in move.SquaresPath)
            {
                var nextVM = Squares.First(s => s.Row == step.Row && s.Column == step.Column);

                var capture = move.Captures.FirstOrDefault(c => c.Landing.Row == step.Row && c.Landing.Column == step.Column);
                if (capture.Captured != null)
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

            // קידום למלך
            if (piece is Man)
            {
                if ((piece.Color == PieceColor.White && currentVM.Row == 0) ||
                    (piece.Color == PieceColor.Black && currentVM.Row == Board.Size - 1))
                {
                    currentVM.Piece = new King(piece.Color);
                }
            }

            // החלפת תור
            gameManager.SwitchTurn();
        }




        public static UndoInfo MakeMove(Board board, Move move)
        {
            var fromSquare = move.From;
            var toSquare = move.To;
            var piece = fromSquare.Piece!;

            // שומרים מידע על captured pieces
            var capturedPieces = new List<(Square, Piece)>();
            foreach (var cap in move.Captures)
            {
                if (cap.Captured.Piece != null)
                    capturedPieces.Add((cap.Captured, cap.Captured.Piece));
                cap.Captured.Piece = null;
            }

            // הזזת החתיכה
            fromSquare.Piece = null;
            toSquare.Piece = piece;

            // קידום למלך אם צריך
            if (piece is Man)
            {
                if ((piece.Color == PieceColor.White && toSquare.Row == 0) ||
                    (piece.Color == PieceColor.Black && toSquare.Row == Board.Size - 1))
                {
                    toSquare.Piece = new King(piece.Color);
                }
            }

            return new UndoInfo(fromSquare, toSquare, piece, capturedPieces);
        }

        public static void UndoMove(Board board, UndoInfo undo)
        {
            // מחזירים את החתיכה למקום המקורי
            undo.To.Piece = null;
            undo.From.Piece = undo.MovedPiece;

            // מחזירים את כל החתיכות שנאכלו
            foreach (var (square, piece) in undo.CapturedPieces)
            {
                square.Piece = piece;
            }
        }
        private async Task MakeAIMove(AIManager ai)
        {
            int depth = 5; // אפשר לשחק עם depth גבוה יותר
            var bestMove = await ai.FindBestMoveAsync(board);

            if (bestMove == null) return;

            var fromVM = Squares.First(s => s.Row == bestMove.From.Row && s.Column == bestMove.From.Column);
            await ExecuteMove(bestMove, fromVM);
        }


        private async Task PlayerMoved()
        {
            gameManager.SwitchTurn();

            // אם עכשיו תור ה-AI
            if (!gameManager.IsWhiteTurn) // נניח שה-AI שחור
            {
                await MakeAIMove(aiManager); // aiManager צריך להיות מופע של AIManager
            }
        }
    }
}
