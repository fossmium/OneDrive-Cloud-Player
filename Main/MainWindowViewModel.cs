using MahApps.Metro.Controls;
using Microsoft.Graph;
using AuthenticationHandler = OneDrive_Cloud_Player.API.AuthenticationHandler;
using OneDrive_Cloud_Player.API;
using OneDrive_Cloud_Player.Caching.GraphData;
using OneDrive_Cloud_Player.Login;
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
using System.Threading;
using System.Linq;
using System.Diagnostics;

namespace OneDrive_Cloud_Player.Main
{
    class MainWindowViewModel : MetroWindow, INotifyPropertyChanged, IValueConverter
    {
        public ICommand GetDrivesCommand { get; set; }
        public ICommand GetSharedFolderChildrenCommand { get; set; }
        public ICommand GetChildrenFomItemCommand { get; set; }
        public ICommand GetChildrenFomDriveCommand { get; set; }
        public ICommand ReloadCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

        private readonly GraphHandler graph;

        public event PropertyChangedEventHandler PropertyChanged;


        // The list of the different drives
        private List<CachedDrive> driveList;

        public List<CachedDrive> DriveList
        {
            get { return driveList; }
            set
            {
                driveList = value;
                NotifyPropertyChanged();
            }
        }

        // The list of children that is given back when you click on a parent
        private List<CachedDriveItem> explorerItemsList;

        public List<CachedDriveItem> ExplorerItemsList
        {
            get { return explorerItemsList; }
            set
            {
                explorerItemsList = value;
                NotifyPropertyChanged();
            }
        }

        // The folder that gets selected when you click on a onedrive
        private CachedDrive selectedDriveFolder;

        public CachedDrive SelectedDriveFolder
        {
            get { return selectedDriveFolder; }
            set
            {
                selectedDriveFolder = value;
                NotifyPropertyChanged();
            }
        }

        // The folder that gets selected when you click on a onedrive
        private CachedDriveItem selectedExplorerItem;

        public CachedDriveItem SelectedExplorerItem
        {
            get { return selectedExplorerItem; }
            set
            {
                selectedExplorerItem = value;
                NotifyPropertyChanged();
            }
        }
		
        private string visibilityReloadButton = "Visible";

        public string VisibilityReloadButton {
            get { return visibilityReloadButton; }
            set {
                visibilityReloadButton = value;
                NotifyPropertyChanged();
            }
        }

        private string currentUsername;

        public string CurrentUsername {
            get { return currentUsername; }
            set {
                currentUsername = value;
                NotifyPropertyChanged();
            }
        }


        private string SelectedDriveId { get; set; }

        public DriveItem PreviousSelectedCategory { get; private set; }


        public MainWindowViewModel()
        {
            DriveList = null;
            this.graph = new GraphHandler();
            GetDrivesCommand = new CommandHandler(GetDrives, CanExecuteMethod);
            GetChildrenFomItemCommand = new CommandHandler(GetChildrenFomItem, CanExecuteMethod);
            GetChildrenFomDriveCommand = new CommandHandler(GetChildrenFomDrive, CanExecuteMethod);
            ReloadCommand = new CommandHandler(ReloadCache, CanExecuteMethod);
            LogoutCommand = new CommandHandler(Logout, CanExecuteMethod);
            // OnLoad runs the login and gets the shared drives
            GetUserInformation();
        }

        public async void GetUserInformation()
        {
            CurrentUsername = (await graph.GetOneDriveUserInformationAsync()).DisplayName;
        }
		
		/// <param name="obj"></param>
		private void ReloadCache(object obj)
		{
            Console.WriteLine("Reload Cache called");
            VisibilityReloadButton = "Collapsed";
            new Thread(async () =>
            {
                await App.Current.CacheHandler.UpdateDriveCache();
                DriveList = App.Current.CacheHandler.CurrentUserCache.Drives;
                VisibilityReloadButton = "Visible";
            }).Start();
        }

        private bool CanExecuteMethod(object arg)
        {
            return true;
        }

        /// <summary>
        /// Sign the user out and clear the cache before switching to the login window
        /// </summary>
        /// <param name="obj"></param>
        private async void Logout(object obj)
        {
            AuthenticationHandler auth = new API.AuthenticationHandler();
            await auth.SignOut();
            App.Current.CacheHandler.ResetCache();
            App.Current.SwitchWindows(new LoginWindow());
        }

