using Checkers.Models;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Checkers.Data
{
    public class GameRealtimeService : GameService
    {
        private const string GamesCollection = "games";
        private IDisposable? _subscription;

        /// <summary>
        /// מנוי לשינויים במשחק ספציפי
        /// </summary>
        /// 

        private static readonly GameRealtimeService _instance = new();
        private GameRealtimeService() { }

        public static GameRealtimeService GetInstance() { return _instance; }

        public IDisposable SubscribeToGame(string gameId, Action<GameModel> onGameChanged)
        {

            var subscription = firebaseClient
            .Child(GamesCollection)
            .AsObservable<GameModel>()
            .Subscribe(d =>
            {
                Debug.WriteLine($"EventType: {d.EventType}");

                var game = d.Object;
                if (game == null)
                    return;

                // ודא שזה באמת המשחק שאנחנו מאזינים לו
                if (game.GameId != gameId)
                    return;

                if (d.EventType == FirebaseEventType.InsertOrUpdate)
                {
                    try
                    {
                        Debug.WriteLine("invoking function!!!!!!!");
                        onGameChanged?.Invoke(game);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error invoking game change handler: {ex.Message}");
                    }
                }
            });


            return subscription;
        }

        /// <summary>
        /// ביטול המנוי
        /// </summary>
        public void Unsubscribe()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        
        public IDisposable SubscribeToAvailableGames(Action<List<GameModel>> onGamesUpdated)
        {
            var subscription = firebaseClient
                .Child("games")
                .AsObservable<GameModel>()
                .Subscribe(d =>
                {
                    if (d.EventType == FirebaseEventType.InsertOrUpdate || d.EventType == FirebaseEventType.Delete)
                    {
                        Task.Run(async () =>
                        {
                            var allGames = await GetAllGamesAsync();
                            var available = allGames.Where(g => string.IsNullOrEmpty(g.Guest)).ToList();
                            MainThread.BeginInvokeOnMainThread(() => onGamesUpdated(available));
                        });
                    }
                });

            return subscription;
        }

    }
}
