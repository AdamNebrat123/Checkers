using Checkers.Data;
using Checkers.GameLogic;
using Checkers.Models;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Checkers.Views
{
    //[QueryProperty(nameof(GameName), "gameName")]
    [QueryProperty(nameof(GameId), "gameId")]
    public partial class WaitingRoom : ContentPage
    {
        //public string GameName { get; set; }
        public string GameId { get; set; }

        private readonly GameEventDispatcher gameEventDispatcher;
        private readonly GameService _gameService = GameService.GetInstance();
        private readonly GameRealtimeService _realtimeService = GameRealtimeService.GetInstance();

        private bool _joined = false;
        private bool _subscribed = false;

        public WaitingRoom()
        {
            InitializeComponent();

            gameEventDispatcher = GameEventDispatcher.GetInstance();

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //GameCodeLabel.Text = GameName ?? "GAME1234";
            GameIdLabel.Text = $"Game ID: {GameId}";

            if (!_subscribed && !string.IsNullOrEmpty(GameId))
                ListenForGuestAsync();

        }

        private void ListenForGuestAsync()
        {
            try
            {
                gameEventDispatcher.Subscribe(GameId, OnGuestJoined);
                _subscribed = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error listening for guest: {ex.Message}");
            }
        }

        private async Task OnGuestJoined(GameModel gameModel)
        {
            if (gameModel.Guest != "")
            {
                _joined = true;
                MainThread.BeginInvokeOnMainThread(async () =>
                {

                    var onlineSettings = new OnlineSettings { GameId = gameModel.GameId, IsLocalPlayerWhite = gameModel.HostColor == "White" };
                    var wrapper = new ModeParametersWrapper
                    {
                        Mode = GameMode.Online.ToString(),
                        Parameters = JsonSerializer.SerializeToElement(onlineSettings)
                    };

                    await Shell.Current.GoToAsync(nameof(GamePage), new Dictionary<string, object>
                    {
                        { "wrapper", wrapper }
                    });
                });
            }
        }

        private async void OnCancelWaitingClicked(object sender, EventArgs e)
        {
            try
            {
                StopListeningForGuest();

                if (!_joined && !string.IsNullOrEmpty(GameId))
                    await _gameService.DeleteGameAsync(GameId);

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling waiting: {ex.Message}");
            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopListeningForGuest();
        }

        private void StopListeningForGuest()
        {
            if (_subscribed)
            {
                gameEventDispatcher.Unsubscribe(GameId, OnGuestJoined);
                _subscribed = false;
            }
        }
    }
}
