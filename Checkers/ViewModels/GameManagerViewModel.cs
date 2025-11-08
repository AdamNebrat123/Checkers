using Checkers.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.ViewModels
{
    public class GameManagerViewModel
    {
        public event Action TurnSwitched;
        public bool IsWhiteTurn { get; set; } = true; // לבן מתחיל

        public GameManagerViewModel()
        {

        }

        public void SwitchTurn()
        {
            IsWhiteTurn = !IsWhiteTurn;
            InvokeTurnSwitched();
        }

        private void InvokeTurnSwitched()
        {
            TurnSwitched?.Invoke();
        }

        public bool CanMove(Piece piece)
        {
            // רק אם הכלי תואם לתור הנוכחי
            bool canMove = false;
            if (IsWhiteTurn && piece.Color == PieceColor.White)
                canMove = true;
            if (!IsWhiteTurn && piece.Color == PieceColor.Black)
                canMove = true;

            return canMove;
        }
    }

}
