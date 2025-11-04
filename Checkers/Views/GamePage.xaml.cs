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

            if (Wrapper == null)
                throw new InvalidOperationException("ParametersWrapper is required");

            // קובע את המוד
            GameMode mode = Enum.Parse<GameMode>(Wrapper.Mode, ignoreCase: true);

            // יוצר Settings לפי JSON
            object settings = mode switch
            {
                GameMode.AI => JsonSerializer.Deserialize<AiSettings>(Wrapper.Parameters.GetRawText())!,
                GameMode.Online => JsonSerializer.Deserialize<OnlineSettings>(Wrapper.Parameters.GetRawText())!,
                _ => throw new NotSupportedException($"Unsupported mode: {mode}")
            };

            // קובע אם הלבן
            bool isWhite = settings switch
            {
                AiSettings ai => ai.IsWhite,
                OnlineSettings online => online.IsLocalPlayerWhite,
                _ => true
            };

            // מאתחל לוח
            _gameViewModel.InitializeBoard(isWhite, false);

            // יוצר אסטרטגיה דרך Factory
            var strategy = _strategyFactory.Create(mode, _gameViewModel.GameManager, settings);
            _gameViewModel.SetStrategy(strategy);

            // אירוע לחיצה על ריבועים
            _gameViewModel.BoardVM.SquareClicked += async square =>
                await _gameViewModel.HandleSquareSelectedAsync(square);

            BindingContext = _gameViewModel;
            await _gameViewModel.InitializeAsync();
        }
    }
}
