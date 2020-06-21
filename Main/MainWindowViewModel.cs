using MahApps.Metro.Controls;
using Microsoft.Graph;
using OneDrive_Cloud_Player.API;
using AuthenticationHandler = OneDrive_Cloud_Player.API.AuthenticationHandler;
using OneDrive_Cloud_Player.VLC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using OneDrive_Cloud_Player.Login;
using System.Threading.Tasks;

namespace OneDrive_Cloud_Player.Main
{
    class MainWindowViewModel : MetroWindow, INotifyPropertyChanged, IValueConverter
    {
        public ICommand GetDrivesCommand { get; set; }
        public ICommand GetSharedFolderChildrenCommand { get; set; }
        public ICommand GetChildrenFomItemCommand { get; set; }
        public ICommand GetChildrenFomDriveCommand { get; set; }
        public ICommand ReturnToParentFolderCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        public ICommand GetProfileInfoCommand { get; set; }

        private readonly GraphHandler graph;

        public event PropertyChangedEventHandler PropertyChanged;


        // The list of the different drives
        private List<DriveItem> driveList;

        public List<DriveItem> DriveList
        {
            get { return driveList; }
            set
            {
                driveList = value;
                NotifyPropertyChanged();
            }
        }

        // The list of children that is given back when you click on a parent
        private List<DriveItem> explorerItemsList;

        public List<DriveItem> ExplorerItemsList
        {
            get { return explorerItemsList; }
            set
            {
                explorerItemsList = value;
                NotifyPropertyChanged();
            }
        }

        // The folder that gets selected when you click on a onedrive
        private DriveItem selectedDriveFolder;

        public DriveItem SelectedDriveFolder
        {
            get { return selectedDriveFolder; }
            set
            {
                selectedDriveFolder = value;
                NotifyPropertyChanged();
            }
        }

        // The folder that gets selected when you click on a onedrive
        private DriveItem selectedExplorerItem;

        public DriveItem SelectedExplorerItem
        {
            get { return selectedExplorerItem; }
            set
            {
                selectedExplorerItem = value;
                NotifyPropertyChanged();
            }
        }

        // The string to display as the profile name
        private string profileName;

        public string ProfileName
        {
            get { return profileName; }
            set
            {
                profileName = value;
                NotifyPropertyChanged();
            }
        }

        // The users profile picture
        private System.IO.Stream profileImage;

        public System.IO.Stream ProfileImage
        {
            get { return profileImage; }
            set
            {
                profileImage = value;
                NotifyPropertyChanged();
            }
        }

        // The DriveItem of the parent of a folder
        private DriveItem backButtonParent;

        public DriveItem BackButtonParent
        {
            get { return backButtonParent; }
            set
            {
                backButtonParent = value;
                NotifyPropertyChanged();
            }
        }

        // The indicator showing whether the parentBackButton has been pressed
        private bool backParentIsPressed;

        // The string of the drive of a folder needed to go back to it

        private string SelectedDriveId { get; set; }

        public DriveItem PreviousSelectedCategory { get; private set; }

        public MainWindowViewModel()
        {
            driveList = null;
            this.graph = new GraphHandler();
            GetDrivesCommand = new CommandHandler(GetDrives, CanExecuteMethod);
            GetChildrenFomItemCommand = new CommandHandler(GetChildrenFomItem, CanExecuteMethod);
            GetChildrenFomDriveCommand = new CommandHandler(GetChildrenFomDrive, CanExecuteMethod);
            ReturnToParentFolderCommand = new CommandHandler(ReturnToParentFolder, CanExecuteMethod);
            LogoutCommand = new CommandHandler(Logout, CanExecuteMethod);
            GetProfileInfoCommand = new CommandHandler(GetProfileInfo, CanExecuteMethod);
            // OnLoad runs the login and gets the shared drives
            GetDrivesCommand.Execute(null);
            GetProfileInfoCommand.Execute(null);
        }

        private bool CanExecuteMethod(object arg)
        {
            return true;
        }

        private async void Logout(object obj)
        {
            AuthenticationHandler auth = new API.AuthenticationHandler();
            await auth.SignOut();

            App.Current.SwitchWindows(new LoginWindow());
        }

        //TODO: Implement that the personal drive is also added the the drivelist in the UI.
        /// <summary>
        /// Creates a list of the drives that are shared with the user.
        /// </summary>
        /// <returns></returns>
        public async void GetDrives(object obj)
        {
            //Creates local list to store the user drive and shared drives of the user.
            List<DriveItem> localDriveList = new List<DriveItem>();

            DriveItem personalDrive = await graph.GetUserRootDrive();

            localDriveList.Add(personalDrive);

            //Retrieves and stores the drives that are shared with the user.
            IDriveSharedWithMeCollectionPage sharedDrivesCollection = await graph.GetSharedDrivesAsync();


            //Adds only drives to the driveList when they are a folder. To filter out the shared files.
            foreach (DriveItem drive in sharedDrivesCollection)
            {
                if (drive.Folder != null)
                {
                    localDriveList.Add(drive);
                }
            }

            //Sets the DriveItemList property so it updates the UI.
            DriveList = localDriveList;

            Console.WriteLine(" + Loaded Drives.");
        }

