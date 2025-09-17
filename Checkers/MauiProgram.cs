﻿using Checkers.Data;
using Checkers.ViewModel;
using Checkers.ViewModels;
using Checkers.Views;
using Microsoft.Extensions.Logging;

namespace Checkers
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();

            // Register Services, ViewModels, Pages
            RegisterServices(builder);
            RegisterViewModels(builder);
            RegisterPages(builder);
#endif

            return builder.Build();
        }

        private static void RegisterViewModels(MauiAppBuilder builder)
        {
            
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<HomePageViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<BoardViewModel>(); 
            builder.Services.AddTransient<GameManagerViewModel>();


        }

        private static void RegisterPages(MauiAppBuilder builder)
        {
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<CreateGame>();
            builder.Services.AddTransient<WaitingRoom>();
            builder.Services.AddTransient<GamePage>();
        }

        private static void RegisterServices(MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<FirebaseService>();
            builder.Services.AddSingleton<UserService>();

        }
    }
}
