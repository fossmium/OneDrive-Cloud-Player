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
        public ICommand GetSharedFolderChildrenTestCommand { get; set; }

        private GraphHandler graph;

        public event PropertyChangedEventHandler PropertyChanged;


        // The list of the different drives
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

        // The list of children that is given back when you click on a parent
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

        // The folder that gets selected when you click on a onedrive
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

        // The folder that gets selected when you click on a onedrive
        private DriveItem folderChild;

        public DriveItem FolderChild
        {
            get { return folderChild; }
            set
            {
                folderChild = value;
               

                //if (folderChild == null)
                //{
                //    return;
                //}

                //if (value != null)
                //{
                //    PreviousSelectedCategory = value;
                //}

                //folderChild = value;
                //GetSharedFolderChildren(folderChild);
                NotifyPropertyChanged();
            }
        }

        public DriveItem PreviousSelectedCategory { get; private set; }

        public ExplorerViewModel()
        {
            driveItemList = null;
            this.graph = new GraphHandler();
            GetSharedDrivesCommand = new CommandHandler(GetSharedDrivesASyncCall, CanExecuteMethod);
            GetSharedFolderChildrenCommand = new CommandHandler(GetSharedFolderChildren, CanExecuteMethod);
            GetSharedFolderChildrenTestCommand = new CommandHandler(GetSharedFolderChildrenTest, CanExecuteMethod);
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
            Console.WriteLine("drive get complete");
        }

        /// <summary>
        /// Creates a list of the names from the selected list
        /// </summary>
        /// <param name="obj"></param>
        public async void GetSharedFolderChildrenTest(object obj)
        {
            List<DriveItem> childrenTempList = new List<DriveItem>();
            if (folderChild != null)
            {
                string SelectedFolderId = folderChild.ParentReference.DriveId;
                string SharedFolderId = folderChild.Id;
                IDriveItemChildrenCollectionPage Children = await graph.GetChildrenOfItemAsync(SharedFolderId, SelectedFolderId);
                foreach (DriveItem item in Children)
                {
                    childrenTempList.Add(item);
                }
                Console.WriteLine("folder loaded");
                ChildrenList = childrenTempList;
            }
            Console.WriteLine("uPDATED folderchild");
        }

            /// <summary>
            /// Creates a list of the names from the selected list
            /// </summary>
            /// <param name="obj"></param>
            public async void GetSharedFolderChildren(object obj)
        {
            List<DriveItem> childrenTempList = new List<DriveItem>();
            if (folderChild != null)
            {
                string SelectedFolderId = folderChild.ParentReference.DriveId;
                string SharedFolderId = folderChild.Id;
                IDriveItemChildrenCollectionPage Children = await graph.GetChildrenOfItemAsync(SharedFolderId, SelectedFolderId);
                foreach (DriveItem item in Children)
                {
                    childrenTempList.Add(item);
                }
                Console.WriteLine("folder loaded");
            }
            else
            {
                try
                {
                    string SelectedDriveId = sharedFolder.RemoteItem.ParentReference.DriveId;
                    string SharedItemId = sharedFolder.RemoteItem.Id;
                    IDriveItemChildrenCollectionPage Children = await graph.GetChildrenOfItemAsync(SharedItemId, SelectedDriveId);
                    foreach (DriveItem item in Children)
                    {
                        childrenTempList.Add(item);
                    }
                    Console.WriteLine("empty loaded.");
                }
                catch (Exception)
                {


                }

            }
            ChildrenList = childrenTempList;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
