using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Services
{
    #if ANDROID
        using Android.Media;
        using Android.App;
    #elif IOS
    using AVFoundation;
    using Foundation;
    #endif

    public class SoundEffectsMusicService : ISoundEffectService
    {
        #if ANDROID
                private MediaPlayer _player;
        #elif IOS
            private AVAudioPlayer _player;
        #endif

        public string CurrentMusic { get; private set; }

        public bool IsPlaying
        {
            #if ANDROID
                        get => _player?.IsPlaying ?? false;
            #elif IOS
                    get => _player?.Playing ?? false;
            #else
                    get => false;
            #endif
        }

        public void Play(string musicName, bool isLooping = true)
        {

            if (string.IsNullOrEmpty(musicName) || musicName.ToUpper() == "OFF")
            {
                CurrentMusic = null;
                return;
            }

            CurrentMusic = musicName;

            #if ANDROID
            var context = Android.App.Application.Context;
            int resId = context.Resources.GetIdentifier(musicName, "raw", context.PackageName);
            if (resId == 0) return; // הקובץ לא נמצא

            _player = MediaPlayer.Create(context, resId);
            _player.Looping = isLooping;               // לולאה אינסופית
            _player.SetVolume(1.0f, 1.0f);       // לוודא שהווליום מקסימלי
            _player.Start();                      // התחלת השמעה

            #elif IOS
                // הכנת AudioSession כדי שהשמע יושמע
                var audioSession = AVFoundation.AVAudioSession.SharedInstance();
                audioSession.SetCategory(AVFoundation.AVAudioSessionCategory.Playback);
                audioSession.SetActive(true);

                // טעינת הקובץ
                var url = NSUrl.FromFilename(musicName + ".mp3"); // חייב להיות ב-Resources עם Build Action=BundleResource
                _player = AVAudioPlayer.FromUrl(url);
                _player.NumberOfLoops = isLooping ? -1 : 0;  // לולאה אינסופית
                _player.PrepareToPlay();
                _player.Play();
            #endif
        }


        public void Pause()
        {
#if ANDROID
            if (_player?.IsPlaying ?? false)
                _player.Pause();
#elif IOS
        if (_player?.Playing ?? false)
            _player.Pause();
#endif
        }
        public void Unpause()
        {
#if ANDROID
    if (_player != null && !_player.IsPlaying)
    {
        _player.Start();
    }
#elif IOS
    if (_player != null && !_player.Playing)
    {
        _player.Play();
    }
#endif
        }

        public void Stop()
        {
#if ANDROID
            if (_player != null)
            {
                if (_player.IsPlaying)
                    _player.Stop();
                _player.Release();
                _player = null;
            }
#elif IOS
        if (_player != null)
        {
            if (_player.Playing)
                _player.Stop();
            _player.Dispose();
            _player = null;
        }
#endif
            CurrentMusic = null;
        }
    }
}
