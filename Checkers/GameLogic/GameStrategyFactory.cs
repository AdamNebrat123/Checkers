using Checkers.ViewModels;
using LiteDB.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public class GameStrategyFactory : IGameStrategyFactory
    {
        public IGameStrategy Create(GameMode mode, GameManagerViewModel manager, object? parameters = null)
        {
            return mode switch
            {
                GameMode.AI => CreateAiStrategy(manager, parameters),
                GameMode.Online => CreateOnlineStrategy(manager, parameters),
                _ => throw new NotSupportedException($"Unsupported game mode: {mode}")
            };
        }

        private static IGameStrategy CreateAiStrategy(GameManagerViewModel manager, object? parameters)
        {
            if (parameters is not AiSettings ai)
                throw new ArgumentException("Expected AiSettings for AI mode");

            return new AiGameStrategy(manager, ai.Depth, ai.IsWhite);
        }

        private static IGameStrategy CreateOnlineStrategy(GameManagerViewModel manager, object? parameters)
        {
            if (parameters is not OnlineSettings online)
                throw new ArgumentException("Expected OnlineSettings for Online mode");

            return new OnlineGameStrategy(manager, online.GameId, online.IsLocalPlayerWhite);
        }
    }
}
