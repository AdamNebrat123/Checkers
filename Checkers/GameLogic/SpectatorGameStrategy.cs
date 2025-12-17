using Checkers.Data;
using Checkers.Model;
using Checkers.Models;
using Checkers.MoveHistory;
using Checkers.Services;
using Checkers.Utils;
using Checkers.ViewModel;
using Checkers.ViewModels;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public class SpectatorGameStrategy : IGameStrategy, IBoardSnapshotHistory, IGameNames
    {
        private readonly IMusicService _musicService = IPlatformApplication.Current.Services.GetRequiredService<IMusicService>();
        private readonly GameEventDispatcher gameEventDispatcher;
        private readonly GameManagerViewModel gameManager;
        private readonly GameRealtimeService realtimeService = GameRealtimeService.GetInstance();
        private readonly string gameId;
        private BoardViewModel boardVM;

        private bool _subscribed = false;
        private bool isWhitePerspective;

        public BoardSnapshotHistory BoardSnapshotHistory { get; private set; }


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

        public async Task InitializeAsync(BoardViewModel VM)
        {
            this.boardVM = VM;

            BoardSnapshotHistory = new BoardSnapshotHistory(boardVM, this.isWhitePerspective);

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
            // update the squares in the UI
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                boardVM.Board = BoardHelper.ConvertStateToBoard(
                    existingModel.BoardState,
                    isWhitePerspective);


                for (int row = 0; row < Board.Size; row++)
                {
                    for (int col = 0; col < Board.Size; col++)
                    {
                        int index = row * Board.Size + col;
                        var squareVM = boardVM.Squares[index];

                        squareVM.Piece = boardVM.Board.Squares[row, col].Piece;
                        // ה-Piece כבר עודכן דרך BoardHelper
                        squareVM.UpdateProperty(nameof(SquareViewModel.Piece));
                        squareVM.UpdateProperty(nameof(SquareViewModel.PieceImage));
                    }
                }
                await AddInitialBoardSnapshot();

                // becuase the current state is one step behind
                await HandleOnUpdatedBoard(existingModel);


            });

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
                // Reset Board To Most Updated Snapshot
                BoardSnapshotHistory.ResetBoardToMostUpdatedSnapshot();


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

                        gameManager.SwitchTurn();


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
                        UpdateBoard();
                        int[][] boardState = BoardHelper.ConvertBoardToState(boardVM.Board, isWhitePerspective);
                        BoardSnapshotHistory.AddState(boardState);
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

        private async Task AddInitialBoardSnapshot()
        {
            var boardState = BoardHelper.ConvertBoardToState(boardVM.Board, isWhitePerspective);
            BoardSnapshotHistory.AddState(boardState);

        }
        private void UpdateBoard()
        {
            for (int row = 0; row < Board.Size; row++)
            {
                for (int col = 0; col < Board.Size; col++)
                {
                    int index = row * Board.Size + col;
                    var squareVM = boardVM.Squares[index];

                    boardVM.Board.Squares[row, col].Piece = squareVM.Piece;
                }
            }
        }

        public async Task<(string playerName, string opponentName)> GetGameNames()
        {
            string playerName = string.Empty;
            string opponentName = string.Empty;

            if (!string.IsNullOrEmpty(gameId))
            {
                GameModel? existingModel = null;
                existingModel = await GameRealtimeService.GetInstance().GetGameAsync(gameId);
                if (existingModel != null)
                {
                    if (isWhitePerspective)
                    {
                        if (existingModel.GuestColor == PieceColor.White.ToString())
                        {
                            playerName = existingModel.Guest;
                            opponentName = existingModel.Host;
                        }
                        else
                        {
                            playerName = existingModel.Host;
                            opponentName = existingModel.Guest;
                        }
                    }
                    else
                    {
                        if (existingModel.GuestColor == PieceColor.Black.ToString())
                        {
                            playerName = existingModel.Guest;
                            opponentName = existingModel.Host;
                        }
                        else
                        {
                            playerName = existingModel.Host;
                            opponentName = existingModel.Guest;
                        }
                    }
                }
            }
            return (playerName, opponentName);
        }
    }
}
