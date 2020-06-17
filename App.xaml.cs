using active_directory_wpf_msgraph_v2;
using Microsoft.Identity.Client;
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

        public App()
        {
            CreateScopedPublicClientApplicationInstance();
        }

        public async void Initialize_Startup(object sender, StartupEventArgs e)
        {
            if (await IsLoggedIn())
            {
                // show Explorer Window
                StartupUri = new Uri("Main/MainWindow.xaml", UriKind.Relative);
            }
        }

        public async Task<bool> IsLoggedIn()
        {
            IEnumerable<IAccount> Accounts = await PublicClientApplication.GetAccountsAsync();
            return Accounts.Count() != 0;
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
