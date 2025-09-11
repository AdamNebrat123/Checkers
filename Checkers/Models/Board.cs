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
        public Square[,] Squares { get; private set; }

        public Board()
        {
            Squares = new Square[Size, Size];
            Initialize();
        }

        private void Initialize()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    bool isDark = (row + col) % 2 != 0;
                    var square = new Square
                    {
                        Row = row,
                        Column = col,
                        IsDark = isDark
                    };

                    if (isDark && row < 3)
                    {
                        square.Piece = new Piece { Color = PieceColor.White };
                    }
                    else if (isDark && row > 4)
                    {
                        square.Piece = new Piece { Color = PieceColor.Black };
                    }

                    Squares[row, col] = square;
                }
            }
        }
    }
}
