using Android.App;
using Android.Content.PM;
using Android.OS;
using Checkers.Services;

namespace Checkers
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnPause()
        {
            base.OnPause();
            MauiApplication.Current.Services
                .GetService<ISoundEffectService>()?
                .Pause();

            MauiApplication.Current.Services
                .GetService<IBackgroundMusicService>()?
                .Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            MauiApplication.Current.Services
                .GetService<ISoundEffectService>()?
                .Unpause();

            MauiApplication.Current.Services
                .GetService<IBackgroundMusicService>()?
                .Unpause();
        }
    }
}
