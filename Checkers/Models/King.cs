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
            var results = new List<Move>();

            // 1. מחשבים multi-capture בזיגזג
            ExploreCaptures(board, fromSquare,
                            new List<(Square Captured, Square Landing)>(),
                            new List<Square> { fromSquare },
                            results);

            // 2. אם אין קפיצות → מהלכים רגילים לאורך כל האלכסונים
            if (!results.Any())
            {
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

                    while (board.IsInsideBoard(row, col))
                    {
                        var sq = board.Squares[row, col];
                        if (sq.Piece != null) break; // חסום ע"י חבר או אויב
                        results.Add(new Move(fromSquare, sq));
                        row += dir[0];
                        col += dir[1];
                    }
                }
            }

            return results;
        }

        private void ExploreCaptures(Board board, Square current,
    List<(Square Captured, Square Landing)> capturedSoFar,
    List<Square> pathSoFar,
    List<Move> results)
        {
            int[][] directions = new int[][]
            {
        new[] { -1, -1 },
        new[] { -1,  1 },
        new[] {  1, -1 },
        new[] {  1,  1 }
            };

            foreach (var dir in directions)
            {
                int row = current.Row + dir[0];
                int col = current.Column + dir[1];
                Square? enemySquare = null;

                while (board.IsInsideBoard(row, col))
                {
                    var sq = board.Squares[row, col];

                    if (sq.Piece == null)
                    {
                        if (enemySquare != null && !capturedSoFar.Any(c => c.Captured == enemySquare))
                        {
                            // אפשר לנחות על כל ריבוע פנוי אחרי האויב
                            int landingRow = row;
                            int landingCol = col;
                            while (board.IsInsideBoard(landingRow, landingCol) && board.Squares[landingRow, landingCol].Piece == null)
                            {
                                var landingSquare = board.Squares[landingRow, landingCol];

                                var newCaptured = new List<(Square Captured, Square Landing)>(capturedSoFar)
                        {
                            (enemySquare, landingSquare)
                        };
                                var newPath = new List<Square>(pathSoFar) { landingSquare };

                                // מוסיפים Move
                                results.Add(new Move(pathSoFar.First(), landingSquare, newCaptured, newPath));

                                // רקורסיה להמשך multi-capture
                                ExploreCaptures(board, landingSquare, newCaptured, newPath, results);

                                // המשך לכיוון להוספת אפשרות לנחות עוד ריבוע
                                landingRow += dir[0];
                                landingCol += dir[1];
                            }

                            break; // אסור להמשיך באותו כיוון אחרי אויב
                        }

                        row += dir[0];
                        col += dir[1];
                    }
                    else
                    {
                        if (sq.Piece.Color == this.Color || enemySquare != null)
                        {
                            break; // חסום או כבר היה אויב
                        }
                        enemySquare = sq;
                        row += dir[0];
                        col += dir[1];
                    }
                }
            }
        }
    }


}
