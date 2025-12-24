using System;
using System.Collections.Generic;
using System.Linq;
using Checkers.Services;
using Microsoft.Maui.Controls;

namespace Checkers.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly IBackgroundMusicService _backgroundMusicService = IPlatformApplication.Current.Services.GetRequiredService<IBackgroundMusicService>();
        private readonly ISoundEffectService _SFX = IPlatformApplication.Current.Services.GetRequiredService<ISoundEffectService>();

        private readonly Dictionary<string, string> _musicOptions = new Dictionary<string, string>
        {
            { "No music", "OFF" },
            { "Music1", "trump" },
            { "Music2", "bibi" },
        };

        public SettingsPage()
        {
            InitializeComponent();

            // מילוי ה-Picker עם השמות הידידותיים
            MusicPicker.ItemsSource = _musicOptions.Keys.ToList();

            // קביעת הבחירה הנוכחית לפי המוזיקה הנוכחית
            if (_backgroundMusicService.CurrentMusic != null)
            {
                var selected = _musicOptions.FirstOrDefault(x => x.Value == _backgroundMusicService.CurrentMusic).Key;
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
            //if (MusicPicker.SelectedItem is string displayName && !string.IsNullOrEmpty(displayName))
            //{

            //    // לנגן את המוזיקה
            //    if (_musicOptions.TryGetValue(displayName, out var fileName))
            //    {
            //        _backgroundMusicService.Stop();
            //        _backgroundMusicService.Play(fileName);
            //    }
            //}
        }
        private void OnStopClicked(object sender, EventArgs e)
        {
            _backgroundMusicService.Stop();
        }

        private void OnPlayClicked(object sender, EventArgs e)
        {
            if (MusicPicker.SelectedItem is string displayName && !string.IsNullOrEmpty(displayName))
            {
                if (_musicOptions.TryGetValue(displayName, out var fileName))
                {
                    _backgroundMusicService.Stop();
                    _backgroundMusicService.Play(fileName);
                }
            }
        }
    }
}
