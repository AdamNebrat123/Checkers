using Checkers.Data;
using Checkers.Model;
using Checkers.Models;
using Checkers.Services;
using Checkers.Utils;
using Checkers.ViewModel;
using Checkers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public class SpectatorGameStrategy : IGameStrategy
    {
        private readonly IMusicService _musicService = IPlatformApplication.Current.Services.GetRequiredService<IMusicService>();
        private readonly GameEventDispatcher gameEventDispatcher;
        private readonly GameManagerViewModel gameManager;
        private readonly GameRealtimeService realtimeService = GameRealtimeService.GetInstance();
        private readonly string gameId;
        private BoardViewModel boardVM;

        private bool _subscribed = false;
        private bool isWhitePerspective;

        public SpectatorGameStrategy(GameManagerViewModel gameManager, string gameId, bool isLocalPlayerWhite)
        {
            this.gameManager = gameManager;
            this.gameId = gameId;
            this.isWhitePerspective = isLocalPlayerWhite;

            gameEventDispatcher = GameEventDispatcher.GetInstance();
        }
        public Task HandleSquareSelectedAsync(BoardViewModel boardVM, SquareViewModel squareVM)
        {
            _musicService.Play(SfxEnum.illegal.ToString(), false);
            return Task.CompletedTask;
        }
        public void SetBoardViewModel(BoardViewModel boardVM)
        {
            this.boardVM = boardVM;
        }

        public async Task InitializeAsync(BoardViewModel boardVM)
        {
            this.boardVM = boardVM;


            // שליפה של המשחק הקיים
            GameModel? existingModel = null;
            try
            {
                existingModel = await realtimeService.GetGameAsync(gameId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching game from Firebase: {ex.Message}");
                return;
            }

            if (existingModel == null)
                return;

            // צריך לאתחל את מצב הלוח כמו שצריך, שיהיה זהה למצב בו השחקנים נמצאים כרגע
            BoardHelper.ConvertStateToBoard(existingModel.BoardState, boardVM.Board, isWhitePerspective);

            if (!_subscribed)
            {
                try
                {
                    SubscribeToGameUpdates();
                    _subscribed = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error subscribing to game updates: {ex.Message}");
                }
            }

           

        }

        private void SubscribeToGameUpdates()
        {
            try
            {
                gameEventDispatcher.Subscribe(gameId, HandleOnUpdatedBoard);
                _subscribed = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subscribing to Firebase: {ex.Message}");
            }
        }

        public void UnsubscribeFromGameUpdates()
        {
            if (_subscribed)
            {
                gameEventDispatcher.Unsubscribe(gameId, HandleOnUpdatedBoard);
                _subscribed = false;
            }
        }

        private async Task HandleOnUpdatedBoard(GameModel gameModel)
        {
            try
            {
                if (gameModel == null) return;
                GameMove? originalMove = gameModel.Move;
                GameMove? lastMove = gameModel.Move;
                lastMove = MoveHelper.ConvertMoveByPerspective(lastMove, isWhitePerspective);
                if (lastMove == null) return;

                // שליפת הריבועים המעורבים
                var fromSquare = boardVM.Squares[lastMove.FromRow * Board.Size + lastMove.FromCol];
                var toSquare = boardVM.Squares[lastMove.ToRow * Board.Size + lastMove.ToCol];
                var movingPiece = fromSquare.Piece;
                if (movingPiece == null) return;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        // איפוס סימנים
                        foreach (var sq in boardVM.Squares)
                            sq.HasMoveMarker = false;

                        // ריקון ריבוע המקור
                        fromSquare.Piece = null;
                        fromSquare.UpdateProperty(nameof(fromSquare.Piece));
                        fromSquare.UpdateProperty(nameof(fromSquare.PieceImage));

                        // נניע את החייל שלב-שלב
                        var currentSquare = fromSquare;



                        // אם יש קפיצות (Captures)
                        if (lastMove.Captures != null && lastMove.Captures.Any())
                        {
                            foreach (var cap in lastMove.Captures)
                            {
                                // מחיקת החייל שנאכל
                                var eatenSquare = boardVM.Squares[cap.CapturedRow * Board.Size + cap.CapturedCol];
                                eatenSquare.Piece = null;
                                eatenSquare.UpdateProperty(nameof(eatenSquare.Piece));
                                eatenSquare.UpdateProperty(nameof(eatenSquare.PieceImage));

                                // תזוזת החייל למיקום הנחיתה
                                var landingSquare = boardVM.Squares[cap.LandingRow * Board.Size + cap.LandingCol];
                                landingSquare.Piece = movingPiece;
                                landingSquare.UpdateProperty(nameof(landingSquare.Piece));
                                landingSquare.UpdateProperty(nameof(landingSquare.PieceImage));

                                // ריקון הריבוע הקודם
                                currentSquare.Piece = null;
                                currentSquare.UpdateProperty(nameof(currentSquare.Piece));
                                currentSquare.UpdateProperty(nameof(currentSquare.PieceImage));

                                currentSquare = landingSquare;

                                _musicService.Play(SfxEnum.capture.ToString(), false);
                                await Task.Delay(700); // דיליי קטן בין כל אכילה
                            }
                        }
                        else
                        {
                            // מהלך רגיל (בלי אכילה)
                            toSquare.Piece = movingPiece;
                            toSquare.UpdateProperty(nameof(toSquare.Piece));
                            toSquare.UpdateProperty(nameof(toSquare.PieceImage));

                            _musicService.Play(SfxEnum.move_self.ToString(), false);
                        }

                        // בדיקת קידום ל־King
                        if (movingPiece is Man)
                        {
                            if ((originalMove.ToRow == Board.Size - 1 && movingPiece.Color == PieceColor.Black)
                                || (originalMove.ToRow == 0 && movingPiece.Color == PieceColor.White))
                            {
                                toSquare.Piece = new King(movingPiece.Color);
                                _musicService.Play(SfxEnum.promote.ToString(), false);

                                currentSquare.UpdateProperty(nameof(currentSquare.Piece));
                                currentSquare.UpdateProperty(nameof(currentSquare.PieceImage));
                            }
                        }

                        // עדכון מצב הלוח לפי ה-state מהפיירבייס
                        //BoardHelper.ConvertStateToBoard(gameModel.BoardState, boardVM.Board, IsWhitePerspective);
                        gameManager.SwitchTurn();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error animating opponent Move: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in game update handler: {ex.Message}");
            }
        }


    }
}
