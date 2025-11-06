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

namespace Checkers.GameLogic
{
    public class OnlineGameStrategy : IGameStrategy
    {
        private readonly GameManagerViewModel gameManager;
        private readonly GameService gameService = GameService.GetInstance();
        private readonly GameRealtimeService realtimeService = GameRealtimeService.GetInstance();
        private IDisposable? _gameSubscription;
        private readonly string gameId;
        private BoardViewModel boardVM;
        private bool isSubscribed = false;
        private bool isLocalPlayerWhite;

        private string? lastSentMoveId = "";

        public OnlineGameStrategy(GameManagerViewModel gameManager, string gameId, bool isLocalPlayerWhite)
        {
            this.gameManager = gameManager;
            this.gameId = gameId;
            this.isLocalPlayerWhite = isLocalPlayerWhite;
        }

        public void SetBoardViewModel(BoardViewModel boardVM)
        {
            this.boardVM = boardVM;
        }

        public async Task InitializeAsync(BoardViewModel boardVM)
        {
            this.boardVM = boardVM;

            if (!isSubscribed)
            {
                try
                {
                    await SubscribeToGameUpdates();
                    isSubscribed = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error subscribing to game updates: {ex.Message}");
                }
            }
        }
        public void UnsubFromGame()
        {
            _gameSubscription?.Dispose();
            _gameSubscription = null;
        }

        private async Task SubscribeToGameUpdates()
        {
            try
            {
                _gameSubscription = realtimeService.SubscribeToGame(gameId, async (gameModel) =>
                {
                    try
                    {
                        if (gameModel == null) return;

                        GameMove? lastMove = gameModel.Moves?.LastOrDefault();
                        lastMove = MoveHelper.ConvertMoveByPerspective(lastMove, isLocalPlayerWhite);
                        if (lastMove == null) return;

                        // אל תטפל במהלך אם זה המהלך שלך
                        if (lastMove.Id == lastSentMoveId) return;

                        // אל תטפל במהלך אם הוא מהצד שלך
                        if (lastMove.WasWhite == isLocalPlayerWhite) return;


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
                                        await Task.Delay(700); // דיליי קטן בין כל אכילה
                                    }
                                }
                                else
                                {
                                    // מהלך רגיל (בלי אכילה)
                                    toSquare.Piece = movingPiece;
                                    toSquare.UpdateProperty(nameof(toSquare.Piece));
                                    toSquare.UpdateProperty(nameof(toSquare.PieceImage));
                                }

                                // בדיקת קידום ל־King
                                if (movingPiece is Man)
                                {
                                    if ((movingPiece.Color == PieceColor.White && currentSquare.Row == 0) ||
                                        (movingPiece.Color == PieceColor.Black && currentSquare.Row == Board.Size - 1))
                                    {
                                        currentSquare.Piece = new King(movingPiece.Color);
                                        currentSquare.UpdateProperty(nameof(currentSquare.Piece));
                                        currentSquare.UpdateProperty(nameof(currentSquare.PieceImage));
                                    }
                                }

                                // עדכון מצב הלוח לפי ה-state מהפיירבייס
                                BoardHelper.ConvertStateToBoard(gameModel.BoardState, boardVM.Board, isLocalPlayerWhite);
                                gameManager.IsWhiteTurn = gameModel.IsWhiteTurn;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error animating opponent move: {ex.Message}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in game update handler: {ex.Message}");
                    }
                });


                Debug.WriteLine("Subscribed to Firebase!!!!");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subscribing to Firebase: {ex.Message}");
            }
        }

        public async Task HandleSquareSelectedAsync(BoardViewModel boardVM, SquareViewModel squareVM)
        {
            try
            {
                if (!CanLocalPlayerMove()) return;

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

                if (!gameManager.CanMove(piece)) return;

                var moves = piece.GetPossibleMoves(boardVM.Board,
                    boardVM.Board.Squares[boardVM.SelectedSquare.Row, boardVM.SelectedSquare.Column]);

                var move = moves.FirstOrDefault(m => m.To.Row == targetSquare.Row && m.To.Column == targetSquare.Column);
                if (move == null) return;

                foreach (var sq in boardVM.Squares)
                    sq.HasMoveMarker = false;

                boardVM.SelectedSquare = null;

                var currentVM = boardVM.Squares.First(s => s.Row == move.From.Row && s.Column == move.From.Column);

                foreach (var step in move.SquaresPath)
                {
                    var nextVM = boardVM.Squares.First(s => s.Row == step.Row && s.Column == step.Column);

                    var capture = move.Captures.FirstOrDefault(c => c.Landing.Row == step.Row && c.Landing.Column == step.Column);
                    if (capture.Captured != null)
                    {
                        var capturedVM = boardVM.Squares.First(s => s.Row == capture.Captured.Row && s.Column == capture.Captured.Column);
                        capturedVM.Piece = null;
                    }

                    currentVM.Piece = null;
                    nextVM.Piece = piece;
                    nextVM.RaisePieceImageChanged();
                    currentVM = nextVM;

                    await Task.Delay(700);
                }

                if (piece is Man)
                {
                    if ((piece.Color == PieceColor.White && currentVM.Row == 0) ||
                        (piece.Color == PieceColor.Black && currentVM.Row == Board.Size - 1))
                    {
                        currentVM.Piece = new King(piece.Color);
                    }
                }

                await PlayerMovedAsync(move);
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
                // החלפת תור
                gameManager.SwitchTurn();

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

                // הוספת המהלך החדש לרשימת המהלכים
                existingModel.Moves ??= new List<GameMove>();
                existingModel.Moves.Add(gameMove);

                // עדכון מצב הלוח והתור
                existingModel.BoardState = boardState;
                existingModel.IsWhiteTurn = gameManager.IsWhiteTurn;

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

    }
}
