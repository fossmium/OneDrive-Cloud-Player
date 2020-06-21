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
using OneDrive_Cloud_Player.Login;
using System.Threading.Tasks;

namespace OneDrive_Cloud_Player.Main
{
    class MainWindowViewModel : MetroWindow, INotifyPropertyChanged,  IValueConverter
    {
        public ICommand GetDrivesCommand { get; set; }
        public ICommand GetSharedFolderChildrenCommand { get; set; }
        public ICommand GetChildrenFomItemCommand { get; set; }
        public ICommand GetChildrenFomDriveCommand { get; set; }
        public ICommand ReloadCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        public ICommand ToParentFolderCommand { get; set; }
        public ICommand GetProfileInfoCommand { get; set; }

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

        public string VisibilityReloadButton
        {
            get { return visibilityReloadButton; }
            set
            {
                visibilityReloadButton = value;
                NotifyPropertyChanged();
            }
        }

        private string currentUsername;

        public string CurrentUsername
        {
            get { return currentUsername; }
            set
            {
                currentUsername = value;
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

        private string SelectedDriveId { get; set; }

        private CachedDriveItem parentItem;

        public CachedDriveItem ParentItem
        {
            get { return parentItem; }
            set
            {
                parentItem = value;
                NotifyPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            DriveList = null;
            this.graph = new GraphHandler();
            GetDrivesCommand = new CommandHandler(GetDrives, CanExecuteMethod);
            GetChildrenFomItemCommand = new CommandHandler(GetChildrenFomItem, CanExecuteMethod);
            GetChildrenFomDriveCommand = new CommandHandler(GetChildrenFomDrive, CanExecuteMethod);
            ReloadCommand = new CommandHandler(ReloadCache, CanExecuteMethod);
            LogoutCommand = new CommandHandler(Logout, CanExecuteMethod);
            ToParentFolderCommand = new CommandHandler(ToParentFolder, CanExecuteMethod);
            //GetProfileInfoCommand = new CommandHandler(GetProfileInfo, CanExecuteMethod);
            //GetProfileInfoCommand.Execute(null);
            // OnLoad runs the login and gets the shared drives
            GetUserInformation();
        }

        public async void GetUserInformation()
        {
            CurrentUsername = "Hi, " + (await graph.GetOneDriveUserInformationAsync()).GivenName;
            ProfileImage = await graph.GetOneDriveOwnerPhotoAsync();
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
                // reset the current item list so we don't get an exception
                ExplorerItemsList = null;
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
            //Creates local list to store the user drive and shared drives of the user.
            List<CachedDrive> localDriveList = await App.Current.CacheHandler.GetDrives();

            //Sets the DriveItemList property so it updates the UI.
            DriveList = localDriveList;
        }

        /// <summary>
        /// Retrieves the children that are inside the drive and fills the the Childrenlist property with those items.
        /// </summary>
        /// <param name="obj"></param>
        public async void GetChildrenFomDrive(object obj)
        {
            //
            ParentItem = null;

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
            if (SelectedExplorerItem is null) { return; }

            if (SelectedDriveFolder is null) { return; }

            //Sets the current selected item as a parent item.
            ParentItem = SelectedExplorerItem;

            //Checks if the SelectedExplorerItem is an folder.
            if (SelectedExplorerItem.IsFolder)
            {
                string ItemId = SelectedExplorerItem.ItemId;
                //IDriveItemChildrenCollectionPage driveItemsCollection = await graph.GetChildrenOfItemAsync(SelectedDriveId, ItemId);
                List<CachedDriveItem> driveItems = await App.Current.CacheHandler.GetCachedChildrenFromItem(SelectedDriveFolder, ItemId);

                Console.WriteLine(" + Loaded folder.");

                //Sets the ExplorerItemsList with the items that are inside the folder. This also updates the UI.
                ExplorerItemsList = driveItems;
            }
            else
            {
                App.Current.MainWindow.Hide();
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

        private void ToParentFolder(object obj)
        {
            if (SelectedDriveFolder is null) { return; }

            if (ParentItem is null) { return; }

            //App.Current.CacheHandler.GetCachedChildrenFromDrive
            //App.Current.CacheHandler.GetCachedChildrenFromItem

            
            string id = ParentItem.ParentItemId;
            List<CachedDriveItem> ParentItemList = App.Current.CacheHandler.GetDriveOrItemsWithParentId(SelectedDriveFolder, id);
            ExplorerItemsList = ParentItemList;

            if (App.Current.CacheHandler.IsParentChildOfDrive(SelectedDriveFolder, id))
            {
                ParentItem = null;
            }
            else
            {
                // Every time we go up a folder, we need to set the ParentItem to one higher
                ParentItem = App.Current.CacheHandler.GetParentItemByParentItemId(SelectedDriveFolder, ParentItem.ParentItemId);
            }
        }


        /// <summary>
        /// Changes XAML Elements on runtime.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Changes the ItemContentType icon of the items in the explorer.
            if (parameter.Equals("ContentTypeExplorerItem"))
            {
                if ((bool)value)
                {
                    return "/Assets/Icons/folder.png";
                }
                else
                {
                    return "/Assets/Icons/MultiMediaIcon.png";
                }
            }

            //Returns the item child when it's a folder. Otherwise return a line.
            if (parameter.Equals("ContentChildCountExplorer"))
            {
                CachedDriveItem item = (CachedDriveItem)value;
                if (item.IsFolder)
                {
                    return item.ChildCount;
                }
                return "-";
            }

            //Returns the correct size format.
            if (parameter.Equals("ContentItemSizeExplorer"))
            {
                Console.WriteLine(value);
                long size = (long)value;
                if (size > 1000 && size <1000000000)
                {

                    return Math.Round((size / (double)Math.Pow(1024, 2))) + " MB";
                }
                else if(size > 1000000000)
                {
                    return Decimal.Round((Decimal)(size / (double)Math.Pow(1024, 3)), 2) + " GB";
                }
                else
                {
                    return size + " Bytes";
                }
            }
            return null;
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