using System;
using System.Collections.Generic;
using System.Linq;
using Checkers.Services;
using Microsoft.Maui.Controls;

namespace Checkers.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly IMusicService _musicService;

        private readonly Dictionary<string, string> _musicOptions = new Dictionary<string, string>
        {
            { "No music", "OFF" },
            { "Music1", "trump" },
            { "Music2", "bibi" },
        };

        public SettingsPage(IMusicService musicService)
        {
            InitializeComponent();
            _musicService = musicService;

            // ����� �-Picker �� ����� ����������
            MusicPicker.ItemsSource = _musicOptions.Keys.ToList();

            // ����� ������ ������� ��� ������� �������
            if (_musicService.CurrentMusic != null)
            {
                var selected = _musicOptions.FirstOrDefault(x => x.Value == _musicService.CurrentMusic).Key;
                if (selected != null)
                    MusicPicker.SelectedItem = selected;
            }
            else
            {
                MusicPicker.SelectedItem = "No music";
            }
        }

        private void OnMusicChanged(object sender, EventArgs e)
        {
            if (MusicPicker.SelectedItem is string displayName && !string.IsNullOrEmpty(displayName))
            {

                // ���� �� �������
                if (_musicOptions.TryGetValue(displayName, out var fileName))
                {

                    _musicService.Play(fileName);
                }
            }
        }
        private void OnPauseClicked(object sender, EventArgs e)
        {
            _musicService.Pause();
        }

        private void OnPlayClicked(object sender, EventArgs e)
        {
            if (_musicService.CurrentMusic != null)
                _musicService.Play(_musicService.CurrentMusic);
        }
    }
}
