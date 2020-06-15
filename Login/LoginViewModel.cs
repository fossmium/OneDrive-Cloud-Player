using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Graph;
using OneDrive_Cloud_Player.API;

namespace OneDrive_Cloud_Player.Login
{
    class LoginViewModel : MetroWindow
    {
        public ICommand MyCommand { get; set; }
        public ICommand MyCommand2 { get; set; }
        public ICommand MyCommand3 { get; set; }

        private GraphHandler graph { get; set; }

        private bool LockAuthCall { get; set; }

        public LoginViewModel()
        {
            graph = new GraphHandler();
            LockAuthCall = false;

            MyCommand = new CommandHandler(ExecuteMethod, CanExecuteMethod);
            MyCommand2 = new CommandHandler(ExecuteTestToken, CanExecuteMethod2);
            MyCommand3 = new CommandHandler(ExecuteLogout, CanExecuteMethod2);
        }

        private void ExecuteLogout(object obj)
        {
            API.AuthenticationHandler auth = new API.AuthenticationHandler();
            auth.SignOut();
        }

        private bool CanExecuteMethod(object parameter)
        {
            return true;
        }

        private void ExecuteMethod(object parameter)
        {
            API.AuthenticationHandler auth = new API.AuthenticationHandler();
            auth.GetAccessTokenForcedInteractive();
        }

        private bool CanExecuteMethod2(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Contains test and example code and is meant to give you an idea how the api calls can be used. 
        /// Do not just copy this over to production code.
        /// When using graph calls, try to find a way to cache or save results so you have to call as little as possible.
        /// The API is to SLOW!!! to handle useless requests.
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
                User OwnerInformation = await graph.GetOneDriveUserInformationAsync();
                //Display the full name of the OneDrive owner that called the graph api.
                Console.WriteLine("OneDrive Owner Display Name: " + OwnerInformation.GivenName);

                Drive YourDriveInformation = await graph.GetDriveInformationAsync("3");
                //Get the name of the owner of the caller his OneDrive.
                Console.WriteLine("Drive Owner Name: " + YourDriveInformation.Owner.User.DisplayName);


                IDriveSharedWithMeCollectionPage SharedItems = await graph.GetSharedDrivesAsync();
                //Get the name of the first shared folder.
                Console.WriteLine("Shared item name: " + SharedItems[0].RemoteItem.Name);
                //Store the drive id from the shared folder.
                string SharedDriveId = SharedItems[0].RemoteItem.ParentReference.DriveId;
                //Store the itemid (file and folders are items)
                string SharedItemId = SharedItems[0].RemoteItem.Id;


                Drive SharedDriveInformation = await graph.GetDriveInformationAsync(SharedDriveId);
                //Display the owner of the shared item.
                Console.WriteLine("Shared Drive Owner Name: " + SharedDriveInformation.Owner.User.DisplayName);


                IDriveItemChildrenCollectionPage Children = await graph.GetChildrenOfItemAsync(SharedItemId, SharedDriveId);
                //Display all children inside the shared folder.
                foreach (var child in Children)
                {
                    Console.WriteLine(child.Name);
                }


                //Get the profile picture of the OneDrive owner.
                //IO stream containing the photo.
                //More information on how to work with the photo you can find here: https://stackoverflow.com/questions/42126660/c-sharp-how-to-get-office-365-user-photo-using-microsoft-graph-api
                Stream OwnerPhoto = await graph.GetOneDriveOwnerPhotoAsync();


            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e + "\n Error" + "\n");
            }
            LockAuthCall = false;
        }
    }

}
