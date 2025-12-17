using Checkers.Data;
using Checkers.GameLogic;
using Checkers.Model;
using Checkers.Models;
using Checkers.Services;
using Checkers.ViewModel;
using Checkers.ViewModels;
using System.ComponentModel;
using System.Text.Json;

namespace Checkers.Views
{
    [QueryProperty(nameof(Wrapper), "wrapper")]

    public partial class GamePage : ContentPage
    {
        private readonly GameViewModel _gameViewModel;
        private readonly IGameStrategyFactory _strategyFactory;
        private bool _initialized;


        public ModeParametersWrapper? Wrapper { get; set; }

        public GamePage(GameViewModel gameViewModel, IGameStrategyFactory strategyFactory)
        {
            InitializeComponent();
            _gameViewModel = gameViewModel;
            _strategyFactory = strategyFactory;
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            WinnerContentView.SetBinding(ContentView.IsVisibleProperty,
                    new Binding(nameof(GameViewModel.IsWinnerVisible),
                                mode: BindingMode.OneWay,
                                stringFormat: "{0:mm\\:ss}"));

            if (_initialized) return;

            if (Wrapper == null)
                throw new InvalidOperationException("ParametersWrapper is required");

            GameMode mode = Enum.Parse<GameMode>(Wrapper.Mode, ignoreCase: true);

            object settings = mode switch
            {
                GameMode.AI => JsonSerializer.Deserialize<AiSettings>(Wrapper.Parameters.GetRawText())!,
                GameMode.Online => JsonSerializer.Deserialize<OnlineSettings>(Wrapper.Parameters.GetRawText())!,
                GameMode.Spectator => JsonSerializer.Deserialize<SpectatorSettings>(Wrapper.Parameters.GetRawText())!,
                GameMode.Replay => JsonSerializer.Deserialize<ReplaySettings>(Wrapper.Parameters.GetRawText())!,

                _ => throw new NotSupportedException($"Unsupported mode: {mode}")
            };

            bool isWhite = settings switch
            {
                AiSettings ai => ai.IsWhite,
                OnlineSettings online => online.IsLocalPlayerWhite,
                SpectatorSettings spectator => spectator.IsWhitePerspective,
                ReplaySettings replay => replay.IsWhitePerspective,
                _ => true
            };

            // אתחול לוח + שמירת פרספקטיבה
            _gameViewModel.InitializeBoard(isWhite);

            // צור strategy
            var strategy = _strategyFactory.Create(mode, _gameViewModel.GameManager, settings);
            _gameViewModel.SetStrategy(strategy);

            // הרשמת אירוע קליקים
            _gameViewModel.BoardVM.SquareClicked += async square =>
                await _gameViewModel.HandleSquareSelectedAsync(square);

            BindingContext = _gameViewModel;

            // אם זה מצב Online — נרשום גם למצב התור דרך GameViewModel
            string gameId = string.Empty;
            if (settings is OnlineSettings onlineSettings)
            {
                gameId = onlineSettings.GameId;

                // TIMERS
                _gameViewModel.InitTimers(onlineSettings.TimerTimeInMinutes);
                SetupTimersUI();
            }
            if (settings is SpectatorSettings spectatorSettings)
            {
                gameId = spectatorSettings.GameId;
            }
            if (settings is ReplaySettings replaySettings)
            {
                gameId = replaySettings.GameId;
            }


            SetGameNames();




            await _gameViewModel.InitializeAsync();

            // הגדרה ראשונית של ה-highlights (sync UI)
            if (!(settings is AiSettings) && !(settings is ReplaySettings))
            {
                UpdateHighlightsInitial(); 
            }

            // הרשמה לשינויים ב־ViewModel כדי לאנימט
            if (_gameViewModel is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged += GameViewModel_PropertyChanged;
            }

            _initialized = true;

            var musicService = IPlatformApplication.Current.Services.GetRequiredService<IMusicService>();
            musicService.Play(SfxEnum.game_start.ToString(), false);

        }

        private void UpdateHighlightsInitial()
        {

            // בהתאם ל־IsLocalTurn, נשנה את ה־Opacity הראשוני
            var localPlayerIsWhite = _gameViewModel.IsLocalPlayerIsWhite;
            // local means bottom highlight active
            // אנימציית מצב מתחלף קצרה
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await BottomHighlight.FadeTo(localPlayerIsWhite ? 1.0 : 0.12, 200);
                await TopHighlight.FadeTo(localPlayerIsWhite ? 0.12 : 1.0, 200);
            });
        }


        private void GameViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameViewModel.IsLocalTurn))
            {
                // ערך חדש
                var isLocal = _gameViewModel.IsLocalTurn;
                // אנימציה קצרה של Fade
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // כשזה שלך נדליק את ה־BottomHighlight, וכשזה לא שלך נדליק את ה־TopHighlight
                    // נעשה fade לשליטה חלקה
                    await Task.WhenAll(
                        BottomHighlight.FadeTo(isLocal ? 1.0 : 0.12, 300),
                        TopHighlight.FadeTo(isLocal ? 0.12 : 1.0, 300)
                    );
                });
            }
        }
        private async void OnWinnerCloseClicked(object sender, EventArgs e)
        {
            string username = Preferences.Get("UserName", "");
            bool isLoggedIn = !string.IsNullOrEmpty(username);
            if (isLoggedIn) 
                await Shell.Current.GoToAsync("///HomePage");
            else
                await Shell.Current.GoToAsync("///MainPage");

        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _gameViewModel.UnubFromGame();

            if (_gameViewModel is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged -= GameViewModel_PropertyChanged;
            }
        }

        private void SetupTimersUI()
        {
            bool iAmWhite = _gameViewModel.IsLocalPlayerIsWhite;


            PlayerTimerLabel.IsVisible = true;
            OpponentTimerLabel.IsVisible = true;

            // 2. Binding לפי צבע השחקן
            if (iAmWhite)
            {
                // Player - white
                // Opponent - black
                PlayerTimerLabel.SetBinding(Label.TextProperty,
                    new Binding(nameof(GameViewModel.WhiteTimeLeft),
                                mode: BindingMode.OneWay,
                                stringFormat: "{0:mm\\:ss}"));

                OpponentTimerLabel.SetBinding(Label.TextProperty,
                    new Binding(nameof(GameViewModel.BlackTimeLeft),
                                mode: BindingMode.OneWay,
                                stringFormat: "{0:mm\\:ss}"));
            }
            else
            {
                // Player - black
                // Opponent - white

                PlayerTimerLabel.SetBinding(Label.TextProperty,
                    new Binding(nameof(GameViewModel.BlackTimeLeft),
                                mode: BindingMode.OneWay,
                                stringFormat: "{0:mm\\:ss}"));

                OpponentTimerLabel.SetBinding(Label.TextProperty,
                    new Binding(nameof(GameViewModel.WhiteTimeLeft),
                                mode: BindingMode.OneWay,
                                stringFormat: "{0:mm\\:ss}"));
            }
        }
        private async void SetGameNames()
        {
            IGameNames strategy = _gameViewModel.Strategy as IGameNames;

            var (playerName, opponentName) = await strategy.GetGameNames();

            _gameViewModel.PlayerName = playerName;
            _gameViewModel.OpponentName = opponentName;
        }
    }
}
