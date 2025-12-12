using Checkers.Data;
using Checkers.GameLogic;
using Checkers.ViewModels;
using System;
using System.Threading.Tasks;
using System.ComponentModel;
using Checkers.Model;
using Checkers.Models;
using Checkers.Views;
using Checkers.Services;
using System.Reflection;

namespace Checkers.ViewModel
{
    public class GameViewModel : ViewModelBase
    {
        public BoardViewModel BoardVM { get; private set; }
        public GameManagerViewModel GameManager { get; }
        private IGameStrategy _strategy;

        private readonly IMusicService _musicService = IPlatformApplication.Current.Services.GetRequiredService<IMusicService>();


        public GameViewModel()
        {
            
        }

        private string _playerName = "";
        public string PlayerName
        {
            get => _playerName;
            set
            {
                if (_playerName != value)
                {
                    _playerName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _opponentName = "";
        public string OpponentName
        {
            get => _opponentName;
            set
            {
                if (_opponentName != value)
                {
                    _opponentName = value;
                    OnPropertyChanged();
                }
            }
        }

        // --- turn indicator ---
        private bool _isLocalTurn;
        public bool IsLocalTurn
        {
            get => _isLocalTurn;
            set
            {
                if (_isLocalTurn != value)
                {
                    _isLocalTurn = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsOpponentTurn));
                }
            }
        }

        public bool IsOpponentTurn => !_isLocalTurn;

        // פרספקטיבה מקומית (מגדירים כשאתה יודע אם המשתמש הוא לבן)
        private bool _localPlayerIsWhite;
        public bool LocalPlayerIsWhite
        {
            get => _localPlayerIsWhite;
            private set
            {
                if (_localPlayerIsWhite != value)
                {
                    _localPlayerIsWhite = value;
                    OnPropertyChanged();
                }
            }
        }


        private int _playerCapturedCount;
        public int PlayerCapturedCount
        {
            get => _playerCapturedCount;
            set { _playerCapturedCount = value; OnPropertyChanged(); }
        }

        private int _opponentCapturedCount;
        public int OpponentCapturedCount
        {
            get => _opponentCapturedCount;
            set { _opponentCapturedCount = value; OnPropertyChanged(); }
        }


        private CancellationTokenSource? _whiteTimerCts;
        private CancellationTokenSource? _blackTimerCts;

        private TimeSpan _whiteTimeLeft;
        public TimeSpan WhiteTimeLeft
        {
            get => _whiteTimeLeft;
            set { _whiteTimeLeft = value; OnPropertyChanged(); }
        }

        private TimeSpan _blackTimeLeft;
        public TimeSpan BlackTimeLeft
        {
            get => _blackTimeLeft;
            set { _blackTimeLeft = value; OnPropertyChanged(); }
        }
        private bool _isWinnerVisible = false;
        public bool IsWinnerVisible
        {
            get => _isWinnerVisible;
            set
            {
                _isWinnerVisible = value;
                OnPropertyChanged();
            }
        }

        private string _winnerName;
        public string WinnerName
        {
            get => _winnerName;
            set
            {
                _winnerName = "Winner is: " + value;
                OnPropertyChanged();
            }
        }

        public string PlayerPieceImage => LocalPlayerIsWhite ? "black_piece.png" : "white_piece.png";
        public string OpponentPieceImage => LocalPlayerIsWhite ? "white_piece.png" : "black_piece.png";

        // --- event dispatcher subscription ---
        private readonly GameEventDispatcher _dispatcher = GameEventDispatcher.GetInstance();
        private Func<Checkers.Models.GameModel, Task>? _dispatcherCallback;
        private string? _subscribedGameId;

        // Inject only the GameManager via DI
        public GameViewModel(GameManagerViewModel gameManager)
        {
            GameManager = gameManager;

            gameManager.TurnSwitched += UpdateCapturedCounts;
        }

        public void UnubFromGame()
        {
            if (_strategy is OnlineGameStrategy online)
                online.UnsubscribeFromGameUpdates();

            // גם נבטל את המנוי ל־GameEventDispatcher אם קיים
            UnsubscribeFromTurnUpdates();
        }

        public void SetBoardViewModel(BoardViewModel boardVM)
        {
            BoardVM = boardVM;
            _strategy?.SetBoardViewModel(boardVM);
        }

        // Create the board internally with the given perspective
        public void InitializeBoard(bool whitePerspective, bool buttonsInverted)
        {
            var boardVM = new BoardViewModel(whitePerspective, buttonsInverted);
            SetBoardViewModel(boardVM);
            // שמירה של הפרספקטיבה המקומית
            LocalPlayerIsWhite = whitePerspective;
        }

        public void SetStrategy(IGameStrategy strategy)
        {
            _strategy = strategy;
            if (BoardVM != null)
                _strategy.SetBoardViewModel(BoardVM);
        }

        public async Task InitializeAsync()
        {
            if (_strategy != null && BoardVM != null)
            {
                await _strategy.InitializeAsync(BoardVM);
            }
        }

        public async Task HandleSquareSelectedAsync(SquareViewModel square)
        {
            if (_strategy == null) return;
            await _strategy.HandleSquareSelectedAsync(BoardVM, square);
        }

        // --------- Turn subscription helpers ---------

        /// <summary>
        /// הירשם לעדכונים של משחק ספציפי כדי לעדכן את מצב התור.
        /// call with the gameId and the local player's color (LocalPlayerIsWhite already set by InitializeBoard)
        /// </summary>
        public void SubscribeToTurnUpdates(string gameId)
        {
            if (string.IsNullOrEmpty(gameId)) return;

            _subscribedGameId = gameId;
            _dispatcherCallback = async (gameModel) =>
            {
                try
                {
                    if (gameModel == null) return;

                    // gameModel.IsWhiteTurn = האם תור הלבן
                    bool isWhiteTurn = gameModel.IsWhiteTurn;

                    // אם הפרספקטיבה המקומית היא לבן => תורי אם isWhiteTurn == true
                    bool isLocalTurnNow = (isWhiteTurn == LocalPlayerIsWhite);

                    // עדכון ב־UI thread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        IsLocalTurn = isLocalTurnNow;
                    });

                    UpdateTimer(isWhiteTurn);

                    await HandleWinnerCheck(gameModel);

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in turn update callback: {ex.Message}");
                }
            };

