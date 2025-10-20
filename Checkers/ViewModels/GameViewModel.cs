using Checkers.Data;
using Checkers.Models;
using Checkers.ViewModel;
using System.Threading.Tasks;

namespace Checkers.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private readonly GameService _gameService;
        private readonly BoardViewModel _board;
        private readonly string _gameId;
        private readonly string _playerName;
        private readonly bool _isCreator;

        public GameViewModel(GameService gameService, BoardViewModel board, string gameId, string playerName, bool isCreator)
        {
            _gameService = gameService;
            _board = board;
            _gameId = gameId;
            _playerName = playerName;
            _isCreator = isCreator;

            // מאזין למהלכים שמגיעים מהצד השני
            _gameService.ListenForLastMove(_gameId, OnMoveReceived);
        }

        /// <summary>
        /// שולח מהלך ל־Firebase אחרי שהשחקן ביצע אותו.
        /// </summary>
        public async Task SendMoveAsync(GameMove move)
        {
            await _gameService.SendMoveAsync(_gameId, move);
        }

        /// <summary>
        /// כשהצד השני ביצע מהלך חדש.
        /// </summary>
        private void OnMoveReceived(GameMove move)
        {
            if (move == null)
                return;

            // אם זה מהלך של היריב (לא שלך)
            bool isOpponentMove = move.isWhiteTurn != _board.gameManager.IsWhiteTurn;

            if (isOpponentMove)
            {
                _board.ApplyMove(move);
            }
        }

        /// <summary>
        /// מאפס את הלוח להתחלת משחק חדש.
        /// </summary>
        public void ResetGame()
        {
            _board.ResetBoard();
        }
    }
}
