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
            var results = new List<Move>();

            // 1. מחשבים multi-capture זיגזג
            ExploreCaptures(board, fromSquare, new List<Square>(), new List<Square> { fromSquare }, results);

            // 2. אם אין אכילות כלל → צעדים רגילים
            if (!results.Any())
            {
                int[][] directions = new int[][]
                {
            new[] { -1, -1 }, new[] { -1, 1 },
            new[] { 1, -1 },  new[] { 1, 1 }
                };

                foreach (var dir in directions)
                {
                    int newRow = fromSquare.Row + dir[0];
                    int newCol = fromSquare.Column + dir[1];

                    if (board.IsInsideBoard(newRow, newCol) && board.Squares[newRow, newCol].Piece == null)
                    {
                        results.Add(new Move(fromSquare, board.Squares[newRow, newCol]));
                    }
                }
            }

            return results;
        }

        private void ExploreCaptures(Board board, Square current, List<Square> capturedSoFar, List<Square> pathSoFar, List<Move> results)
        {
            bool anyCapture = false;

            int[][] directions = new int[][]
            {
        new[] { -1, -1 }, new[] { -1, 1 },
        new[] { 1, -1 },  new[] { 1, 1 }
            };

            foreach (var dir in directions)
            {
                int midRow = current.Row + dir[0];
                int midCol = current.Column + dir[1];
                int landingRow = current.Row + 2 * dir[0];
                int landingCol = current.Column + 2 * dir[1];

                if (!board.IsInsideBoard(landingRow, landingCol)) continue;

                var midSquare = board.Squares[midRow, midCol];
                var landingSquare = board.Squares[landingRow, landingCol];

                if (midSquare.Piece != null &&
                    midSquare.Piece.Color != this.Color &&
                    landingSquare.Piece == null &&
                    !capturedSoFar.Contains(midSquare))
                {
                    anyCapture = true;

                    var newCaptured = new List<Square>(capturedSoFar) { midSquare };
                    var newPath = new List<Square>(pathSoFar) { landingSquare };

                    // רץ רקורסיה לבדוק המשך קפיצות
                    ExploreCaptures(board, landingSquare, newCaptured, newPath, results);

                    // מוסיפים גם אפשרות לאכול רק את הקפיצה הנוכחית (לא חובה להמשיך)
                    var capturesList = new List<(Square Captured, Square Landing)>();
                    for (int i = 0; i < newCaptured.Count; i++)
                    {
                        capturesList.Add((newCaptured[i], newPath[i + 1]));
                    }
                    results.Add(new Move(pathSoFar.First(), landingSquare, capturesList, newPath));
                }
            }

            // אם אין עוד קפיצות → לא מוסיפים כלום כי כל מה שהצטבר כבר נוסף
        }
    }

}
