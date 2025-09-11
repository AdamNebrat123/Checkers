using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Helpers
{
    /// <summary>
    /// Centralized class for storing all project-wide constants.
    /// </summary>
    public static class Constants
    {
        #region User Realtime database fields

        public const string UsersCollection = "users";
        public const string FullName = "FullName";
        public const string UserName = "UserName";
        public const string Email = "Email";
        public const string MobileNo = "MobileNo";
        public const string BirthDate = "BirthDate";
        #endregion

        #region Category Realtime database fields
        public const string CategoriesCollection = "categories";
        public const string CategoryName = "Name";
        #endregion

        #region Prompt for adding category
        public const string NewCategoryTitle = "New Category";
        public const string NewCategoryMessage = "Enter the category name:";
        public const string CategoryPlaceholder = "Category Name";
        #endregion

        #region Alert messages
        public const string ErrorTitle = "Error";
        public const string CategoryAlreadyExistsMessage = "Category already exists";
        #endregion



        #region UI Element text
        public const string AddButtonText = "Add";
        public const string CancelButtonText = "Cancel";
        public const string DeleteButtonText = "Delete";
        public const string OkButtonText = "OK";

        #endregion


    }
}