            _dispatcher.Subscribe(gameId, _dispatcherCallback);
        }

        public void UnsubscribeFromTurnUpdates()
        {
            if (!string.IsNullOrEmpty(_subscribedGameId) && _dispatcherCallback != null)
            {
                _dispatcher.Unsubscribe(_subscribedGameId, _dispatcherCallback);
                _dispatcherCallback = null;
                _subscribedGameId = null;
            }
        }
        private async Task HandleWinnerCheck(GameModel game)
        {
            string winner = CheckWinner(game);

            if (string.IsNullOrEmpty(winner))
            return;

            ShowWinner(winner);

        }
        private string CheckWinner(GameModel game)
        {
            int totalWhites = 0;
            int totalBlacks = 0;

            foreach (var square in BoardVM.Squares)
            {
                if (square.Piece == null) continue;
                if (square.Piece.Color == PieceColor.White)
                    totalWhites++;
                else
                    totalBlacks++;
            }

            if (totalWhites == 0)
            {
                return game.GuestColor == PieceColor.White.ToString() ? game.Guest : game.Host;
            }
            if (totalBlacks == 0)
            {
                return game.GuestColor == PieceColor.Black.ToString() ? game.Guest : game.Host;
            }

            return string.Empty;
        }
        public async void ShowWinner(string name)
        {
            WinnerName = name;

            await Task.Delay(1000);
            _musicService.Play(SfxEnum.game_end.ToString(), false);

            await Task.Delay(1000);
            IsWinnerVisible = true;
        }

        

        public void UpdateCapturedCounts()
        {
            if (BoardVM?.Squares == null) return;

            int totalWhites = 0;
            int totalBlacks = 0;

            foreach (var square in BoardVM.Squares)
            {
                if (square.Piece == null) continue;
                if (square.Piece.Color == PieceColor.White)
                    totalWhites++;
                else
                    totalBlacks++;
            }

            // נניח שהתחלנו עם 12 חיילים מכל צבע
            int whiteEaten = 12 - totalWhites;
            int blackEaten = 12 - totalBlacks;

            // אם אני לבן, למטה זה כמה שחורים נאכלו
            if (LocalPlayerIsWhite)
            {
                PlayerCapturedCount = blackEaten;
                OpponentCapturedCount = whiteEaten;
            }
            else
            {
                PlayerCapturedCount = whiteEaten;
                OpponentCapturedCount = blackEaten;
            }
        }
        private async Task RunTimerAsync(bool isWhite, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1000);

                if (isWhite)
                {
                    WhiteTimeLeft -= TimeSpan.FromSeconds(1);
                    if (WhiteTimeLeft <= TimeSpan.Zero)
                    {
                        WhiteTimeLeft = TimeSpan.Zero;
                        HandleTimerExpired(isWhite);
                        return;
                    }
                }
                else
                {
                    BlackTimeLeft -= TimeSpan.FromSeconds(1);
                    if (BlackTimeLeft <= TimeSpan.Zero)
                    {
                        BlackTimeLeft = TimeSpan.Zero;
                        HandleTimerExpired(isWhite);
                        return;
                    }
                }
            }
        }
        public void InitTimers(int minutes)
        {
            WhiteTimeLeft = TimeSpan.FromMinutes(minutes);
            BlackTimeLeft = TimeSpan.FromMinutes(minutes);
        }

        private void StartWhiteTimer()
        {
            _blackTimerCts?.Cancel();
            _whiteTimerCts = new CancellationTokenSource();
            _ = RunTimerAsync(true, _whiteTimerCts.Token);
        }

        private void StartBlackTimer()
        {
            _whiteTimerCts?.Cancel();
            _blackTimerCts = new CancellationTokenSource();
            _ = RunTimerAsync(false, _blackTimerCts.Token);
        }

        public void UpdateTimer(bool nowWhiteTurn)
        {
            if (nowWhiteTurn)
                StartWhiteTimer();
            else
                StartBlackTimer();
        }
        private void HandleTimerExpired(bool isWhite)
        {
            if (isWhite)
                WhiteTimeLeft = TimeSpan.Zero;
            else
                BlackTimeLeft = TimeSpan.Zero;

            string winner = "";
            if (isWhite)
            {
                if (!LocalPlayerIsWhite)
                    winner = PlayerName;
                else
                    winner = OpponentName;

            }
            else
            {
                if (LocalPlayerIsWhite)
                    winner = PlayerName;
                else
                    winner = OpponentName;
            }


            ShowWinner(winner);
        }
    }
}
