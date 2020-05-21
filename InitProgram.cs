using OneDrive_Cloud_Player;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Interop;
using System.Threading;

namespace OneDrive_Cloud_Player
{
    public partial class InitProgram
    {
        public InitProgram()
        {
            CreateScopedPublicClientApplicationInstance();
            //TestAccountAsync();

        }

        public static new InitProgram Current => (InitProgram)Application.Current;
        public IPublicClientApplication PublicClientApplication { get; private set; }
        public string[] Scopes { get; private set; }

        /// <summary>
        /// Create a plublic client application instance and set it to the PublicClientApplication property.
        /// </summary>
        private void CreateScopedPublicClientApplicationInstance()
        {


            PublicClientApplication = PublicClientApplicationBuilder.Create("cfc49d19-b88e-4986-8862-8b5de253d0fd")
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                .Build();
            Scopes = new[]
                {
                    "offline_access",
                    "openid",
                    "profile",
                    "user.read"
                };

            
            //var token = PublicClientApplication.AcquireTokenSilentAsync(Scopes, PublicClientApplication.Users.First());


        }
        private async Task TestAccountAsync()
        {
            
            AuthenticationResult result;

            var accounts = await PublicClientApplication.GetAccountsAsync();

            IAccount account = accounts.First();
            // for instance accounts.FirstOrDefault
            // if the app manages is at most one account  
            try
            {
                result = await PublicClientApplication.AcquireTokenSilent(Scopes, account)
                                  .ExecuteAsync();
                Debug.Write("Access token silent: " + result.AccessToken + "\n");
            }
            catch (MsalUiRequiredException ex)
            {
                //result = await PublicClientApplication.AcquireTokenInteractive(ScrollAmount, account)
                //                  .WithOptionalParameterXXX(parameter)
                //                  .ExecuteAsync();
                Debug.Write("EXEPTION SILENT");
            }
        }

    }
}
