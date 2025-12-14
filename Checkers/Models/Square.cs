using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Model
{
    public class Square
    {
        public int Row { get; }
        public int Column { get; }
        public bool IsDark { get; }
        public Piece? Piece { get; set; }

        public Square(int row, int col, bool isDark)
        {
            Row = row;
            Column = col;
            IsDark = isDark;
        }

        
    }
}
