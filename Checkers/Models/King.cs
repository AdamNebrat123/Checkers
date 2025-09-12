using Checkers.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class King : Piece
    {
        public King(PieceColor color) : base(color) { }

        public override IEnumerable<Move> GetPossibleMoves(Board board, Square fromSquare)
        {
            var moves = new List<Move>();

            // ארבעת האלכסונים
            int[][] directions = new int[][]
            {
                new[] { -1, -1 },
                new[] { -1,  1 },
                new[] {  1, -1 },
                new[] {  1,  1 }
            };

            foreach (var dir in directions)
            {
                int row = fromSquare.Row + dir[0];
                int col = fromSquare.Column + dir[1];
                bool enemyFound = false;
                Square? enemySquare = null;

                while (board.IsInsideBoard(row, col))
                {
                    var sq = board.Squares[row, col];

                    if (sq.Piece == null)
                    {
                        if (!enemyFound)
                        {
                            // תנועה רגילה (אם לא מצאנו אויב קודם)
                            moves.Add(new Move(fromSquare, sq));
                        }
                        else
                        {
                            // אחרי אויב → קפיצה
                            moves.Add(new Move(fromSquare, sq, true, enemySquare!));
                        }
                    }
                    else
                    {
                        if (sq.Piece.Color == this.Color) break; // חסום ע"י חבר
                        if (enemyFound) break; // כבר יש אויב אחד, אי אפשר לעבור
                        enemyFound = true;
                        enemySquare = sq;
                    }

                    row += dir[0];
                    col += dir[1];
                }
            }

            return moves;
        }
    }
}
