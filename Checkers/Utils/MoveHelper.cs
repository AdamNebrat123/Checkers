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

            // השחור צריך להפוך את כל הקואורדינטות
            return new GameMove
            {
                Id = gameMove.Id,
                WasWhite = gameMove.WasWhite,
                FromRow = BoardSize - 1 - gameMove.FromRow,
                FromCol = BoardSize - 1 - gameMove.FromCol,
                ToRow = BoardSize - 1 - gameMove.ToRow,
                ToCol = BoardSize - 1 - gameMove.ToCol,
                EatenRow = gameMove.EatenRow.HasValue ? BoardSize - 1 - gameMove.EatenRow.Value : null,
                EatenCol = gameMove.EatenCol.HasValue ? BoardSize - 1 - gameMove.EatenCol.Value : null
            };
        }

        public static GameMove? ConvertMoveByPerspective(GameMove gameMove, bool isLocalPlayerWhite)
        {
            return isLocalPlayerWhite
                ? GetLastMoveForWhite(gameMove)
                : GetLastMoveForBlack(gameMove);
        }
    }
}
