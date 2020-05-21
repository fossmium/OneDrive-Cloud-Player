using Microsoft.Identity.Client.Extensibility;
using OneDrive_Cloud_Player.Login;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OneDrive_Cloud_Player.API
{
    class Authentication
    {
        public string MyProperty { get; set; }

        public Authentication()
        {

        }
        private string GetAccessToken()
        {
            return "";
        }

        //private async string getAccessToken
        private async void getAccessToken() {
            try
            {
                var r = await InitProgram.Current
                    .PublicClientApplication
                    .AcquireTokenInteractive(InitProgram.Current.Scopes)
                    .WithCustomWebUi(new EmbeddedBrowser(InitProgram.Current.MainWindow))
                    .ExecuteAsync();

                //NewAccessToken = r.AccessToken;
            }
            catch (Exception ex)
            {
                //textBlockOutput.Text = ex.Message;
                Debug.WriteLine(ex);
            }
        }
    }
}
