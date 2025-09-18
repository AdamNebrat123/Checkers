using Checkers.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public struct UndoInfo
    {
        public Square From;
        public Square To;
        public Piece? MovedPiece;
        public List<(Square Captured, Piece Piece)> CapturedPieces; // מה היה כל captured

        public UndoInfo(Square from, Square to, Piece? movedPiece, List<(Square Captured, Piece Piece)> capturedPieces)
        {
            From = from;
            To = to;
            MovedPiece = movedPiece;
            CapturedPieces = capturedPieces;
        }
    }
}
