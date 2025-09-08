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
        public PieceColor Color { get; set; }
        public bool IsKing { get; set; }
    }
}
