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

        /// <summary>
        /// ממיר את הנתונים הגולמיים מה־Firebase ל-GameModel
        /// </summary>
        private GameModel GetGameFromRawData(Dictionary<string, object> data, string gameId)
        {
            int[][] boardState = new int[8][];
            for (int i = 0; i < 8; i++) boardState[i] = new int[8];

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
                        boardState = JsonSerializer.Deserialize<int[][]>(json)!;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            List<GameMove> moves = new();
            if (data.TryGetValue("Moves", out var movesObj))
            {
                try
                {
                    string json = movesObj switch
                    {
                        JsonElement jeMoves when jeMoves.ValueKind != JsonValueKind.Null => jeMoves.GetRawText(),
                        not null => movesObj.ToString()!,
                        _ => "null"
                    };

                    if (json != "null")
                        moves = JsonSerializer.Deserialize<List<GameMove>>(json)!;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            Debug.WriteLine("print from GetGameFromRawData");

            return new GameModel
            {
                GameId = data.TryGetValue("GameId", out var id) ? id.ToString()! : gameId,
                Host = data.TryGetValue("Host", out var host) ? host.ToString()! : "",
                HostColor = data.TryGetValue("HostColor", out var hColor) ? hColor.ToString()! : "White",
                Guest = data.TryGetValue("Guest", out var guest) ? guest.ToString()! : "",
                GuestColor = data.TryGetValue("GuestColor", out var gColor) ? gColor.ToString()! : "Black",
                IsWhiteTurn = data.TryGetValue("IsWhiteTurn", out var turn) ? bool.Parse(turn.ToString()!) : true,
                BoardState = boardState,
                Moves = moves,
                CreatedAt = data.TryGetValue("CreatedAt", out var created) ? DateTime.Parse(created.ToString()!) : DateTime.UtcNow
            };
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
