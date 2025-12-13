using Checkers.GameLogic;
using Checkers.Model;
using Checkers.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class AIManager
    {
        public PieceColor AIColor { get; }
        private int depth;

        public AIManager(PieceColor aiColor, int depth)
        {
            AIColor = aiColor;
            this.depth = depth;
        }

        public List<Move> GetAllMoves(Board board, PieceColor color)
        {
            var moves = new List<Move>();
            foreach (var square in board.Squares)
            {
                if (square.Piece != null && square.Piece.Color == color)
                {
                    moves.AddRange(square.Piece.GetPossibleMoves(board, square));
                }
            }
            return moves;
        }

        public async Task<Move?> FindBestMoveAsync(Board board)
        {
            return await Task.Run(() =>
            {
                Move? bestMove = null;
                int bestValue = int.MinValue;

                foreach (var move in GetAllMoves(board, AIColor))
                {
                    var undo = AiGameStrategy.MakeMove(board, move, AIColor);
                    int moveValue = Minimax(board, depth - 1, false, int.MinValue, int.MaxValue, AIColor);
                    AiGameStrategy.UndoMove(board, undo);

                    if (moveValue > bestValue)
                    {
                        bestValue = moveValue;
                        bestMove = move;
                    }
                }

                return bestMove;
            });
        }

        private int Minimax(Board board, int depth, bool isMaximizingPlayer, int alpha, int beta, PieceColor aiColor)
        {
            if (depth == 0 || GameOver(board))
                return EvaluateBoard(board, aiColor);

            if (isMaximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (var move in GetAllMoves(board, aiColor))
                {
                    var undo = AiGameStrategy.MakeMove(board, move, AIColor);
                    int eval = Minimax(board, depth - 1, false, alpha, beta, aiColor);
                    AiGameStrategy.UndoMove(board, undo);

                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break; // חותכים ענפים מיותרים
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                PieceColor humanColor = aiColor == PieceColor.White ? PieceColor.Black : PieceColor.White;

                foreach (var move in GetAllMoves(board, humanColor))
                {
                    var undo = AiGameStrategy.MakeMove(board, move, AIColor);
                    int eval = Minimax(board, depth - 1, true, alpha, beta, aiColor);
                    AiGameStrategy.UndoMove(board, undo);

                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                        break; // חותכים ענפים מיותרים
                }
                return minEval;
            }
        }

        private int EvaluateBoard(Board board, PieceColor aiColor)
        {
            int score = 0;
            foreach (var sq in board.Squares)
            {
                if (sq.Piece == null) continue;
                int value = sq.Piece is King ? 30 : 10;
                score += sq.Piece.Color == aiColor ? value : -value;
            }
            return score;
        }

        private bool GameOver(Board board)
        {
            return !GetAllMoves(board, AIColor).Any() ||
                   !GetAllMoves(board, AIColor == PieceColor.White ? PieceColor.Black : PieceColor.White).Any();
        }
    }

}
