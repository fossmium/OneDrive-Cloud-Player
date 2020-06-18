using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Identity.Client;
using OneDrive_Cloud_Player.API;
using OneDrive_Cloud_Player.Main;

namespace OneDrive_Cloud_Player.Login
{
    class LoginViewModel : MetroWindow
    {
        public ICommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new CommandHandler(ExecuteLogin, CanExecute);
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        private async void ExecuteLogin(object parameter)
        {
            //make button unusable

            // log the user in
            AuthenticationHandler auth = new AuthenticationHandler();
            AuthenticationResult LocalResult = await auth.GetAccessTokenForcedInteractive();
            // check whether or not the user closed the popup dialog window.
            if (LocalResult == null)
            {
                // make button usable again
                return;
            }
            App.Current.SwitchWindows(new MainWindow());
        }
    }

}
