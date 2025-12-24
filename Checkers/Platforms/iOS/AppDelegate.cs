using Checkers.Services;
using Foundation;
using UIKit;

namespace Checkers
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override void OnResignActivation(UIApplication application)
        {
            MauiUIApplicationDelegate.Current.Services
                .GetService<ISoundEffectService>()?
                .Pause();
        }

        public override void OnActivated(UIApplication application)
        {
            MauiUIApplicationDelegate.Current.Services
                .GetService<ISoundEffectService>()?
                .Unpause();
        }

    }
}
