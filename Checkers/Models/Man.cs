using Checkers.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class Man : Piece
    {
        public Man(PieceColor color) : base(color) { }

        public override IEnumerable<Move> GetPossibleMoves(Board board, Square fromSquare)
        {
            var moves = new List<Move>();

            // ארבעת הכיוונים האפשריים (כולל אחורה כי ב־International מותר גם לאחור)
            int[][] directions = new int[][]
            {
                new[] { -1, -1 },
                new[] { -1,  1 },
                new[] {  1, -1 },
                new[] {  1,  1 }
            };

            foreach (var dir in directions)
            {
                int newRow = fromSquare.Row + dir[0];
                int newCol = fromSquare.Column + dir[1];

                // צעד רגיל
                if (board.IsInsideBoard(newRow, newCol) && board.Squares[newRow, newCol].Piece == null)
                {
                    moves.Add(new Move(fromSquare, board.Squares[newRow, newCol]));
                }

                // קפיצה (אכילה)
                int jumpRow = fromSquare.Row + 2 * dir[0];
                int jumpCol = fromSquare.Column + 2 * dir[1];

                if (board.IsInsideBoard(jumpRow, jumpCol) &&
                    board.Squares[newRow, newCol].Piece != null &&
                    board.Squares[newRow, newCol].Piece.Color != this.Color &&
                    board.Squares[jumpRow, jumpCol].Piece == null)
                {
                    moves.Add(new Move(fromSquare, board.Squares[jumpRow, jumpCol], true, board.Squares[newRow, newCol]));
                }
            }

            return moves;
        }
    }
}
