using Checkers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Utils
{
    public static class MoveHelper
    {
        private static int BoardSize = 8;
        public static GameMove? GetLastMoveForWhite(GameMove gameMove)
        {
            if (gameMove == null)
                return null;

            // הלבן רואה את הכל כמו ב־Firebase, אין שינוי
            return gameMove;
        }

        public static GameMove? GetLastMoveForBlack(GameMove gameMove)
        {
            if (gameMove == null)
                return null;

            var mirroredMove = new GameMove
            {
                Id = gameMove.Id,
                WasWhite = gameMove.WasWhite,
                FromRow = BoardSize - 1 - gameMove.FromRow,
                FromCol = BoardSize - 1 - gameMove.FromCol,
                ToRow = BoardSize - 1 - gameMove.ToRow,
                ToCol = BoardSize - 1 - gameMove.ToCol,
                Timestamp = gameMove.Timestamp
            };

            // הפיכת כל שלב אכילה בנפרד
            if (gameMove.Captures != null && gameMove.Captures.Count > 0)
            {
                foreach (var capture in gameMove.Captures)
                {
                    mirroredMove.Captures.Add(new CaptureStep
                    {
                        CapturedRow = BoardSize - 1 - capture.CapturedRow,
                        CapturedCol = BoardSize - 1 - capture.CapturedCol,
                        LandingRow = BoardSize - 1 - capture.LandingRow,
                        LandingCol = BoardSize - 1 - capture.LandingCol
                    });
                }
            }

            return mirroredMove;
        }


        public static GameMove? ConvertMoveByPerspective(GameMove gameMove, bool isLocalPlayerWhite)
        {
            return isLocalPlayerWhite
                ? GetLastMoveForWhite(gameMove)
                : GetLastMoveForBlack(gameMove);
        }
    }
}
