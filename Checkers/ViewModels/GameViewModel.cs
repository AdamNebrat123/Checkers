using Checkers.GameLogic;
using Checkers.ViewModels;
using System.Threading.Tasks;

namespace Checkers.ViewModel
{
    public class GameViewModel : ViewModelBase
    {
        public BoardViewModel BoardVM { get; }
        public GameManagerViewModel GameManager { get; }
        private readonly IGameStrategy _strategy;

        public GameViewModel(IGameStrategy strategy, GameManagerViewModel gameManager, BoardViewModel boardVM)
        {
            _strategy = strategy;
            GameManager = gameManager;
            BoardVM = boardVM;
        }

        public async Task InitializeAsync()
        {
            await _strategy.InitializeAsync(BoardVM);
        }

        public async Task HandleSquareSelectedAsync(SquareViewModel square)
        {
            await _strategy.HandleSquareSelectedAsync(BoardVM, square);
        }
    }
}
