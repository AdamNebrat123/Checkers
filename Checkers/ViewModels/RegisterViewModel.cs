using Checkers.Data;
using Checkers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Checkers.ViewModels
{
    /// <summary>
    /// ViewModel for handling user registration logic, validation,
    /// and navigation within the application.
    /// </summary>
    public class RegisterViewModel : ViewModelBase
    {
        #region Fields
        // Service responsible for handling user-related operations
        private readonly UserService usrService;

        // Backing fields for user input
        private string? firstName;
        private string? lastName;
        private string? userName;
        private string? email;
        private string? mobileNo;
        private string? password;
        private DateTime birthDate = DateTime.Now;

        // Backing fields for validation error messages
        private string? fullNameError;
        private string? userNameError;
        private string? emailError;
        private string? mobileNoError;
        private string? passwordError;
        private string? birthDateError;
        #endregion

        #region Commands
        // Command executed when user clicks the Register button
        public ICommand RegisterCommand { get; private set; }

        // Command executed when user wants to navigate to Sign In page
        public ICommand NavigateToSignInCommand { get; }
        #endregion

        #region Constructor
        public RegisterViewModel(UserService userService)
        {
            // Initialize fields with default values
            firstName = string.Empty;
            lastName = string.Empty;
            userName = string.Empty;
            email = string.Empty;
            mobileNo = string.Empty;
            password = string.Empty;
            birthDate = DateTime.Now;

            this.usrService = userService;

            // Bind commands to their respective methods
            RegisterCommand = new Command(OnRegister);
            NavigateToSignInCommand = new Command(OnNavigateToSignIn);
        }
        #endregion

        #region Properties

        // First name input with validation trigger
        public string FirstName
        {
            get => firstName;
            set
            {
                if (firstName != value)
                {
                    firstName = value;
                    OnPropertyChanged(nameof(FirstName));
                    HandleError(nameof(FirstName));
                }
            }
        }

        // Last name input with validation trigger
        public string LastName
        {
            get => lastName;
            set
            {
                if (lastName != value)
                {
                    lastName = value;
                    OnPropertyChanged(nameof(LastName));
                    HandleError(nameof(LastName));
                }
            }
        }

        // Username input with validation trigger
        public string UserName
        {
            get => userName;
            set
            {
                if (userName != value)
                {
                    userName = value;
                    OnPropertyChanged(nameof(UserName));
                    HandleError(nameof(UserName));
                }
            }
        }

        // Email input with validation trigger
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

        // Mobile number input with validation trigger
        public string MobileNo
        {
            get => mobileNo;
            set
            {
                if (mobileNo != value)
                {
                    mobileNo = value;
                    OnPropertyChanged(nameof(MobileNo));
                    HandleError(nameof(MobileNo));
                }
            }
        }

        // Password input with validation trigger
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

        // Date of birth input with validation trigger
        public DateTime BirthDate
        {
            get => birthDate;
            set
            {
                if (birthDate != value)
                {
                    birthDate = value;
                    OnPropertyChanged(nameof(BirthDate));
                    OnPropertyChanged(nameof(Age)); // Recalculate age
                    HandleError(nameof(BirthDate));
                }
            }
        }

        // Calculated Age property based on BirthDate
        public int Age => (DateTime.Now.Year - BirthDate.Year) -
            (DateTime.Now.DayOfYear < BirthDate.DayOfYear ? 1 : 0);

        // Error message properties for validation
        public string FullNameError
        {
            get => fullNameError;
            set
            {
                if (fullNameError != value)
                {
                    fullNameError = value;
                    OnPropertyChanged(nameof(FullNameError));
                }
            }
        }

        public string UserNameError
        {
            get => userNameError;
            set
            {
                if (userNameError != value)
                {
                    userNameError = value;
                    OnPropertyChanged(nameof(UserNameError));
                }
            }
        }
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
        public string MobileNoError
        {
            get => mobileNoError;
            set
            {
                if (mobileNoError != value)
                {
                    mobileNoError = value;
                    OnPropertyChanged(nameof(MobileNoError));
                }
            }
        }
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
        public string BirthDateError
        {
            get => birthDateError;
            set
            {
                if (birthDateError != value)
                {
                    birthDateError = value;
                    OnPropertyChanged(nameof(BirthDateError));
                }
            }
        }
        // Indicates if there are any validation errors
        public bool HasError =>
            !string.IsNullOrEmpty(FullNameError) ||
            !string.IsNullOrEmpty(PasswordError) ||
            !string.IsNullOrEmpty(EmailError) ||
            !string.IsNullOrEmpty(userNameError) ||
            !string.IsNullOrEmpty(birthDateError) ||
            !string.IsNullOrEmpty(mobileNoError);

        // Determines if registration can proceed
        public bool CanRegister => !HasError;

        #endregion

        #region Methods

        /// <summary>
        /// Handles validation logic for each property by updating the error fields.
        /// </summary>
        private void HandleError(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(FirstName):
                case nameof(LastName):
                    FullNameError = ValidateFullName() ? string.Empty : "Full Name is required and must contain only letters.";
                    break;
                case nameof(UserName):
                    UserNameError = ValidateUserName() ? string.Empty : "User Name is required, cannot start with a digit, \nand cannot contain spaces.";
                    break;
                case nameof(Email):
                    EmailError = ValidateEmail() ? string.Empty : "Valid Email is required.";
                    break;
                case nameof(MobileNo):
                    MobileNoError = ValidateMobileNumber() ? string.Empty : "Mobile Number is required and must be numeric \nand has 10 digits";
                    break;
                case nameof(Password):
                    PasswordError = ValidatePassword() ? string.Empty : "Password must contain at least one uppercase \nletter and one number.";
                    break;
                case nameof(BirthDate):
                    BirthDateError = ValidateBirthDate() ? string.Empty : "Birth Date is required and must be at least 18 years old.";
                    break;
            }

            // Refresh dependent properties
            OnPropertyChanged(nameof(CanRegister));
            OnPropertyChanged(nameof(HasError));
        }
        #region Validation Methods
        // Checks first and last name validity
        private bool ValidateFullName() =>
            !string.IsNullOrEmpty(FirstName) &&
            !string.IsNullOrEmpty(LastName) &&
            Regex.IsMatch(FirstName, @"^[a-zA-Z]+$") &&
            Regex.IsMatch(LastName, @"^[a-zA-Z]+$");

        // Checks username validity
        private bool ValidateUserName()
        {
            if (string.IsNullOrEmpty(UserName)) return false;
            return Regex.IsMatch(UserName, @"^[a-zA-Z][a-zA-Z0-9]*$");
        }

        // Checks email validity
        private bool ValidateEmail()
        {
            if (string.IsNullOrEmpty(Email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(Email);
                return addr.Address == Email;
            }
            catch
            {
                return false;
            }
        }

        // Checks mobile number validity
        private bool ValidateMobileNumber()
        {
            if (string.IsNullOrEmpty(MobileNo)) return false;
            return Regex.IsMatch(MobileNo, @"^\d{10}$");
        }

        // Checks password validity
        private bool ValidatePassword() =>
            !string.IsNullOrEmpty(Password) &&
            Regex.IsMatch(Password, @"^(?=.*[A-Z])(?=.*\d).+$");

        // Checks if user is at least 18 years old
        private bool ValidateBirthDate()
        {
            var age = DateTime.Now.Year - BirthDate.Year;
            if (DateTime.Now.DayOfYear < BirthDate.DayOfYear) age--;
            return age >= 18;
        }
        #endregion

        /// <summary>
        /// Executes user registration process by creating a user and calling the UserService.
        /// </summary>
        private async void OnRegister()
        {
            if (!HasError)
            {
                try
                {
                    User user = CreateUser();

                    // Save the user using the UserService
                    await usrService.RegisterUserAsync(user, user.Password);

                    await Application.Current.MainPage.DisplayAlert("Success", "Registration successful", "OK");

                    // Navigate to LoginPage after successful registration
                    if (Shell.Current != null)
                    {
                        await Shell.Current.GoToAsync("//LoginPage");
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Registration failed: {ex.Message}", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please correct the errors before proceeding.", "OK");
            }
        }

        /// <summary>
        /// Creates a User object from the current ViewModel properties.
        /// </summary>
        private User CreateUser()
        {
            return new User
            {
                FullName = $"{LastName} {FirstName}",
                UserName = UserName,
                Email = Email,
                MobileNo = MobileNo,
                Password = Password,
                BirthDate = BirthDate
            };
        }

        /// <summary>
        /// Navigates to the Login page via Shell.
        /// </summary>
        private async void OnNavigateToSignIn()
        {
            try
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Navigation Error", ex.Message, "OK");
            }
        }
        #endregion
    }
}
