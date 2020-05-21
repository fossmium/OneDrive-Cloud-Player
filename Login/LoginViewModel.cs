using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Extensibility;
using System.Diagnostics;
using Microsoft.Identity.Client;
using System.Linq;
using System.Threading;
using System.Timers;

namespace OneDrive_Cloud_Player.Login
{
    class LoginViewModel : Window
    {
        public ICommand MyCommand { get; set; }

        private GraphServiceClient graphServiceClient { get; set; }

        private GraphServiceClient graphClientSilentTest { get; set; }

        private static System.Timers.Timer aTimer;

        public string NewAccessToken { get; set; }

        public LoginViewModel()
        {
            MyCommand = new CommandHandler(ExecuteMethod, CanExecuteMethod);
        }


        private bool CanExecuteMethod(object parameter)
        {
            return true;
        }

        private async void ExecuteMethod(object parameter)
        {
            await GenerateLoginWindowAsync();

            setTimer();
        }

        private void setTimer()
        {
            aTimer = new System.Timers.Timer(60000);
            aTimer.Elapsed += SilentToken;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private async Task GenerateLoginWindowAsync()
        {
            try
            {
                var r = await InitProgram.Current
                    .PublicClientApplication
                    .AcquireTokenInteractive(InitProgram.Current.Scopes)
                    .WithCustomWebUi(new EmbeddedBrowser(InitProgram.Current.MainWindow))
                    .ExecuteAsync();

                NewAccessToken = r.AccessToken;



                graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
                  {
                      requestMessage
                          .Headers
                          .Authorization = new AuthenticationHeaderValue("Bearer", NewAccessToken);
                      //Console.Write("Account: " + r.Account + "\n");
                      //Console.Write("\n");
                      //Console.Write("Acces token: " + r.AccessToken + "\n");
                      //Console.Write("\n");
                      //Console.Write("Expire Date: " + r.ExpiresOn + "\n");

                      return Task.FromResult(0);
                  }));



                /*
                 * 
                 * Writes all folders/ files shared with a user.
                 * 
                 */

                var drive = await graphServiceClient.Me.Drive.SharedWithMe().Request().GetAsync();
                //MessageBox.Show(drive.Id);
                foreach (var item in drive)
                {
                    Debug.WriteLine(item.Id);
                }
            }
            catch (Exception ex)
            {
                //textBlockOutput.Text = ex.Message;
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        ///  Silent account call without user interaction.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private async void SilentToken(Object source, ElapsedEventArgs e)
        {
            AuthenticationResult result;

            var accounts = await InitProgram.Current.PublicClientApplication.GetAccountsAsync();

            //Iets met authentication key ophalen ofzo. zorgt er iig voor dat graph calls uitgevoerd kunnen worden.
            graphClientSilentTest = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("Bearer", NewAccessToken);

                return Task.FromResult(0);
            }));


            //Silently acquire a new token.
            result = await InitProgram.Current.PublicClientApplication.AcquireTokenSilent(InitProgram.Current.Scopes, accounts.FirstOrDefault())
                             .ExecuteAsync();

            //Write username of the drive on screen.
            Console.Write("[" + System.DateTime.Now + "] " + "Account name: " + result.Account.Username + "\n");

            NewAccessToken = result.AccessToken;


            //Execute graph call.
            var driveId = await graphClientSilentTest.Me.Drives["b!MFdcYTQb50KRsCG7n7NTZLiOhD1-AB1Kj2aUdVa53fBBD1J-dnclTaEaS6tBko9-"].Request().GetAsync();

            //Write graph call result to screen.
            Console.WriteLine("[" + System.DateTime.Now + "] " + "Drive name: " + driveId.Name);
        }

    }

}
