using MahApps.Metro.Controls;
using Microsoft.Graph;
using OneDrive_Cloud_Player.API;
using OneDrive_Cloud_Player.Explorer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Explorer
{
    class ExplorerView : MetroWindow, INotifyPropertyChanged
    {
        public ICommand GetSharedDrivesCommand { get; set; }

        private GraphHandler graph;

        public event PropertyChangedEventHandler PropertyChanged;

        private List<DriveItem> driveItemList;

        public List<DriveItem> DriveItemList
        {
            get { return driveItemList; }
            set
            {
                driveItemList = value;
                NotifyPropertyChanged();
            }
        }

        private List<DriveItem> folderList;

        public List<DriveItem> FolderList
        {
            get { return folderList; }
            set
            {
                folderList = value;
                NotifyPropertyChanged();
            }
        }

        public ExplorerView()
        {
            driveItemList = null;
            this.graph = new GraphHandler();
            GetSharedDrivesCommand = new CommandHandler(ExecuteMethod, CanExecuteMethod);
            // OnLoad runs the login and gets the shared drives
            GetSharedDrivesCommand.Execute(null);
        }

        private bool CanExecuteMethod(object arg)
        {
            return true;
        }

        private async void ExecuteMethod(object obj)
        {
            await GetSharedDrivesASyncCall();
        }

        /// <summary>
        /// Creates a list of the users shared folders
        /// </summary>
        /// <returns></returns>
        public async Task GetSharedDrivesASyncCall()
        {
            IDriveSharedWithMeCollectionPage driveItemsTemp = await graph.GetSharedItemsAsync();
            List<DriveItem> driveItemList = new List<DriveItem>();
            foreach (DriveItem item in driveItemsTemp)
            {
                if (item.Folder != null)
                {
                    driveItemList.Add(item);
                }
            }
            DriveItemList = driveItemList;
        }

        public async Task GetFolderItems()
        {
            
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
