using Checkers.Models;
using Checkers.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// 

        private static readonly GameService _instance = new GameService();

        protected GameService() { }

        public static GameService GetInstance() { return _instance; }

        public async Task<string?> CreateGameAsync(GameModel game)
        {
            try
            {
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
                    { "CreatedAt", game.CreatedAt.ToString("o") }
                };

                await SaveDocumentAsync(GamesCollection, game.GameId, data);
                Debug.WriteLine("******************************************************");
                Debug.WriteLine($"Game {game.GameId} created at path {GamesCollection}");
                Debug.WriteLine("******************************************************");

                return game.GameId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating game: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// מעדכן את מצב המשחק לאחר מהלך
        /// </summary>
        public async Task UpdateGameAsync(GameModel game)
        {
            try
            {
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
                    { "CreatedAt", game.CreatedAt.ToString("o") },
                };
                await DeleteGameAsync(game.GameId);
                await Task.Delay(3000);

                await CreateGameAsync(game);
                //await UpdateDocumentAsync(GamesCollection, game.GameId, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating game: {ex.Message}");
            }
        }

        /// <summary>
        /// טוען משחק לפי GameId
        /// </summary>
        public async Task<GameModel?> GetGameAsync(string gameId)
        {
            try
            {
                var data = await GetDocumentAsync(GamesCollection, gameId);
                if (data == null) return null;

                return new GameModel
                {
                    GameId = data.TryGetValue("GameId", out var id) ? id.ToString()! : gameId,
                    Host = data.TryGetValue("Host", out var host) ? host.ToString()! : "",
                    HostColor = data.TryGetValue("HostColor", out var hColor) ? hColor.ToString()! : "White",
                    Guest = data.TryGetValue("Guest", out var guest) ? guest.ToString()! : "",
                    GuestColor = data.TryGetValue("GuestColor", out var gColor) ? gColor.ToString()! : "Black",
                    IsWhiteTurn = data.TryGetValue("IsWhiteTurn", out var turn) ? bool.Parse(turn.ToString()!) : true,
                    BoardState = data.TryGetValue("BoardState", out var state)
    ? JsonSerializer.Deserialize<int[][]>(state.ToString()!)!
    : BoardHelper.InitializeEmptyBoard(),
                    Moves = data.TryGetValue("Moves", out var moves)
                        ? JsonSerializer.Deserialize<List<GameMove>>(moves.ToString()!)!
                        : new List<GameMove>(),
                    CreatedAt = data.TryGetValue("CreatedAt", out var created)
                        ? DateTime.Parse(created.ToString()!)
                        : DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading game: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// מוחק משחק
        /// </summary>
        public async Task DeleteGameAsync(string gameId)
        {
            try
            {
                await DeleteDocumentAsync(GamesCollection, gameId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting game: {ex.Message}");
            }
        }


        public async Task<List<GameModel>> GetAllGamesAsync()
        {
            var gamesSnapshot = await firebaseClient
                .Child("games")
                .OnceAsync<Dictionary<string, object>>();

            var games = new List<GameModel>();

            foreach (var gameSnap in gamesSnapshot)
            {
                try
                {
                    var data = gameSnap.Object;
                    var game = new GameModel
                    {
                        GameId = data.TryGetValue("GameId", out var id) ? id.ToString()! : gameSnap.Key,
                        Host = data.TryGetValue("Host", out var host) ? host.ToString()! : "",
                        HostColor = data.TryGetValue("HostColor", out var hColor) ? hColor.ToString()! : "White",
                        Guest = data.TryGetValue("Guest", out var guest) ? guest.ToString()! : "",
                        GuestColor = data.TryGetValue("GuestColor", out var gColor) ? gColor.ToString()! : "Black",
                        IsWhiteTurn = data.TryGetValue("IsWhiteTurn", out var turn) ? bool.Parse(turn.ToString()!) : true,
                        CreatedAt = data.TryGetValue("CreatedAt", out var created) ? DateTime.Parse(created.ToString()!) : DateTime.UtcNow
                    };

                    // ננסה לפרש את ה־BoardState אם קיים
                    if (data.TryGetValue("BoardState", out var stateObj))
                    {
                        try
                        {
                            string json = stateObj switch
                            {
                                JsonElement jeState when jeState.ValueKind != JsonValueKind.Null => jeState.GetRawText(),
                                not null => stateObj.ToString()!,
                                _ => "null"
                            };
                            if (json != "null")
                                game.BoardState = JsonSerializer.Deserialize<int[][]>(json)!;
                        }
                        catch { }
                    }

                    games.Add(game);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing game: {ex.Message}");
                }
            }

            return games;
        }
    }
}
