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
        public bool IsCapture { get; }
        public Square? CapturedSquare { get; }

        public Move(Square from, Square to, bool isCapture = false, Square? capturedSquare = null)
        {
            From = from;
            To = to;
            IsCapture = isCapture;
            CapturedSquare = capturedSquare;
        }
    }
}
