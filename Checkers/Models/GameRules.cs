using Checkers.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class GameRules
    {
        private readonly Board board;

        public GameRules(Board board)
        {
            this.board = board;
        }

        public IEnumerable<Move> GetPossibleMoves(Piece piece)
        {
            var moves = new List<Move>();
            var fromSquare = FindSquare(piece);
            if (fromSquare == null) return moves;

            int direction = piece.Color == PieceColor.White ? -1 : 1;
            var deltas = new List<(int dRow, int dCol)> { (direction, -1), (direction, 1) };

            if (piece.IsKing)
            {
                deltas.Add((-direction, -1));
                deltas.Add((-direction, 1));
            }

            foreach (var (dRow, dCol) in deltas)
            {
                int newRow = fromSquare.Row + dRow;
                int newCol = fromSquare.Column + dCol;

                if (!board.IsInsideBoard(newRow, newCol)) continue;

                var targetSquare = board.Squares[newRow, newCol];

                if (targetSquare.Piece == null)
                {
                    moves.Add(new Move(fromSquare, targetSquare));
                }
                else if (targetSquare.Piece.Color != piece.Color)
                {
                    int jumpRow = newRow + dRow;
                    int jumpCol = newCol + dCol;
                    if (board.IsInsideBoard(jumpRow, jumpCol) && board.Squares[jumpRow, jumpCol].Piece == null)
                    {
                        moves.Add(new Move(fromSquare, board.Squares[jumpRow, jumpCol], true, targetSquare));
                    }
                }
            }

            return moves;
        }

        private Square? FindSquare(Piece piece)
        {
            var square = board.Squares.Cast<Square>().FirstOrDefault(s => s.Piece == piece);
            return square;
        }
    }
}
