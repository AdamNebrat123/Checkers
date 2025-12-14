namespace Checkers.Views;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();

        UserName.Text = Preferences.Get("UserName", "UserName");
        Email.Text = Preferences.Get("Email", "Email");
        FullName.Text = Preferences.Get("FullName", "FullName");
        MobileNo.Text = Preferences.Get("MobileNo", "MobileNo");
        BirthDate.Text = Preferences.Get("BirthDate", "BirthDate");

    }
}