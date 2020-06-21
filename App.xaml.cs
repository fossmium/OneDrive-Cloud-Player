using active_directory_wpf_msgraph_v2;
using Microsoft.Identity.Client;
using OneDrive_Cloud_Player.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OneDrive_Cloud_Player
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //Other classes can now call this class with the use of 'App.Current'. 
        public static new App Current => (App)Application.Current;

        public IPublicClientApplication PublicClientApplication { get; private set; }
        public string[] Scopes { get; private set; }
        public CacheHandler CacheHandler { get; private set; }

        public App()
        {
            CreateScopedPublicClientApplicationInstance();
            CacheHandler = new CacheHandler();
        }

        /// <summary>
        /// Check whether or not the user credentials are cached
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void Initialize_Startup(object sender, StartupEventArgs e)
        {
            if (await IsLoggedIn())
            {
                await App.Current.CacheHandler.Initialize(false);
                // show Explorer Window
                StartupUri = new Uri("Main/MainWindow.xaml", UriKind.Relative);
            }
        }

        /// <summary>
        /// Save the graph cache on exit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            App.Current.CacheHandler.WriteGraphCache();
        }

        /// <summary>
        /// Check whether or not the user credentials are cached via MSAL
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsLoggedIn()
        {
            IEnumerable<IAccount> Accounts = await PublicClientApplication.GetAccountsAsync();
            return Accounts.Count() != 0;
        }

        /// <summary>
        /// Set the MainWindow to the NewWindow and close the previous MainWindow
        /// </summary>
        /// <param name="NewWindow"></param>
        public void SwitchWindows(Window NewWindow)
        {
            Window CurrentWindow = MainWindow;
            MainWindow = NewWindow;
            CurrentWindow.Close();
            NewWindow.Show();
        }

        /// <summary>
        /// Create a plublic client application instance and set it to the PublicClientApplication property.
        /// </summary>
        private void CreateScopedPublicClientApplicationInstance()
        {
            PublicClientApplication = PublicClientApplicationBuilder.Create("cfc49d19-b88e-4986-8862-8b5de253d0fd")
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                .Build();

            //Caches the login and keeps the user logged in after an application restart.
            TokenCacheHelper.EnableSerialization(PublicClientApplication.UserTokenCache);

            Scopes = new[]
                {
                    "offline_access",
                    "openid",
                    "profile",
                    "user.read",
                    "Files.Read.All"
                };
        }
    }
}
