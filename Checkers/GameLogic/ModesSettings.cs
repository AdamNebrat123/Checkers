using Checkers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public class AiSettings
    {
        public int Depth { get; set; }
        public bool IsWhite { get; set; }
    }

    public class OnlineSettings
    {
        public string GameId { get; set; }
        public bool IsLocalPlayerWhite { get; set; }
        public int TimerTimeInMinutes {  get; set; }
    }
    public class SpectatorSettings
    {
        public string GameId { get; set; }
        public bool IsWhitePerspective { get; set; }
    }
    public class ReplaySettings
    {
        public GameReplay GameReplay { get; set; }
        public string GameId { get; set; }
        public bool IsWhitePerspective { get; set; }
    }
}
