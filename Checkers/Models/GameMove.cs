using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class GameMove
    {
        public int FromRow { get; set; }
        public int FromCol { get; set; }

        public int ToRow { get; set; }
        public int ToCol { get; set; }

        // המיקום של החייל שנאכל (null אם אין)
        public int? EatenRow { get; set; }
        public int? EatenCol { get; set; }

        // מי ביצע את המהלך
        public bool WasWhite { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
