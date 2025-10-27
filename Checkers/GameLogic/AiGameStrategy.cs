using Checkers.Model;
using Checkers.Models;
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
        private readonly GameManagerViewModel gameManager;
        private readonly AIManager aiManager;

        public AiGameStrategy(GameManagerViewModel gameManager, int depth, bool whitePerspective)
        {
            this.gameManager = gameManager;
            PieceColor aiColor = whitePerspective ? PieceColor.Black : PieceColor.White;
            aiManager = new AIManager(aiColor, depth);
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
                await MovePieceAsync(boardVM, squareVM);
            else
                boardVM.SelectedSquare = squareVM;
        }

        private async Task MovePieceAsync(BoardViewModel boardVM, SquareViewModel targetSquare)
        {
            var board = boardVM.board;
            var selected = boardVM.SelectedSquare;
            if (selected?.Piece == null) return;

            var piece = selected.Piece;
            if (!gameManager.CanMove(piece)) return;

            var moves = piece.GetPossibleMoves(board, board.Squares[selected.Row, selected.Column]);
            var move = moves.FirstOrDefault(m => m.To.Row == targetSquare.Row && m.To.Column == targetSquare.Column);
            if (move == null) return;

            await ExecuteMoveAsync(boardVM, move);

            gameManager.SwitchTurn();

            // תור ה־AI
            if (gameManager.IsWhiteTurn == (aiManager.AIColor == PieceColor.White))
                await MakeAIMoveAsync(boardVM);
        }

        private async Task MakeAIMoveAsync(BoardViewModel boardVM)
        {
            var board = boardVM.board;
            var bestMove = await aiManager.FindBestMoveAsync(board);
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
