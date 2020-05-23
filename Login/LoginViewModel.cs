using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Graph;
using OneDrive_Cloud_Player.API;

namespace OneDrive_Cloud_Player.Login
{
    class LoginViewModel : Window
    {
        public ICommand MyCommand { get; set; }
        public ICommand MyCommand2 { get; set; }

        private Graph graph { get; set; }

        private bool LockAuthCall { get; set; }

        public LoginViewModel()
        {
            graph = new Graph();
            LockAuthCall = false;

            MyCommand = new CommandHandler(ExecuteMethod, CanExecuteMethod);
            MyCommand2 = new CommandHandler(ExecuteTestToken, CanExecuteMethod2);
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

        /// <summary>
        /// </summary>
        /// <param name="parameter"></param>
        private async void ExecuteTestToken(object parameter)
        {
            //Check if a request is already executing.
            if (LockAuthCall)
            {
                return;
            }
            LockAuthCall = true;
            try
            {
                Drive DriveInformation = await graph.GetDriveInformationAsync("b!MFdcYTQb50KRsCG7n7NTZLiOhD1-AB1Kj2aUdVa53fBBD1J-dnclTaEaS6tBko9-");
                Console.WriteLine("Drive Name: " + DriveInformation.Name);

                IDriveItemChildrenCollectionPage Children = await graph.GetChildrenOfItem("01V3DWMJRSJGFGHNXT5JCJPQ4PZRGDBEQM");
                foreach (var child in Children)
                {
                    Console.WriteLine(child.Name);
                }

                User OwnerInformation = await graph.GetOneDriveOwnerInformationAsync();
                Console.WriteLine("Owner Information: " + OwnerInformation.Drive);

                //IO stream containing the photo.
                //More information how to work with the photo you can find here: https://stackoverflow.com/questions/42126660/c-sharp-how-to-get-office-365-user-photo-using-microsoft-graph-api
                Stream OwnerPhoto = await graph.GetOneDriveOwnerPhotoAsync();

                //IDriveSharedWithMeCollectionPage SharedItems = await graph.GetSharedItemsAsync();
                //Console.WriteLine("Shared item names: ");
                //foreach (var item in SharedItems)
                //{
                //    //Only display the shared items that are of type folder.
                //    if (item.Folder != null)
                //    {
                //        Console.WriteLine(" " + item.Name);
                //    }
                //}


            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e + "\n Error" + "\n");
            }
            LockAuthCall = false;
        }
    }

}
