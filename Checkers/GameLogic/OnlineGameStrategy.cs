using Checkers.Data;
using Checkers.Models;
using Checkers.ViewModel;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public class OnlineGameStrategy : IGameStrategy
    {
        private readonly GameService _gameService;
        private readonly BoardViewModel _board;
        private readonly string _gameId;

        public OnlineGameStrategy(GameService gameService, BoardViewModel board, string gameId)
        {
            _gameService = gameService;
            _board = board;
            _gameId = gameId;
        }

        public async Task InitializeAsync()
        {
            // נרשמים לעדכונים מ־Firebase
            _gameService.ListenForLastMove(_gameId, OnMoveReceived);
        }

        public async Task OnPlayerMoveAsync(GameMove move)
        {
            // שולחים את המהלך ל־Firebase
            await _gameService.SendMoveAsync(_gameId, move);
        }

        private void OnMoveReceived(GameMove move)
        {
            if (move == null)
                return;

            // אם זה תור היריב - נעדכן את הלוח
            bool isOpponentMove = move.isWhiteTurn != _board.gameManager.IsWhiteTurn;
            if (isOpponentMove)
                _board.ApplyMove(move);
        }

        public void Dispose()
        {
            // בעתיד: ננקה מאזינים מ־Firebase אם צריך
        }
    }
}
