using Microsoft.Graph;
using OneDrive_Cloud_Player.API;
using OneDrive_Cloud_Player.Explorer;
using OneDrive_Cloud_Player.Login;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Explorer
{
    class ExplorerView : INotifyPropertyChanged
    {

        public ICommand TestCallCommand { get; set; }
        private Graph graph;
        public List<DriveItem> folders;
        
        public ExplorerView()
        {
            Console.WriteLine("Call");
            folders = new List<DriveItem>();
            this.graph = new Graph();
            TestCallCommand = new OneDrive_Cloud_Player.Explorer.CommandHandler(ExecuteMethod, CanExecuteMethod);
        }

        private bool CanExecuteMethod(object arg)
        {
            return true;
        }

        private async void ExecuteMethod(object obj)
        {
            await TestCallAsync();
        }

        public async Task TestCallAsync()
        {
            IDriveSharedWithMeCollectionPage SharedItems = await graph.GetSharedItemsAsync();
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
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