        /// <summary>
        /// Retrieves the children that are inside the drive and fills the the Childrenlist property with those items.
        /// </summary>
        /// <param name="obj"></param>
        public async void GetChildrenFomDrive(object obj)
        {
            //Prevents exception when user clicks an empty space in the ListBox.
            if (SelectedDriveFolder is null) { return; };

            //Initializing.
            string sharedItemId;

            //Checks if the selected drive is a shared drive or a user drive.
            if (selectedDriveFolder.RemoteItem != null)
            {
                //Sets the SelectedDriveId field with the driveid of the selected drive.
                SelectedDriveId = selectedDriveFolder.RemoteItem.ParentReference.DriveId;

                //Sets the item id of the selectedItem variable.
                sharedItemId = selectedDriveFolder.RemoteItem.Id;
            }
            else
            {
                //Sets the SelectedDriveId field with the driveid of the selected drive.
                SelectedDriveId = selectedDriveFolder.ParentReference.DriveId;

                //Sets the sharedItemId with the id of the selected folder.
                sharedItemId = selectedDriveFolder.Id;
            }

            IDriveItemChildrenCollectionPage driveItemsCollection = await graph.GetChildrenOfItemAsync(SelectedDriveId, sharedItemId);

            //Setting the parent of the folder
            BackButtonParent = selectedDriveFolder;

            List<DriveItem> localItemList = new List<DriveItem>();

            foreach (DriveItem item in driveItemsCollection)
            {
                localItemList.Add(item);
            }
            Console.WriteLine(" + Loaded children from selected Drive.");

            //Sets the ExplorerItemsList with the items that are inside the folder. This also updates the UI.
            ExplorerItemsList = localItemList;
        }

        /// <summary>
        /// Retrieves the children that are inside an item and fills the the Childrenlist property with those items.
        /// </summary>
        /// <param name="obj"></param>
        public async void GetChildrenFomItem(object obj)
        {
            //Prevents exception when user clicks an empty space in the ListBox.
            if (SelectedExplorerItem is null) { return; };

            List<DriveItem> localDriveItemList = new List<DriveItem>();

            //Checks if the SelectedExplorerItem is an folder.
            if (SelectedExplorerItem.Folder != null)
            {
                string ItemId = SelectedExplorerItem.Id;

                IDriveItemChildrenCollectionPage driveItemsCollection = await graph.GetChildrenOfItemAsync(SelectedDriveId, ItemId);

                //Setting the parent of the folder
                BackButtonParent = selectedExplorerItem;

                //Adds every item inside the folder to the localDriveItemList. The item needs to be of type video, audio or folder.
                foreach (DriveItem item in driveItemsCollection)
                {
                    if (item.Folder != null)
                    {
                        localDriveItemList.Add(item);
                    }
                    else if (item.File != null && (item.File.MimeType.Contains("video") || item.File.MimeType.Contains("audio")))
                    {
                        localDriveItemList.Add(item);
                    }
                }
                Console.WriteLine(" + Loaded folder.");

                //Sets the ExplorerItemsList with the items that are inside the folder. This also updates the UI.
                ExplorerItemsList = localDriveItemList;

                //Calls the function to get the parent of the currently selected folder location
            }
            else
            {
                if (SelectedExplorerItem.File != null)
                {
                    OpenItemWithVideoPlayer(SelectedExplorerItem);
                }
            }
            Console.WriteLine(" + Loaded children from folder item.");
        }

        /// <summary>
        /// Opens the selected item with the videoplayer.
        /// </summary>
        private void OpenItemWithVideoPlayer(DriveItem SelectedExplorerItem)
        {
            new VideoPlayerWindow(SelectedDriveId, SelectedExplorerItem.Id);
        }

        /// <summary>
        /// Test code. Do not remove without asking @Tim Gels first.
        /// </summary>
        /// This method is created so i could test how to detect and change the personal user drive on runtime.
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Console.WriteLine("Converted called!");
            //if (value != null)
            //{
            //    return null;
            //}
            Console.WriteLine(value);
            //var color = value.ToString();


            if (value is null)
            {
                return new SolidColorBrush(Colors.Green);
            }
            else
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ReturnToParentFolder(object obj)
        {
            SelectedExplorerItem = backButtonParent;
            GetChildrenFomItem(null);
        }

        public async void GetProfileInfo(object obj)
        {
            User user = await graph.GetOneDriveUserInformationAsync();
            System.IO.Stream user2 = await graph.GetOneDriveOwnerPhotoAsync();
            ProfileName = "Hi, " + user.GivenName;
            ProfileImage = user2;
        }
    }
}