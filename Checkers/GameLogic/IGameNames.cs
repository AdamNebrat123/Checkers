using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public interface IGameNames
    {
        Task<(string playerName, string opponentName)> GetGameNames();
    }
}
