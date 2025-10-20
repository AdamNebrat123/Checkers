using Checkers.Models;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Checkers.Data
{
    public class GameService : FirebaseService
    {
        private const string GamesCollection = "games";

        /// <summary>
        /// יוצר משחק חדש עם מזהה ייחודי, שם היוצר ושדות התחלתיים.
        /// </summary>
        public async Task<string> CreateGameAsync(string creatorName, string gameName)
        {
            string gameId = Guid.NewGuid().ToString()[..8]; // מזהה קצר

            var gameData = new Dictionary<string, object>
            {
                { "creator", creatorName },
                { "guest", "" },
                { "gameName", gameName },
                { "isWhiteTurn", true },
                { "lastMove", null } // אין מהלך עדיין
            };

            await SaveDocumentAsync(GamesCollection, gameId, gameData);
            return gameId;
        }

        /// <summary>
        /// שחקן שני מצטרף למשחק קיים.
        /// </summary>
        public async Task JoinGameAsync(string gameId, string guestName)
        {
            var updateData = new Dictionary<string, object>
            {
                { "guest", guestName }
            };
            await UpdateDocumentAsync(GamesCollection, gameId, updateData);
        }

        /// <summary>
        /// שולח מהלך חדש ל-lastMove ומעדכן את isWhiteTurn
        /// </summary>
        public async Task SendMoveAsync(string gameId, GameMove move)
        {
            var updateData = new Dictionary<string, object>
            {
                { "lastMove", move },
                { "isWhiteTurn", move.isWhiteTurn }
            };

            await UpdateDocumentAsync(GamesCollection, gameId, updateData);
        }

        /// <summary>
        /// מקבל את המהלך האחרון (lastMove) של המשחק.
        /// </summary>
        public async Task<GameMove> GetLastMoveAsync(string gameId)
        {
            var doc = await GetDocumentAsync(GamesCollection, gameId);
            if (doc != null && doc.TryGetValue("lastMove", out var moveObj))
            {
                return JsonConvert.DeserializeObject<GameMove>(moveObj.ToString());
            }
            return null;
        }

        /// <summary>
        /// מאזין לשינויים ב-lastMove של המשחק.
        /// </summary>
        public void ListenForLastMove(string gameId, Action<GameMove> onMoveReceived)
        {
            firebaseClient.Child(GamesCollection).Child(gameId).Child("lastMove")
                .AsObservable<GameMove>()
                .Subscribe(d =>
                {
                    if (d.Object != null)
                        onMoveReceived(d.Object);
                });
        }
    }
}
