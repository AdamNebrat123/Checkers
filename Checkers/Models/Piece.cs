using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Model
{
    public enum PieceColor
    {
        White,
        Black
    }

    public class Piece
    {
        public PieceColor Color { get; }
        public bool IsKing { get; set; }

        public Piece(PieceColor color, bool isKing = false)
        {
            Color = color;
            IsKing = isKing;
        }
    }
}
