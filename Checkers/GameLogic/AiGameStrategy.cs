using Checkers.Model;
using Checkers.Models;
using Checkers.Services;
using Checkers.ViewModel;
using Checkers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public class AiGameStrategy : IGameStrategy
    {
        private readonly IMusicService _musicService = IPlatformApplication.Current.Services.GetRequiredService<IMusicService>();
        private BoardViewModel boardVM;
        private readonly GameManagerViewModel gameManager;
        private readonly AIManager aiManager;

        public AiGameStrategy(GameManagerViewModel gameManager, int depth, bool whitePerspective)
        {
            this.gameManager = gameManager;
            PieceColor aiColor = whitePerspective ? PieceColor.Black : PieceColor.White;
            aiManager = new AIManager(aiColor, depth);
        }
        public void SetBoardViewModel(BoardViewModel boardVM)
        {
            this.boardVM = boardVM;
        }

        public async Task InitializeAsync(BoardViewModel boardVM)
        {
            // אם ה-AI מתחיל ראשון
            if (aiManager.AIColor == PieceColor.White && gameManager.IsWhiteTurn)
                await MakeAIMoveAsync(boardVM);
        }

        public async Task HandleSquareSelectedAsync(BoardViewModel boardVM, SquareViewModel squareVM)
        {
            if (squareVM.HasMoveMarker)
                MovePiece(squareVM);
            else
                boardVM.SelectedSquare = squareVM;
        }

        public async void MovePiece(SquareViewModel targetSquare)
        {
            if (boardVM == null) return;
            if (boardVM.SelectedSquare?.Piece == null) return;

            var piece = boardVM.SelectedSquare.Piece;

            if (!gameManager.CanMove(piece))
            {
                _musicService.Play(SfxEnum.illegal.ToString(), false);

                return;
            }
            var moves = piece.GetPossibleMoves(boardVM.Board, boardVM.Board.Squares[boardVM.SelectedSquare.Row, boardVM.SelectedSquare.Column]);
            var move = moves.FirstOrDefault(m => m.To.Row == targetSquare.Row && m.To.Column == targetSquare.Column);

            if (move == null) return;

            // הסרת סמני מהלך מיד
            foreach (var sq in boardVM.Squares)
                sq.HasMoveMarker = false;

            boardVM.SelectedSquare = null;

            var currentVM = boardVM.Squares.First(s => s.Row == move.From.Row && s.Column == move.From.Column);

            foreach (var step in move.SquaresPath)
            {
                // מחפשים את ה־VM של הצעד הבא
                var nextVM = boardVM.Squares.First(s => s.Row == step.Row && s.Column == step.Column);

                // בודקים אם צעד זה עובר מעל חתיכה שנתפסה
                var capture = move.Captures.FirstOrDefault(c => c.Landing.Row == step.Row && c.Landing.Column == step.Column);
                if (capture.Captured != null)
                {
                    var capturedVM = boardVM.Squares.First(s => s.Row == capture.Captured.Row && s.Column == capture.Captured.Column);
                    capturedVM.Piece = null;
                }

                // הזזת החתיכה
                currentVM.Piece = null;
                nextVM.Piece = piece;
                nextVM.RaisePieceImageChanged();
                currentVM = nextVM;


                if (move.SquaresPath.Count == 1)
                {
                    _musicService.Play(SfxEnum.move_self.ToString(), false);
                }
                else
                {
                    _musicService.Play(SfxEnum.capture.ToString(), false);
                }
                await Task.Delay(700); // דיליי בין כל צעד
            }

            // קידום למלכה
            if (piece is Man)
            {
                if ((piece.Color == PieceColor.White && currentVM.Row == 0) ||
                    (piece.Color == PieceColor.Black && currentVM.Row == Board.Size - 1))
                {
                    currentVM.Piece = new King(piece.Color);
                    _musicService.Play(SfxEnum.promote.ToString(), false);

                }
            }

            await PlayerMoved();
        }

        private async Task MakeAIMoveAsync(BoardViewModel boardVM)
        {
            if (boardVM == null) return;
            var bestMove = await aiManager.FindBestMoveAsync(boardVM.Board);
            if (bestMove == null) return;
            await ExecuteMoveAsync(boardVM, bestMove);
            gameManager.SwitchTurn();
        }

        private async Task ExecuteMoveAsync(BoardViewModel boardVM, Move move)
        {
            var squares = boardVM.Squares;
            var piece = move.From.Piece!;
            var currentVM = squares.First(s => s.Row == move.From.Row && s.Column == move.From.Column);

            foreach (var step in move.SquaresPath)
            {
                var nextVM = squares.First(s => s.Row == step.Row && s.Column == step.Column);
                var capture = move.Captures.FirstOrDefault(c => c.Landing.Row == step.Row && c.Landing.Column == step.Column);
                if (capture.Captured != null)
                {
                    var capturedVM = squares.First(s => s.Row == capture.Captured.Row && s.Column == capture.Captured.Column);
                    capturedVM.Piece = null;
                }

                currentVM.Piece = null;
                nextVM.Piece = piece;
                nextVM.RaisePieceImageChanged();
                currentVM = nextVM;

                await Task.Delay(700);
            }

            // קידום למלכה
            if (piece is Man)
            {
                if ((piece.Color == PieceColor.White && currentVM.Row == 0) ||
                    (piece.Color == PieceColor.Black && currentVM.Row == Board.Size - 1))
                {
                    _musicService.Play(SfxEnum.promote.ToString(), false);
                    currentVM.Piece = new King(piece.Color);
                }
            }
        }

        private async Task PlayerMoved()
        {
            // switch turn after the player moved
            gameManager.SwitchTurn();

            if (boardVM == null) return;

            // אם עכשיו תור ה-AI לפי הצבע שלו
            if (gameManager.IsWhiteTurn == (aiManager.AIColor == PieceColor.White))
            {
                await MakeAIMoveAsync(boardVM);
            }
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

    }

}
