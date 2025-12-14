using Checkers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Model
{
    public class Board
    {
        public const int Size = 8;
        public Square[,] Squares { get; }
        public Board()
        {
            
        }
        public Board(bool isWhitePerspective = true)
        {
            Squares = new Square[Size, Size];

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    bool isDark = (row + col) % 2 != 0;
                    Squares[row, col] = new Square(row, col, isDark);
                }
            }

            if (isWhitePerspective) SetupInitialPiecesWhtitePerspective();
            else SetupInitialPiecesBlackPerspective();
        }

        private void SetupInitialPiecesWhtitePerspective()
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (Squares[row, col].IsDark)
                        Squares[row, col].Piece = new Man(PieceColor.Black);
                }
            }

            for (int row = Size - 3; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (Squares[row, col].IsDark)
                        Squares[row, col].Piece = new Man(PieceColor.White);
                }
            }
        }
        private void SetupInitialPiecesBlackPerspective()
        {
            // White pieces at the top
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (Squares[row, col].IsDark)
                        Squares[row, col].Piece = new Man(PieceColor.White);
                }
            }

            // Black pieces at the bottom
            for (int row = Size - 3; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (Squares[row, col].IsDark)
                        Squares[row, col].Piece = new Man(PieceColor.Black);
                }
            }
        }

        public bool IsInsideBoard(int row, int col)
        {
            return row >= 0 && row < Size && col >= 0 && col < Size;
        }
    }
}
