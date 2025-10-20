using Checkers.Data;
using Checkers.Models;
using System;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public class OnlineGameStrategy : IGameStrategy
    {
        private readonly GameService _gameService;
        private readonly string _gameId;

        // אירוע שמודיע ל־GameViewModel לבצע מהלך
        public event Func<GameMove, Task>? OnMoveReceivedFromServer;

        public OnlineGameStrategy(GameService gameService, string gameId)
        {
            _gameService = gameService;
            _gameId = gameId;
        }

        public async Task InitializeAsync()
        {
            // מאזינים לעדכונים מהשרת
            _gameService.ListenForLastMove(_gameId, async move =>
            {
                if (move == null) return;

                // מודיעים ל־GameViewModel לבצע את המהלך
                if (OnMoveReceivedFromServer != null)
                    await OnMoveReceivedFromServer.Invoke(move);
            });
        }

        public async Task OnPlayerMoveAsync(GameMove move)
        {
            // שולחים את המהלך לשרת
            await _gameService.SendMoveAsync(_gameId, move);
        }

        public void Dispose()
        {
            // בעתיד: ננקה מאזינים מהשרת
        }
    }
}
