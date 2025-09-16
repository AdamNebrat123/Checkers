using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Data
{
    /// <summary>
    /// Base service for interacting with Firebase Authentication and Firebase Realtime Database.
    /// Provides common logic such as initialization, authentication, and CRUD operations.
    /// </summary>
    public class FirebaseService
    {
        /// <summary>
        /// Firebase Authentication client used for user login and registration.
        /// </summary>
        protected readonly FirebaseAuthClient auth;

        /// <summary>
        /// Firebase Realtime Database client used for CRUD operations on data.
        /// </summary>
        protected readonly FirebaseClient firebaseClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebaseService"/> class.
        /// Sets up Firebase authentication and database clients.
        /// </summary>
        public FirebaseService()
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyCqYkZ67XuB62GrPzBrbtPgCtI0QIyTNL4",
                AuthDomain = "checkers-cc0c6.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
                UserRepository = new FileUserRepository("FirebaseApp")
            };

            auth = new FirebaseAuthClient(config);

            // Initialize Firebase Realtime Database  https://<project-id>-default-rtdb.firebaseio.com/
            firebaseClient = new FirebaseClient("https://checkers-cc0c6-default-rtdb.firebaseio.com/");
        }

        /// <summary>
        /// Registers a new user with email and password.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>UserCredential object containing user information.</returns>
        public async Task<UserCredential> RegisterAsync(string email, string password)
        {
            return await auth.CreateUserWithEmailAndPasswordAsync(email, password);
        }

        /// <summary>
        /// Logs in an existing user with email and password.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>UserCredential object containing user information.</returns>
        public async Task<UserCredential> LoginAsync(string email, string password)
        {
            return await auth.SignInWithEmailAndPasswordAsync(email, password);
        }

        /// <summary>
        /// Saves a document (overwrites existing data) in the specified collection.
        /// </summary>
        /// <param name="collection">The name of the collection.</param>
        /// <param name="docId">The document ID.</param>
        /// <param name="data">The data to save as a dictionary.</param>
        protected async Task SaveDocumentAsync(string collection, string docId, Dictionary<string, object> data)
        {
            try
            {
                await firebaseClient.Child(collection).Child(docId).PutAsync(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving document: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates a document partially (merges new data with existing data).
        /// </summary>
        /// <param name="collection">The name of the collection.</param>
        /// <param name="docId">The document ID.</param>
        /// <param name="data">The data to update.</param>
        public async Task UpdateDocumentAsync(string collection, string docId, Dictionary<string, object> data)
        {
            try
            {
                await firebaseClient.Child(collection).Child(docId).PatchAsync(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating document: {ex.Message}");
            }
        }
        /// <summary>
        /// Deletes a document from the specified collection.
        /// </summary>
        /// <param name="collection">The name of the collection.</param>
        /// <param name="docId">The document ID.</param>
        public async Task DeleteDocumentAsync(string collection, string docId)
        {
            try
            {
                await firebaseClient.Child(collection).Child(docId).DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting document: {ex.Message}");
            }
        }
        /// <summary>
        /// Retrieves all documents from a collection.
        /// </summary>
        /// <param name="collection">The name of the collection.</param>
        /// <returns>
        /// Dictionary with document IDs as keys and document data as values.
        /// Returns null if an error occurs.
        /// </returns>
        protected async Task<Dictionary<string, Dictionary<string, object>>> GetAllDocumentsAsync(string collection)
        {
            try
            {
                var result = await firebaseClient
                    .Child(collection)
                    .OnceAsync<Dictionary<string, object>>();

                return result.ToDictionary(
                    item => item.Key,
                    item => item.Object
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all documents: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retrieves a single document by ID from a collection.
        /// </summary>
        /// <param name="collection">The name of the collection.</param>
        /// <param name="docId">The document ID.</param>
        /// <returns>Dictionary containing the document data. Returns null if not found.</returns>
        protected async Task<Dictionary<string, object>> GetDocumentAsync(string collection, string docId)
        {
            try
            {
                var result = await firebaseClient
                    .Child(collection)
                    .Child(docId)
                    .OnceSingleAsync<Dictionary<string, object>>();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching document: {ex.Message}");
                return null;
            }
        }
    }
}
