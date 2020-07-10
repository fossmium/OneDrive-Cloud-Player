using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OneDrive_Cloud_Player.Services.Helpers;
using OneDrive_Cloud_Player.Services.Utilities;
using OneDrive_Cloud_Player.Views;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OneDrive_Cloud_Player.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        public ICommand LoginCommand { get; }

        public LoginPageViewModel()
        {
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

            await WindowSwitcher.TryOpenNewWindow(typeof(MainPage));

            await ApplicationView.GetForCurrentView().TryConsolidateAsync();
            //Window.Current.Close();
        }
    }
}
