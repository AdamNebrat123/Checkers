using Checkers.Models;
using Firebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Checkers.Data
{
    public class GameEventDispatcher : FirebaseService
    {
        private static GameEventDispatcher? _instance;
        private static readonly object _lock = new object();

        private const string GamesCollection = "games";

        // מפתח = gameId, ערך = רשימת callbacks אסינכרוניות
        private readonly Dictionary<string, List<Func<GameModel, Task>>> _subscribersByGame = new();
        private IDisposable? _firebaseSubscription;

        private GameEventDispatcher()
        {
            // האזנה ל-Firebase פעם אחת בלבד
            _firebaseSubscription = firebaseClient
                .Child(GamesCollection)
                .AsObservable<GameModel>()
                .Subscribe(async d =>
                {
                    var game = d.Object;
                    if (game == null) return;

                    if (d.EventType == FirebaseEventType.InsertOrUpdate)
                    {
                        if (string.IsNullOrEmpty(game.GameId))
                        {
                            Debug.WriteLine("Received game with null or empty GameId — ignoring");
                            return;
                        }


                        if (_subscribersByGame.TryGetValue(game.GameId, out var subscribers))
                        {
                            foreach (var subscriber in subscribers.ToList()) // ToList כדי למנוע בעיות בזמן הסרה
                            {
                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        await subscriber(game);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine($"Error invoking subscriber for game {game.GameId}: {ex.Message}");
                                    }
                                });
                            }
                        }
                    }
                });
        }

        public static GameEventDispatcher GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new GameEventDispatcher();
                }
            }
            return _instance;
        }

        /// <summary>
        /// מוסיף callback אסינכרוני למשחק ספציפי
        /// </summary>
        public void Subscribe(string gameId, Func<GameModel, Task> callback)
        {
            if (!_subscribersByGame.ContainsKey(gameId))
                _subscribersByGame[gameId] = new List<Func<GameModel, Task>>();

            if (!_subscribersByGame[gameId].Contains(callback))
            {
                _subscribersByGame[gameId].Add(callback);
                Debug.WriteLine($"Subscribed to gameId - {gameId}");
            }
        }

        /// <summary>
        /// מסיר callback ממשחק ספציפי
        /// </summary>
        public void Unsubscribe(string gameId, Func<GameModel, Task> callback)
        {
            if (_subscribersByGame.TryGetValue(gameId, out var subscribers))
            {
                if (subscribers.Contains(callback))
                    subscribers.Remove(callback);

                if (subscribers.Count == 0)
                    _subscribersByGame.Remove(gameId);
            }
        }

        public void Dispose()
        {
            _firebaseSubscription?.Dispose();
            _firebaseSubscription = null;
            _subscribersByGame.Clear();
        }

    }
}
