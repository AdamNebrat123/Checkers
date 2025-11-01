using Checkers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Checkers.Data
{
    public class GameService : FirebaseService
    {
        private const string GamesCollection = "games";

        /// <summary>
        /// יוצר משחק חדש ושומר בפיירבייס
        /// </summary>
        public async Task<string> CreateGameAsync(GameModel game)
        {
            // serialize BoardState ו־Moves
            var data = new Dictionary<string, object>
            {
                { "GameId", game.GameId },
                { "Host", game.Host },
                { "HostColor", game.HostColor },
                { "Guest", game.Guest ?? "" },
                { "GuestColor", game.GuestColor ?? "" },
                { "IsWhiteTurn", game.IsWhiteTurn },
                { "BoardState", JsonSerializer.Serialize(game.BoardState) },
                { "Moves", JsonSerializer.Serialize(game.Moves) },
                { "CreatedAt", game.CreatedAt.ToString("o") } // ISO 8601
            };

            await SaveDocumentAsync(GamesCollection, game.GameId, data);
            return game.GameId;
        }

        /// <summary>
        /// מעדכן את מצב המשחק לאחר מהלך
        /// </summary>
        public async Task UpdateGameAsync(GameModel game)
        {
            var data = new Dictionary<string, object>
            {
                { "IsWhiteTurn", game.IsWhiteTurn },
                { "BoardState", JsonSerializer.Serialize(game.BoardState) },
                { "Moves", JsonSerializer.Serialize(game.Moves) },
                { "Guest", game.Guest ?? "" } // אם נצטרך להוסיף שחקן באמצע
            };

            await UpdateDocumentAsync(GamesCollection, game.GameId, data);
        }

        /// <summary>
        /// טוען משחק לפי GameId
        /// </summary>
        public async Task<GameModel?> GetGameAsync(string gameId)
        {
            var data = await GetDocumentAsync(GamesCollection, gameId);
            if (data == null) return null;

            var game = new GameModel
            {
                GameId = data.TryGetValue("GameId", out var id) ? id.ToString()! : gameId,
                Host = data.TryGetValue("Host", out var host) ? host.ToString()! : "",
                HostColor = data.TryGetValue("HostColor", out var hColor) ? hColor.ToString()! : "White",
                Guest = data.TryGetValue("Guest", out var guest) ? guest.ToString()! : "",
                GuestColor = data.TryGetValue("GuestColor", out var gColor) ? gColor.ToString()! : "Black",
                IsWhiteTurn = data.TryGetValue("IsWhiteTurn", out var turn) ? bool.Parse(turn.ToString()!) : true,
                BoardState = data.TryGetValue("BoardState", out var state)
                    ? JsonSerializer.Deserialize<int[,]>(state.ToString()!)!
                    : new int[8, 8],
                Moves = data.TryGetValue("Moves", out var moves)
                    ? JsonSerializer.Deserialize<List<GameMove>>(moves.ToString()!)!
                    : new List<GameMove>(),
                CreatedAt = data.TryGetValue("CreatedAt", out var created)
                    ? DateTime.Parse(created.ToString()!)
                    : DateTime.UtcNow
            };

            return game;
        }

        /// <summary>
        /// מוחק משחק
        /// </summary>
        public async Task DeleteGameAsync(string gameId)
        {
            await DeleteDocumentAsync(GamesCollection, gameId);
        }
    }
}
