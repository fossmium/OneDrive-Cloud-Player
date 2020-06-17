using System.Windows.Input;
using MahApps.Metro.Controls;
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
            // log the user in
            AuthenticationHandler auth = new AuthenticationHandler();
            await auth.GetAccessTokenForcedInteractive();

            //close the login window and open the explorer window
            MainWindow MainWindow = new MainWindow();
            LoginWindow LoginWindow = (LoginWindow)App.Current.MainWindow;
            App.Current.MainWindow = MainWindow;
            MainWindow.Show();
            LoginWindow.Close();
        }
    }

}
