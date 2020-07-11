using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using OneDrive_Cloud_Player.Services.Helpers;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace OneDrive_Cloud_Player.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        public ICommand LoginCommand { get; }

        public LoginPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            LoginCommand = new RelayCommand(Login, CanExecuteCommand);
        }

        private bool CanExecuteCommand()
        {
            return true;
        }
     
        private async void Login()
        {
            GraphAuthHelper help = new GraphAuthHelper();
            await help.GetAccessToken();
            var accounts = await App.Current.PublicClientApplication.GetAccountsAsync();
            var LocalResult = await App.Current.PublicClientApplication.AcquireTokenSilent(App.Current.Scopes, accounts.FirstOrDefault())
                      .ExecuteAsync();
            Debug.WriteLine(LocalResult.Account.Username);

            // If the Cache.Count is not 0 upon login, this means that the user has logged out and is logging back in.
            // This is used to decide whether or not to read cache from disk. It prevents reading from old disk cache, since the cache is only written to disk upon application suspension.
            bool HasAlreadyLoggedIn = App.Current.CacheHelper.Cache.Count != 0;
            await App.Current.CacheHelper.Initialize(HasAlreadyLoggedIn);

            _navigationService.NavigateTo("MainPage");
        }
    }
}
