using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Checkers.Helpers;
namespace Checkers.Data
{
    // UserService class handles user registration and login functionality
    // Inherits from FirebaseService which provides methods to interact with Firebase Authentication and Realtime Database
    public class UserService : FirebaseService
    {

        // Registers a new user in Firebase Authentication and Realtime Database
        public async Task RegisterUserAsync(Models.User user, string password)
        {
            // Register the user in Firebase Authentication
            // Returns a UserCredential object if registration is successful
            var authUser = await RegisterAsync(user.Email, password);

            if (authUser != null)
            {
                // Get the unique Firebase UID of the registered user
                var firebaseId = authUser.User.Uid;

                // Prepare a dictionary of user data to store in Realtime Database
                var userData = new Dictionary<string, object>
                {
                    { Constants.FullName, user.FullName },
                    { Constants.UserName, user.UserName },
                    { Constants.Email, user.Email },
                    { Constants.MobileNo, user.MobileNo },
                    { Constants.BirthDate, user.BirthDate.ToString("yyyy-MM-dd") } // store date in standard format
                };

                // Save the user data under the /users/{firebaseId} path in Realtime Database
                await SaveDocumentAsync(Constants.UsersCollection, firebaseId, userData);
            }
        }

        // Logs in an existing user using Firebase Authentication
        public async Task<Models.User> LoginUserAsync(string email, string password)
        {
            // Attempt login via FirebaseService
            var authUser = await LoginAsync(email, password);

            if (authUser != null)
            {
                // Get the unique Firebase UID
                var firebaseId = authUser.User.Uid;

                // Retrieve the user data from Realtime Database at /users/{firebaseId}
                var userData = await GetDocumentAsync(Constants.UsersCollection, firebaseId);

                if (userData != null)
                {
                    // Map Realtime Database data to a Models.User object
                    return new Models.User
                    {
                        FullName = userData.TryGetValue(Constants.FullName, out var fullName) ? fullName.ToString() : "",
                        UserName = userData.TryGetValue(Constants.UserName, out var userName) ? userName.ToString() : "",
                        Email = userData.TryGetValue(Constants.Email, out var emailVal) ? emailVal.ToString() : "",
                        MobileNo = userData.TryGetValue(Constants.MobileNo, out var mobile) ? mobile.ToString() : "",
                        BirthDate = userData.TryGetValue(Constants.BirthDate, out var birth) ? DateTime.Parse(birth.ToString()) : DateTime.MinValue
                    };
                }
            }

            // Return null if login fails or user data does not exist
            return null;
        }
    }
}
