using Checkers.Models;
using Checkers.Utils;
using Firebase.Database.Query;
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
                await firebaseClient
                    .Child(GamesCollection)
                    .Child(game.GameId)
                    .PutAsync(game);

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
                //await DeleteGameAsync(game.GameId);
                //await Task.Delay(3000);

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
                var game = await firebaseClient
                    .Child(GamesCollection)
                    .Child(gameId)
                    .OnceSingleAsync<GameModel>();

                return game;
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
            try
            {
                var gamesDict = await firebaseClient
                    .Child("games")
                    .OnceAsync<GameModel>();

                var games = gamesDict.Select(g => g.Object).ToList();
                return games;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading all games: {ex.Message}");
                return new List<GameModel>();
            }
        }
    }
}
