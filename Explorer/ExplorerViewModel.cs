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
    class ExplorerViewModel : MetroWindow, INotifyPropertyChanged
    {
        public ICommand GetSharedDrivesCommand { get; set; }
        public ICommand GetSharedFolderChildrenCommand { get; set; }

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

        private DriveItem sharedFolder;

        public DriveItem SharedFolder
        {
            get { return sharedFolder; }
            set
            {
                sharedFolder = value;
                NotifyPropertyChanged();
            }
        }
        
        private List<DriveItem> childrenList;

        public List<DriveItem> ChildrenList
        {
            get { return childrenList; }
            set
            {
                childrenList = value;
                NotifyPropertyChanged();
            }
        }

        public ExplorerViewModel()
        {
            driveItemList = null;
            this.graph = new GraphHandler();
            GetSharedDrivesCommand = new CommandHandler(GetSharedDrivesASyncCall, CanExecuteMethod);
            GetSharedFolderChildrenCommand = new CommandHandler(GetSharedFolderChildren, CanExecuteMethod);
            // OnLoad runs the login and gets the shared drives
            GetSharedDrivesCommand.Execute(null);
        }

        private bool CanExecuteMethod(object arg)
        {
            return true;
        }


        /// <summary>
        /// Creates a list of the users shared folders
        /// </summary>
        /// <returns></returns>
        public async void GetSharedDrivesASyncCall(object obj)
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

        /// <summary>
        /// Creates a list of the names from the selected list
        /// </summary>
        /// <param name="obj"></param>
        public async void GetSharedFolderChildren(object obj)
        {
            string SelectedDriveId = sharedFolder.RemoteItem.ParentReference.DriveId;
            string SharedItemId = sharedFolder.RemoteItem.Id;
            List<DriveItem> childrenTempList = new List<DriveItem>();
            IDriveItemChildrenCollectionPage Children = await graph.GetChildrenOfItemAsync(SharedItemId, SelectedDriveId);
            foreach (DriveItem item in Children)
            {
                childrenTempList.Add(item);
            }
            ChildrenList = childrenTempList;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
