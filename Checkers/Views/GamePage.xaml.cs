using Checkers.GameLogic;
using Checkers.ViewModel;
using Checkers.ViewModels;
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

            if (_initialized) return;

            if (Wrapper == null)
                throw new InvalidOperationException("ParametersWrapper is required");

            // ÷åáò àú äîåã
            GameMode mode = Enum.Parse<GameMode>(Wrapper.Mode, ignoreCase: true);

            // éåöø Settings ìôé JSON
            object settings = mode switch
            {
                GameMode.AI => JsonSerializer.Deserialize<AiSettings>(Wrapper.Parameters.GetRawText())!,
                GameMode.Online => JsonSerializer.Deserialize<OnlineSettings>(Wrapper.Parameters.GetRawText())!,
                _ => throw new NotSupportedException($"Unsupported mode: {mode}")
            };

            // ÷åáò àí äìáï
            bool isWhite = settings switch
            {
                AiSettings ai => ai.IsWhite,
                OnlineSettings online => online.IsLocalPlayerWhite,
                _ => true
            };

            // îàúçì ìåç
            _gameViewModel.InitializeBoard(isWhite, false);

            // éåöø àñèøèâéä ãøê Factory
            var strategy = _strategyFactory.Create(mode, _gameViewModel.GameManager, settings);
            _gameViewModel.SetStrategy(strategy);

            // àéøåò ìçéöä òì øéáåòéí
            _gameViewModel.BoardVM.SquareClicked += async square =>
                await _gameViewModel.HandleSquareSelectedAsync(square);

            BindingContext = _gameViewModel;
            await _gameViewModel.InitializeAsync();

            _initialized = true; // ככה אני מוודא שבפעם הבאה הוא לא יעשה את הכל מחדש ויפתח עוד מאזינים וכו'

        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _gameViewModel.UnubFromGame();
        }
    }
}
