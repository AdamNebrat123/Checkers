using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class GameMove
    {
        public string Id { get; set; }

        public int FromRow { get; set; }
        public int FromCol { get; set; }

        public int ToRow { get; set; }
        public int ToCol { get; set; }

        // רשימת אכילות (תומכת בריבוי קפיצות)
        public List<CaptureStep> Captures { get; set; } = new();

        // מי ביצע את המהלך
        public bool WasWhite { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
