using Checkers.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class Move
    {
        public Square From { get; }
        public Square To { get; }
        public List<(Square Captured, Square Landing)> Captures { get; } // כל קפיצה: איזו חתיכה נאכלה ולאן נחתנו
        public List<Square> SquaresPath { get; } // כל הצעדים בסדר

        public bool IsCapture => Captures.Any();

        public Move(Square from, Square to, List<(Square Captured, Square Landing)>? captures = null, List<Square>? squaresPath = null)
        {
            From = from;
            To = to;
            Captures = captures ?? new List<(Square, Square)>();
            SquaresPath = squaresPath ?? new List<Square> { to };
        }
    }
}
