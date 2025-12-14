using Checkers.Data;
using Checkers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Checkers.ViewModels
{
    /// <summary>
    /// ViewModel responsible for handling user login logic,
    /// validation, and navigation. Uses MVVM pattern for binding
    /// between the UI and backend logic.
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        #region Fields
        private readonly UserService usrService; // Service to handle user operations (login, register, etc.)
        private string email;                     // Holds the user's entered email
        private string password;                  // Holds the user's entered password
        private bool isPasswordVisible;           // Indicates if the password should be visible (toggle eye icon)
        private string emailError;                // Error message for email validation
        private string passwordError;             // Error message for password validation
        private bool isLoading;                   // Indicates if a login operation is currently in progress
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of LoginViewModel.
        /// Commands are initialized here and property change
        /// listeners are configured for form validation.
        /// </summary>
        public LoginViewModel(UserService userService)
        {
            this.usrService = userService;
            isLoading = false;

            // Initialize commands
            NavigateToSignUpCommand = new Command(OnNavigateToSignUp);
            TogglePasswordVisibilityCommand = new Command(TogglePasswordVisibility);
            LoginCommand = new Command(OnLogin, CanLogin);
            CancelCommand = new Command(OnCancel, CanCancel);

            // Refresh command availability when Email or Password changes
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Email) || e.PropertyName == nameof(Password))
                {
                    ((Command)LoginCommand).ChangeCanExecute();
                    ((Command)CancelCommand).ChangeCanExecute();
                    OnPropertyChanged(nameof(IsLoginEnabled));
                }
            };
        }
        #endregion

        #region Properties
        /// <summary>
        /// Indicates if the login process is loading (used for showing activity indicator).
        /// </summary>
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        /// <summary>
        /// User's email input.
        /// Validation is triggered whenever the value changes.
        /// </summary>
        public string Email
        {
            get => email;
            set
            {
                if (email != value)
                {
                    email = value;
                    OnPropertyChanged(nameof(Email));
                    HandleError(nameof(Email));
                }
            }
        }

        /// <summary>
        /// User's password input.
        /// Validation is triggered whenever the value changes.
        /// </summary>
        public string Password
        {
            get => password;
            set
            {
                if (password != value)
                {
                    password = value;
                    OnPropertyChanged(nameof(Password));
                    HandleError(nameof(Password));
                }
            }
        }

        /// <summary>
        /// Determines if the password should be displayed in plain text.
        /// Bound to "eye" toggle button in UI.
        /// </summary>
        public bool IsPasswordVisible
        {
            get => isPasswordVisible;
            set
            {
                if (isPasswordVisible != value)
                {
                    isPasswordVisible = value;
                    OnPropertyChanged(nameof(IsPasswordVisible));
                    OnPropertyChanged(nameof(IsPasswordHidden));
                }
            }
        }

        /// <summary>
        /// Inverse of IsPasswordVisible (used in XAML bindings).
        /// </summary>
        public bool IsPasswordHidden => !IsPasswordVisible;

        /// <summary>
        /// Error message for the Email field (shown in UI if invalid).
        /// </summary>
        public string EmailError
        {
            get => emailError;
            set
            {
                if (emailError != value)
                {
                    emailError = value;
                    OnPropertyChanged(nameof(EmailError));
                }
            }
        }

        /// <summary>
        /// Error message for the Password field (shown in UI if invalid).
        /// </summary>
        public string PasswordError
        {
            get => passwordError;
            set
            {
                if (passwordError != value)
                {
                    passwordError = value;
                    OnPropertyChanged(nameof(PasswordError));
                }
            }
        }

        /// <summary>
        /// True if both Email and Password fields are filled.
        /// Used to enable/disable Login button.
        /// </summary>
        public bool IsLoginEnabled => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);

        /// <summary>
        /// True if at least one of the fields (Email/Password) is filled.
        /// Used to enable/disable Cancel button.
        /// </summary>
        public bool IsCancelEnabled => !string.IsNullOrWhiteSpace(Email) || !string.IsNullOrWhiteSpace(Password);

        /// <summary>
        /// True if there are any validation errors.
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(EmailError) || !string.IsNullOrEmpty(PasswordError);
        #endregion

        #region Commands
        public ICommand NavigateToSignUpCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Validates properties and sets error messages accordingly.
        /// </summary>
        private void HandleError(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Email):
                    EmailError = string.IsNullOrEmpty(Email) ? "Email is required" : string.Empty;
                    break;
                case nameof(Password):
                    PasswordError = string.IsNullOrEmpty(Password) ? "Password is required" : string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Navigates the user to the registration page.
        /// </summary>
        private async void OnNavigateToSignUp()
        {
            try
            {
                await Shell.Current.GoToAsync("//RegisterPage");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Navigation Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Attempts to log in the user using UserService.
        /// If successful, navigates to ProductsPage and sets AuthenticatedShell.
        /// </summary>
        private async void OnLogin()
        {
            if (!HasError)
            {
                try
                {
                    IsLoading = true; // Start loading indicator

                    User user = await usrService.LoginUserAsync(Email, Password);
                    if (user != null)
                    {
                        // Store username securely for future sessions
                        await SecureStorage.SetAsync("Email", user.Email);
                        await SecureStorage.SetAsync("Password", Password);

                        Preferences.Set("UserName", user.UserName);
                        Preferences.Set("Email", user.Email);
                        Preferences.Set("FullName", user.FullName);
                        Preferences.Set("MobileNo", user.MobileNo);
                        Preferences.Set("BirthDate", user.BirthDate.ToString("dd MMM yyyy"));


                        // Navigate to authenticated page
                        Application.Current.MainPage = new AuthenticatedShell();
                        await Shell.Current.GoToAsync("///HomePage");


                    }
                    else
                    {
                        PasswordError = "Invalid credentials, please try again.";
                        await ShowAlert("Login", PasswordError);
                    }
                }
                catch (Exception ex)
                {
                    await ShowAlert("Error", $"Login failed: {ex.Message}");
                }
                finally
                {
                    IsLoading = false; // End loading indicator
                }
            }
        }

        public async Task TryAutoLogin(string email, string password)
        {
            User user = await usrService.LoginUserAsync(email, password);
            if (user == null)
                return;


            // Navigate to authenticated page
            Application.Current.MainPage = new AuthenticatedShell();

        }

        private async Task NavigateToMainPage()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        /// <summary>
        /// Displays an alert message on the screen.
        /// </summary>
        private Task ShowAlert(string title, string message)
        {
            return Application.Current.MainPage.DisplayAlert(title, message, "OK");
        }

        /// <summary>
        /// Determines if the Login command can execute.
        /// </summary>
        private bool CanLogin()
        {
            return IsLoginEnabled && !HasError;
        }

        /// <summary>
        /// Clears the Email and Password fields.
        /// </summary>
        private void OnCancel()
        {
            Email = string.Empty;
            Password = string.Empty;
            ClearErrors();
            NavigateToMainPage();
        }

        /// <summary>
        /// Clears all error messages.
        /// </summary>
        private void ClearErrors()
        {
            EmailError = string.Empty;
            PasswordError = string.Empty;
        }

        /// <summary>
        /// Determines if the Cancel command can execute.
        /// </summary>
        private bool CanCancel()
        {
            return IsCancelEnabled;
        }

        /// <summary>
        /// Toggles password visibility (show/hide).
        /// </summary>
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }
        #endregion
    }
}