        /// <summary>
        /// Creates a list of the drives that are shared with the user.
        /// </summary>
        /// <returns></returns>
        public async void GetDrives(object obj)
        {
            StackTrace stackTrace = new StackTrace();
            // Get calling method name
            Console.WriteLine(stackTrace.GetFrame(1).GetMethod().Name);
            var yeet = stackTrace.GetFrame(1).GetMethod();
            Console.WriteLine(new StackFrame(1).GetMethod().Name);
            //Creates local list to store the user drive and shared drives of the user.
            List<CachedDrive> localDriveList = await App.Current.CacheHandler.GetDrives();
            //List<DriveItem> localDriveList = new List<DriveItem>();

            //DriveItem personalDrive = await graph.GetUserRootDrive();

            //localDriveList.Add(personalDrive);

            //Retrieves and stores the drives that are shared with the user.
            //IDriveSharedWithMeCollectionPage sharedDrivesCollection = await graph.GetSharedDrivesAsync();


            //Adds only drives to the driveList when they are a folder. To filter out the shared files.
            //foreach (DriveItem drive in sharedDrivesCollection)
            //{
            //	if (drive.Folder != null)
            //	{
            //		localDriveList.Add(drive);
            //	}
            //}

            //Sets the DriveItemList property so it updates the UI.
            DriveList = localDriveList;
        }



        /// <summary>
        /// Retrieves the children that are inside the drive and fills the the Childrenlist property with those items.
        /// </summary>
        /// <param name="obj"></param>
        public async void GetChildrenFomDrive(object obj)
        {
            //Prevents exception when user clicks an empty space in the ListBox.
            if (SelectedDriveFolder is null) { return; };

            //Sets the SelectedDriveId field with the driveid of the selected drive.
            SelectedDriveId = SelectedDriveFolder.DriveId;

            //Sets the item id of the selectedItem variable.
            string itemId = SelectedDriveFolder.Id;

            //IDriveItemChildrenCollectionPage driveItemsCollection = await graph.GetChildrenOfItemAsync(SelectedDriveId, itemId);
            List<CachedDriveItem> driveItems = await App.Current.CacheHandler.GetCachedChildrenFromDrive(SelectedDriveId, itemId);

			if (driveItems is null)
            {
                // Show dialog and return
                MessageBox.Show("An error has occured while entering this shared folder. Please try again later.");
                return;
            }

            //List<DriveItem> localItemList = new List<DriveItem>();

            //foreach (DriveItem item in driveItemsCollection)
            //{
            //    localItemList.Add(item);
            //}
            Console.WriteLine(" + Loaded children from selected Drive.");

            //Sets the ExplorerItemsList with the items that are inside the folder. This also updates the UI.
            //ExplorerItemsList = localItemList;
            ExplorerItemsList = driveItems;
        }

        /// <summary>
        /// Retrieves the children that are inside an item and fills the the Childrenlist property with those items.
        /// </summary>
        /// <param name="obj"></param>
        public async void GetChildrenFomItem(object obj)
        {
            //Prevents exception when user clicks an empty space in the ListBox.
            if (SelectedExplorerItem is null) { return; };

            List<CachedDriveItem> localDriveItemList = new List<CachedDriveItem>();

            //Checks if the SelectedExplorerItem is an folder.
            if (SelectedExplorerItem.IsFolder)
            {
                string ItemId = SelectedExplorerItem.ItemId;
                //IDriveItemChildrenCollectionPage driveItemsCollection = await graph.GetChildrenOfItemAsync(SelectedDriveId, ItemId);
                List<CachedDriveItem> driveItems = await App.Current.CacheHandler.GetCachedChildrenFromItem(SelectedDriveFolder, ItemId);

                //Adds every item inside the folder to the localDriveItemList. The item needs to be of type video, audio or folder.
                foreach (CachedDriveItem item in driveItems)
                {
                    if (item.IsFolder)
                    {
                        localDriveItemList.Add(item);
                    }
                    else if (item.MimeType.Contains("video") || item.MimeType.Contains("audio"))
                    {
                        localDriveItemList.Add(item);
                    }
                }
                Console.WriteLine(" + Loaded folder.");

                //Sets the ExplorerItemsList with the items that are inside the folder. This also updates the UI.
                ExplorerItemsList = localDriveItemList;
            }
            else
            {
                OpenItemWithVideoPlayer(SelectedExplorerItem);
            }
            Console.WriteLine(" + Loaded children from folder item.");
        }

        /// <summary>
        /// Opens the selected item with the videoplayer.
        /// </summary>
        private void OpenItemWithVideoPlayer(CachedDriveItem SelectedExplorerItem)
        {
            new VideoPlayerWindow(SelectedDriveFolder.DriveId, SelectedExplorerItem.ItemId);
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
            //Console.WriteLine("Converted called!");
            //if (value != null)
            //{
            //    return null;
            //}
            //Console.WriteLine(value);
            //var color = value.ToString();


            if (!(bool)value)
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

    }
}
