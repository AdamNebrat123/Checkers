using Checkers.Models;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Checkers.Data
{
    public class GameRealtimeService : GameService
    {
        private IDisposable? _subscription;

        /// <summary>
        /// מנוי לשינויים במשחק ספציפי
        /// </summary>
        public void SubscribeToGame(string gameId, Action<GameModel> onGameChanged)
        {
            // **** the action signature can be changed if i will need to change it ****
            _subscription = firebaseClient
                .Child("games")
                .Child(gameId)
                .AsObservable<Dictionary<string, object>>()
                .Subscribe(d =>
                {
                    if (d.EventType == FirebaseEventType.InsertOrUpdate)
                    {
                        // טען מחדש את המשחק מהנתונים
                        var game = GetGameFromRawData(d.Object, gameId);
                        onGameChanged?.Invoke(game);
                    }
                    // אפשר לטפל גם במקרה של Delete אם רוצים
                });
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
            int[,] boardState = new int[8, 8];
            if (data.TryGetValue("BoardState", out var stateObj))
            {
                try
                {
                    if (stateObj is JsonElement jeState && jeState.ValueKind != JsonValueKind.Null)
                        boardState = JsonSerializer.Deserialize<int[,]>(jeState.GetRawText())!;
                    else if (stateObj != null)
                        boardState = JsonSerializer.Deserialize<int[,]>(stateObj.ToString()!)!;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            List<GameMove> moves = new();
            if (data.TryGetValue("Moves", out var movesObj))
            {
                try
                {
                    if (movesObj is JsonElement jeMoves && jeMoves.ValueKind != JsonValueKind.Null)
                        moves = JsonSerializer.Deserialize<List<GameMove>>(jeMoves.GetRawText())!;
                    else if (movesObj != null)
                        moves = JsonSerializer.Deserialize<List<GameMove>>(movesObj.ToString()!)!;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

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

    }
}
