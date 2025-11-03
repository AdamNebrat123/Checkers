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
    [QueryProperty(nameof(GameName), "gameName")]
    [QueryProperty(nameof(GameId), "gameId")]
    public partial class WaitingRoom : ContentPage
    {
        public string GameName { get; set; }
        public string GameId { get; set; }

        private readonly GameService _gameService = GameService.GetInstance();
        private readonly GameRealtimeService _realtimeService = GameRealtimeService.GetInstance();
        private IDisposable? _gameSubscription;

        private bool _joined = false;

        public WaitingRoom()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            GameCodeLabel.Text = GameName ?? "GAME1234";
            GameIdLabel.Text = $"Game ID: {GameId}";
            await ListenForGuestAsync();
        }

        private async Task ListenForGuestAsync()
        {
            try
            {
                // נשתמש ב־Realtime listener כדי לזהות אם השדה Guest התמלא
                _gameSubscription = _realtimeService.SubscribeToGame(GameId, game =>
                {
                    if (game.Guest != "")
                    {
                        _joined = true;
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert("Guest Joined!", $"{game.Guest} joined your game.", "OK");

                            var onlineSettings = new OnlineSettings { GameId = game.GameId, IsLocalPlayerWhite = game.HostColor == "White" };
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
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error listening for guest: {ex.Message}");
            }
        }

        private async void OnCancelWaitingClicked(object sender, EventArgs e)
        {
            try
            {
                _gameSubscription?.Dispose();
                _gameSubscription = null;

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
            _gameSubscription?.Dispose();
            _gameSubscription = null;
        }
    }
}
