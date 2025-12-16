using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class GameReplay
    {
        public string GameId { get; set; }

        public bool IsWhitePerspective { get; set; }
        public string Host { get; set; }
        public string HostColor { get; set; }

        public string Guest { get; set; }
        public string GuestColor { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<int[][]> BoardStates { get; set; }
    }

}
