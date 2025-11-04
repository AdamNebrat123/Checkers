using Checkers.GameLogic;
using Checkers.ViewModels;
using System.Threading.Tasks;

namespace Checkers.ViewModel
{
    public class GameViewModel : ViewModelBase
    {
        public BoardViewModel BoardVM { get; private set; }
        public GameManagerViewModel GameManager { get; }
        private IGameStrategy _strategy;

        // Inject only the GameManager via DI
        public GameViewModel(GameManagerViewModel gameManager)
        {
            GameManager = gameManager;
        }

        public void SetBoardViewModel(BoardViewModel boardVM)
        {
            BoardVM = boardVM;
            _strategy?.SetBoardViewModel(boardVM);
        }

        // Create the board internally with the given perspective
        public void InitializeBoard(bool whitePerspective, bool buttonsInverted)
        {
            var boardVM = new BoardViewModel(whitePerspective, buttonsInverted);
            SetBoardViewModel(boardVM);
        }

        public void SetStrategy(IGameStrategy strategy)
        {
            _strategy = strategy;
            if (BoardVM != null)
                _strategy.SetBoardViewModel(BoardVM);
        }

        public async Task InitializeAsync()
        {
            if (_strategy != null && BoardVM != null)
            {
                await _strategy.InitializeAsync(BoardVM);
            }
        }

        public async Task HandleSquareSelectedAsync(SquareViewModel square)
        {
            if (_strategy == null) return;
            await _strategy.HandleSquareSelectedAsync(BoardVM, square);
        }
    }
}
