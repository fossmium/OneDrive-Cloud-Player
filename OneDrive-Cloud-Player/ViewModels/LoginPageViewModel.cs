using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OneDrive_Cloud_Player.Services.Helpers;
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
            //Window.Current.CoreWindow.

            bool HasAlreadyLoggedIn = App.Current.CacheHelper.Cache.Count != 0;
            await App.Current.CacheHelper.Initialize(HasAlreadyLoggedIn);

            await TryOpenNewWindow(typeof(MainPage));

            await ApplicationView.GetForCurrentView().TryConsolidateAsync();
            //Window.Current.Close();
        }

        public static async Task<bool> TryOpenNewWindow(Type page)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                Frame frame = new Frame();
                frame.Navigate(page);
                Window.Current.Content = frame;
                // You have to activate the window in order to show it later.
                Window.Current.Activate();

                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
            return viewShown;
        }
    }
}
