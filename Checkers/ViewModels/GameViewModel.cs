using Checkers.Data;
using Checkers.GameLogic;
using Checkers.Models;
using Checkers.ViewModel;
using System;
using System.Threading.Tasks;

namespace Checkers.ViewModels
{
    public class GameViewModel : ViewModelBase, IDisposable
    {
        private readonly BoardViewModel _board;
        private readonly IGameStrategy _strategy;

        public GameViewModel(BoardViewModel board, IGameStrategy strategy)
        {
            _board = board;
            _strategy = strategy;

            _board.OnPlayerMoveCompleted += async move =>
            {
                await _strategy.OnPlayerMoveAsync(move);
            };

            if (_strategy is OnlineGameStrategy online)
            {
                online.OnMoveReceivedFromServer += async move =>
                {
                    await _board.ApplyMove(move);
                };
            }
        }

        public async Task InitializeAsync()
        {
            await _strategy.InitializeAsync();
        }

        private GameMove CreateMoveFromSelection(SquareViewModel from, SquareViewModel to)
        {
            return new GameMove
            {
                fromRow = from.Row,
                fromCol = from.Column,
                toRow = to.Row,
                toCol = to.Column,
                isWhiteTurn = _board.gameManager.IsWhiteTurn
            };
        }

        public void Dispose()
        {
            _strategy.Dispose();
        }
    }
}
