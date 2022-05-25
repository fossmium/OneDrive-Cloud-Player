using Microsoft.Identity.Client;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OneDrive_Cloud_Player.Services;
using OneDrive_Cloud_Player.Services.Helpers;
using OneDrive_Cloud_Player.Views;
using System.Windows.Input;

namespace OneDrive_Cloud_Player.ViewModels
{
    public class LoginPageViewModel : ObservableRecipient
    {
        public ICommand LoginCommand { get; }

        private bool isLoginButtonEnabled = true;

        public bool IsLoginButtonEnabled {
            get { return isLoginButtonEnabled; }
            set {
                isLoginButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public LoginPageViewModel()
        {
            LoginCommand = new RelayCommand(Login, CanExecuteLoginButton);
        }

        private bool CanExecuteLoginButton()
        {
            return IsLoginButtonEnabled;
        }
        
        private async void Login()
        {
            IsLoginButtonEnabled = false;
            GraphAuthHelper AuthHelper = new GraphAuthHelper();
            AuthenticationResult LocalResult = await AuthHelper.GetAccessTokenForcedInteractive();
            if (LocalResult is null)
            {
                IsLoginButtonEnabled = true;
                return;
            }

            // If the Cache.Count is not 0 upon login, this means that the user has logged out and is logging back in.
            // This is used to decide whether or not to read cache from disk. It prevents reading from old disk cache, since the cache is only written to disk upon application suspension.
            bool HasAlreadyLoggedIn = App.Current.CacheHelper.Cache.Count != 0;
            await App.Current.CacheHelper.Initialize(HasAlreadyLoggedIn);
            NavigationService.Navigate<MainPage>();
            IsLoginButtonEnabled = true;
        }
    }
}
