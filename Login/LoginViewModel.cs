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
using OneDrive_Cloud_Player.API.Authentication.InteractiveComponents;
using OneDrive_Cloud_Player.API;

namespace OneDrive_Cloud_Player.Login
{
    class LoginViewModel : Window
    {
        public ICommand MyCommand { get; set; }
        public ICommand MyCommand2 { get; set; }

        public string NewAccessToken { get; set; }

        public LoginViewModel()
        { 
            MyCommand = new CommandHandler(ExecuteMethod, CanExecuteMethod);

            MyCommand2 = new CommandHandler(ExecuteMethod2, CanExecuteMethod2);
        }


        private bool CanExecuteMethod(object parameter)
        {
            return true;
        }

        private void ExecuteMethod(object parameter)
        {
            Authenticate Auth = new Authenticate();
            Auth.GetAccessTokenWithLogin();
        }

        private bool CanExecuteMethod2(object parameter)
        {
            return true;
        }

        private async void ExecuteMethod2(object parameter)
        {
            Authenticate Auth = new Authenticate();
            await Auth.AcquireAccessToken();
        }
    }

}
