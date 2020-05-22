using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensibility;
using OneDrive_Cloud_Player.API.Authentication.InteractiveComponents;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OneDrive_Cloud_Player.API
{
    class Authenticate
    {
        public string AuthAccessToken { get; set; }

        public AuthenticationResult AuthResult { get; set; }


        public Authenticate()
        {

        }

        //private async string getAccessToken
        public async Task<string> AcquireAccessToken()
        {

            AuthenticationResult LocalResult = null;

            try
            {
                var accounts = await InitProgram.Current.PublicClientApplication.GetAccountsAsync();

                LocalResult = await InitProgram.Current.PublicClientApplication.AcquireTokenSilent(InitProgram.Current.Scopes, accounts.FirstOrDefault())
                       .ExecuteAsync();

                if (LocalResult != null)
                {
                    Console.WriteLine("Silent Acquire successfull.");
                }
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent.
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");


                try
                {
                    Console.WriteLine("Cannot do silent acquire. Trying interactive instead.");
                    LocalResult = await InitProgram.Current
                    .PublicClientApplication
                    .AcquireTokenInteractive(InitProgram.Current.Scopes)
                    .WithCustomWebUi(new EmbeddedBrowser(InitProgram.Current.MainWindow))
                    .ExecuteAsync();

                    if (LocalResult != null)
                    {
                        Console.WriteLine("Interactive acquire successfull.");
                    }
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

        public async void GetAccessTokenWithLogin()
        {
            AuthenticationResult LocalResult = null;

            try
            {

                LocalResult = await InitProgram.Current
                .PublicClientApplication
                .AcquireTokenInteractive(InitProgram.Current.Scopes)
                .WithCustomWebUi(new EmbeddedBrowser(InitProgram.Current.MainWindow))
                .ExecuteAsync();
            }
            catch (MsalException msalex)
            {
                Console.WriteLine($"Error Acquiring Token:{System.Environment.NewLine}{msalex}");
                Debug.WriteLine("\nLocalResult is NULL.\n");
            }
        }
    }
}
