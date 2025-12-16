using Checkers.Models;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Data
{
    public class ReplayService : FirebaseService
    {
        private const string ReplaysCollection = "Replays";

        private static readonly ReplayService _instance = new ReplayService();
        protected ReplayService() { }

        public static ReplayService GetInstance() => _instance;

        /// <summary>
        /// שומר ריפליי של משחק עבור משתמש
        /// </summary>
        public async Task<bool> SaveReplayAsync(string userId, GameReplay replay)
        {
            try
            {
                await firebaseClient
                    .Child(ReplaysCollection)
                    .Child(userId)
                    .Child(replay.GameId)
                    .PutAsync(replay);

                Debug.WriteLine($"Replay {replay.GameId} saved for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving replay: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// טוען ריפליי של משחק לפי משתמש ו־GameId
        /// </summary>
        public async Task<GameReplay?> GetReplayAsync(string userId, string gameId)
        {
            try
            {
                var replay = await firebaseClient
                    .Child(ReplaysCollection)
                    .Child(userId)
                    .Child(gameId)
                    .OnceSingleAsync<GameReplay>();

                return replay;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading replay: {ex.Message}");
                return null;
            }
        }



        /// <summary>
        /// מוחק ריפליי
        /// </summary>
        public async Task DeleteReplayAsync(string userId, string gameId)
        {
            try
            {
                await firebaseClient
                    .Child(ReplaysCollection)
                    .Child(userId)
                    .Child(gameId)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting replay: {ex.Message}");
            }
        }
        public async Task<List<GameReplay>> GetAllReplaysForUserAsync(string userId)
        {
            try
            {
                var result = await firebaseClient
                    .Child(ReplaysCollection)
                    .Child(userId)
                    .OnceAsync<GameReplay>();

                return result
                    .Select(x => x.Object)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading replays for user {userId}: {ex.Message}");
                return new List<GameReplay>();
            }
        }

        public async Task DeleteAllReplaysForUserAsync(string userId)
        {
            try
            {
                await firebaseClient
                    .Child(ReplaysCollection)
                    .Child(userId)
                    .DeleteAsync();

                Debug.WriteLine($"All replays for user {userId} deleted");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting replays for user {userId}: {ex.Message}");
            }
        }
    }
}
