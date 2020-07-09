using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OneDrive_Cloud_Player.Services.Helpers
{
    class GraphAuthHelper
    {
        // The MSAL Public client app
        private static IPublicClientApplication PublicClientApp;

        private static string MSGraphURL = "https://graph.microsoft.com/v1.0/";

        public GraphAuthHelper() { }

        ///// <summary>
        ///// Sign in user using MSAL and obtain a token for MS Graph
        ///// </summary>
        ///// <returns>GraphServiceClient</returns>
        //private async static Task<GraphServiceClient> SignInAndInitializeGraphServiceClient(string[] scopes)
        //{
        //    GraphServiceClient graphClient = new GraphServiceClient(MSGraphURL,
        //        new DelegateAuthenticationProvider(async (requestMessage) =>
        //        {
        //            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", await SignInUserAndGetTokenUsingMSAL(scopes));
        //        }));

        //    return await Task.FromResult(graphClient);
        //}

        /// <summary>
        /// Tries to  silently retrieve the acces token without user interaction. If that fails it tries to retrieve the access token with an interactive login window.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccessToken()
        {
            AuthenticationResult LocalResult = null;
            try
            {
                var accounts = await App.Current.PublicClientApplication.GetAccountsAsync();
                LocalResult = await App.Current.PublicClientApplication.AcquireTokenSilent(App.Current.Scopes, accounts.FirstOrDefault())
                       .ExecuteAsync();

                if (LocalResult != null)
                {
                    Console.WriteLine(" + Silent Acquire successfull.");
                }
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent.
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");
                try
                {
                    Console.WriteLine(" + Cannot do silent acquire. Trying interactive instead.");
                    LocalResult = await App.Current
                    .PublicClientApplication
                    .AcquireTokenInteractive(App.Current.Scopes)
                    //.WithCustomWebUi(new EmbeddedBrowser(App.Current.MainWindow))
                    .ExecuteAsync();
                }
                catch (MsalException msalex)
                {
                    Debug.WriteLine($"Error Acquiring Token:{System.Environment.NewLine}{msalex}");
                    Debug.WriteLine("\nLocalResult is NULL.\n");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
                Debug.WriteLine("\nLocalResult is NULL.\n");
                return null;
            }

            if (LocalResult != null)
            {
                // Return the newly Acquired access token
                return LocalResult.AccessToken;
            }

            //Return null when the method failed to acquire a token.
            Debug.WriteLine("\nLocalResult is NULL.\n");
            return null;
        }

        /// <summary>
        /// Tries to acquire the acces token by forcing to use an interactive window.
        /// Returns null when user closes popup dialog window.
        /// </summary>
        public async Task<AuthenticationResult> GetAccessTokenForcedInteractive()
        {
            AuthenticationResult LocalResult = null;
            try
            {
                LocalResult = await App.Current
                .PublicClientApplication
                .AcquireTokenInteractive(App.Current.Scopes)
                //.WithCustomWebUi(new EmbeddedBrowser(App.Current.MainWindow))
                .ExecuteAsync();
                if (LocalResult != null)
                {
                    Console.WriteLine("Logged in using interaction.");
                }
            }
            catch (MsalException msalex)
            {
                Console.WriteLine($"Error Acquiring Token:{System.Environment.NewLine}{msalex}");
                Debug.WriteLine("\nLocalResult is NULL.\n");
            }
            // finally, return the AuthenticationResult so any callers can check for null
            return LocalResult;
        }

        /// <summary>
        /// Signs out a user. It removes the account from the token cache.
        /// </summary>
        public async Task SignOut()
        {
            IEnumerable<IAccount> accounts = await App.Current.PublicClientApplication.GetAccountsAsync();
            //accounts = (System.Collections.Generic.IEnumerable<IAccount>)accounts.ElementAt(0);

            //Checks if any account is inside the token cache.
            if (accounts.Any())
            {
                try
                {
                    await App.Current.PublicClientApplication.RemoveAsync(accounts.FirstOrDefault());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while trying to sign out: " + e);
                }
            }
        }
    }
}
