using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public interface IGameStrategyFactory
    {
        IGameStrategy Create(GameMode mode, GameManagerViewModel manager, object? parameters = null);
    }
}
