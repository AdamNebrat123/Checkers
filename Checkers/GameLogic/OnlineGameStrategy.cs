using Checkers.Data;
using Checkers.Model;
using Checkers.Models;
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

        private async Task SubscribeToGameUpdates()
        {
            try
            {
                _gameSubscription = realtimeService.SubscribeToGame(gameId, async (gameModel) =>
                {
                    try
                    {
                        if (gameModel == null) return;

                        var lastMove = gameModel.Moves?.LastOrDefault();
                        if (lastMove == null) return;

                        if (lastMove.WasWhite == isLocalPlayerWhite)
                            return;

                        BoardHelper.ConvertStateToBoard(gameModel.BoardState, boardVM.Board);
                        gameManager.IsWhiteTurn = gameModel.IsWhiteTurn;

                        var fromSquare = boardVM.Squares[lastMove.FromRow * Board.Size + lastMove.FromCol];
                        var toSquare = boardVM.Squares[lastMove.ToRow * Board.Size + lastMove.ToCol];

                        SquareViewModel? eatenSquare = null;
                        if (lastMove.EatenRow.HasValue && lastMove.EatenCol.HasValue)
                        {
                            eatenSquare = boardVM.Squares[lastMove.EatenRow.Value * Board.Size + lastMove.EatenCol.Value];
                        }

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            try
                            {
                                if (eatenSquare != null)
                                    boardVM.RefreshSquares(fromSquare, toSquare, eatenSquare);
                                else
                                    boardVM.RefreshSquares(fromSquare, toSquare);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error refreshing squares: {ex.Message}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in game update handler: {ex.Message}");
                    }
                });
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
                gameManager.SwitchTurn();

                var gameMove = new GameMove
                {
                    FromRow = move.From.Row,
                    FromCol = move.From.Column,
                    ToRow = move.To.Row,
                    ToCol = move.To.Column,
                    WasWhite = isLocalPlayerWhite
                };

                var boardState = BoardHelper.ConvertBoardToState(boardVM.Board);

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

                if (existingModel == null) return;

                existingModel.Moves ??= new List<GameMove>();
                existingModel.Moves.Add(gameMove);

                existingModel.BoardState = boardState;
                existingModel.IsWhiteTurn = gameManager.IsWhiteTurn;

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
