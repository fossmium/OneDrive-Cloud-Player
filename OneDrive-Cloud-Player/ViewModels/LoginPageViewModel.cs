using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Identity.Client;
using OneDrive_Cloud_Player.Services.Helpers;
using System.Windows.Input;

namespace OneDrive_Cloud_Player.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        public ICommand LoginCommand { get; }

        private bool isLoginButtonEnabled = true;

        public bool IsLoginButtonEnabled {
            get { return isLoginButtonEnabled; }
            set {
                isLoginButtonEnabled = value;
                RaisePropertyChanged("IsReloadButtonEnabled");
            }
        }

        public LoginPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
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
            _navigationService.NavigateTo("MainPage");
            IsLoginButtonEnabled = true;
        }
    }
}
