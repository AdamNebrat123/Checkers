using Checkers.Data;
using Checkers.GameLogic;
using Checkers.Models;
using Checkers.ViewModel;
using System.Threading.Tasks;

namespace Checkers.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private readonly IGameStrategy _strategy;
        private readonly BoardViewModel _board;

        public GameViewModel(BoardViewModel board, IGameStrategy strategy)
        {
            _board = board;
            _strategy = strategy;
        }

        public async Task InitializeAsync()
        {
            await _strategy.InitializeAsync();
        }

        public async Task PlayerMovedAsync(GameMove move)
        {
            await _strategy.OnPlayerMoveAsync(move);
        }

        public void Dispose()
        {
            _strategy.Dispose();
        }
    }
}
