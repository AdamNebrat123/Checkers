using Checkers.Data;
using Checkers.Model;
using Checkers.Models;
using Checkers.Utils;
using Checkers.ViewModel;
using Checkers.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using Checkers.Services;
using Checkers.MoveHistory;
using System.ComponentModel;

namespace Checkers.GameLogic
{
    public class OnlineGameStrategy : IGameStrategy, IBoardSnapshotHistory, IGameNames
    {
        private readonly IMusicService _musicService = IPlatformApplication.Current.Services.GetRequiredService<IMusicService>();
        private readonly GameEventDispatcher gameEventDispatcher;
        private readonly GameManagerViewModel gameManager;
        private readonly GameService gameService = GameService.GetInstance();
        private readonly GameRealtimeService realtimeService = GameRealtimeService.GetInstance();
        private readonly string gameId;
        private BoardViewModel boardVM;
        private bool _subscribed = false;
        private bool isLocalPlayerWhite;

        private string? lastSentMoveId = "";

        public BoardSnapshotHistory BoardSnapshotHistory { get; private set; }

        public OnlineGameStrategy(GameManagerViewModel gameManager, string gameId, bool isLocalPlayerWhite)
        {
            this.gameManager = gameManager;
            this.gameId = gameId;
            this.isLocalPlayerWhite = isLocalPlayerWhite;

            gameEventDispatcher = GameEventDispatcher.GetInstance();



        }

        public void SetBoardViewModel(BoardViewModel boardVM)
        {
            this.boardVM = boardVM;
        }

        public async Task InitializeAsync(BoardViewModel boardVM)
        {
            this.boardVM = boardVM;

            BoardSnapshotHistory = new BoardSnapshotHistory(boardVM, this.isLocalPlayerWhite);
            AddInitialBoardSnapshot();

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
                lastMove = MoveHelper.ConvertMoveByPerspective(lastMove, isLocalPlayerWhite);
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
                        int[][] boardState = BoardHelper.ConvertBoardToState(boardVM.Board, isLocalPlayerWhite);
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

        public async Task HandleSquareSelectedAsync(BoardViewModel boardVM, SquareViewModel squareVM)
        {
            try
            {
                if (!CanLocalPlayerMove())
                {
                    _musicService.Play(SfxEnum.illegal.ToString(), false);
                    return;
                }

                if (squareVM.HasMoveMarker)
                    await MovePieceAsync(squareVM);
                else
                    boardVM.SelectedSquare = squareVM;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling square selection: {ex.Message}");
            }
        }

        private bool CanLocalPlayerMove()
        {
            return gameManager.IsWhiteTurn == isLocalPlayerWhite;
        }

        private async Task MovePieceAsync(SquareViewModel targetSquare)
        {
            try
            {
                if (boardVM == null) return;
                if (boardVM.SelectedSquare?.Piece == null) return;

                var piece = boardVM.SelectedSquare.Piece;

                if (!gameManager.CanMove(piece))
                {
                    _musicService.Play(SfxEnum.illegal.ToString(), false);
                    return;
                }

                var moves = piece.GetPossibleMoves(boardVM.Board,
                    boardVM.Board.Squares[boardVM.SelectedSquare.Row, boardVM.SelectedSquare.Column]);

                var move = moves.FirstOrDefault(m => m.To.Row == targetSquare.Row && m.To.Column == targetSquare.Column);
                if (move == null) return;

                boardVM.SelectedSquare = null;

                await PlayerMovedAsync(move);

                // אל תזיז את ה־UI כאן
                foreach (var sq in boardVM.Squares)
                    sq.HasMoveMarker = false;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving piece: {ex.Message}");
            }
        }


        private async Task PlayerMovedAsync(Move move)
        {
            try
            {

                // יצירת אובייקט המהלך לשמירה
                var gameMove = new GameMove
                {
                    Id = Guid.NewGuid().ToString(),
                    FromRow = move.From.Row,
                    FromCol = move.From.Column,
                    ToRow = move.To.Row,
                    ToCol = move.To.Column,
                    WasWhite = isLocalPlayerWhite,
                    Timestamp = DateTime.UtcNow
                };

                // העתקת כל שלבי האכילה (אם יש)
                foreach (var cap in move.Captures)
                {
                    gameMove.Captures.Add(new CaptureStep
                    {
                        CapturedRow = cap.Captured.Row,
                        CapturedCol = cap.Captured.Column,
                        LandingRow = cap.Landing.Row,
                        LandingCol = cap.Landing.Column
                    });
                }

                // שמירה של הפרספקטיבה (כדי ששני הצדדים יראו נכון)
                gameMove = MoveHelper.ConvertMoveByPerspective(gameMove, isLocalPlayerWhite);

                // שמירה של מזהה המהלך שנשלח, כדי לא לטפל בו פעמיים
                lastSentMoveId = gameMove.Id;

                // המרת מצב הלוח הנוכחי לשמירה במסד הנתונים
                var boardState = BoardHelper.ConvertBoardToState(boardVM.Board, isLocalPlayerWhite);

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

                // הוספת המהלך החדש
                existingModel.Move = gameMove;

                // עדכון מצב הלוח והתור
                existingModel.BoardState = boardState;
                existingModel.IsWhiteTurn = !existingModel.IsWhiteTurn;

                // שמירת הנתונים המעודכנים
                try
                {
                    await gameService.UpdateGameAsync(existingModel);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating game in Firebase: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PlayerMovedAsync: {ex.Message}");
            }
        }

        private async Task AddInitialBoardSnapshot()
        {

            var boardState = BoardHelper.ConvertBoardToState(boardVM.Board, isLocalPlayerWhite);
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
                    if (isLocalPlayerWhite)
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
