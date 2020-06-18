using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Identity.Client;
using OneDrive_Cloud_Player.API;
using OneDrive_Cloud_Player.Main;

namespace OneDrive_Cloud_Player.Login
{
    class LoginViewModel : MetroWindow
    {
        private bool IsLoginButtonEnabled = true;

        public ICommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new CommandHandler(ExecuteLogin, CanExecuteLoginButton);
        }

        /// <summary>
        /// Determines whether or not the login button is enabled.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool CanExecuteLoginButton(object parameter)
        {
            return IsLoginButtonEnabled;
        }

        /// <summary>
        /// Starts the login process for the user.
        /// </summary>
        /// <param name="parameter"></param>
        private async void ExecuteLogin(object parameter)
        {
            // Disable the Login button.
            IsLoginButtonEnabled = false;
            // log the user in
            AuthenticationHandler auth = new AuthenticationHandler();
            AuthenticationResult LocalResult = await auth.GetAccessTokenForcedInteractive();
            // Check whether or not the user closed the popup dialog window.
            if (LocalResult == null)
            {
                // Enable the Login button.
                IsLoginButtonEnabled = true;
                return;
            }
            // Close this window and switch to the MainWindow.
            App.Current.SwitchWindows(new MainWindow());
        }
    }
}
