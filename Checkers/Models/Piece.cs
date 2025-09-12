using Checkers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Model
{
    public enum PieceColor
    {
        White,
        Black
    }

    public abstract class Piece
    {
        public PieceColor Color { get; }
        protected Piece(PieceColor color)
        {
            Color = color;
        }

        // כל Piece חייב לממש את הפונקציה הזו
        public abstract IEnumerable<Move> GetPossibleMoves(Board board, Square fromSquare);
    }
}
