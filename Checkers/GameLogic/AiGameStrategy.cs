using Checkers.Models;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public class AiGameStrategy : IGameStrategy
    {
        public Task InitializeAsync()
        {
            // בעתיד נטען את ה-AI וכו'
            return Task.CompletedTask;
        }

        public Task OnPlayerMoveAsync(GameMove move)
        {
            // כאן נגרום ל-AI לחשוב ולעשות מהלך
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // כלום כרגע
        }
    }
}
