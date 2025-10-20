using System.Collections.Generic;

namespace Checkers.Models
{
    public class GameMove
    {
        public int fromRow { get; set; }
        public int fromCol { get; set; }
        public int toRow { get; set; }
        public int toCol { get; set; }
        public List<(int row, int col)> captures { get; set; } = new();
        public bool promoted { get; set; }  // האם החתיכה קודמה למלך
        public bool isWhiteTurn { get; set; } // תור השחקן הבא
    }
}
