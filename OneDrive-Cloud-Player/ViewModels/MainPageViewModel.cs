using OneDrive_Cloud_Player.Models.GraphData;
using OneDrive_Cloud_Player.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Threading;
using Microsoft.Graph;
using Windows.UI.Xaml.Data;
using System.Globalization;
using System.Diagnostics;

namespace OneDrive_Cloud_Player.ViewModels
{
    public class MainPageViewModel : ViewModelBase, IValueConverter
    {
        public ICommand GetDrivesCommand { get; set; }
        public ICommand GetSharedFolderChildrenCommand { get; set; }
        public ICommand GetChildrenFomItemCommand { get; set; }
        public ICommand GetChildrenFomDriveCommand { get; set; }
        public ICommand ReloadCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        public ICommand ToParentFolderCommand { get; set; }
        public ICommand GetProfileInfoCommand { get; set; }

        private readonly GraphHelper graph;


        // The list of the different drives
        private List<CachedDrive> driveList;

        public List<CachedDrive> DriveList
        {
            get { return driveList; }
            set
            {
                driveList = value;
                RaisePropertyChanged("DriveList");
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
                RaisePropertyChanged("ExplorerItemsList");
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
                GetChildrenFomDrive();
                RaisePropertyChanged("SelectedDriveFolder");
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
                RaisePropertyChanged("SelectedExplorerItem");
            }
        }

        private string visibilityReloadButton = "Visible";

        public string VisibilityReloadButton
        {
            get { return visibilityReloadButton; }
            set
            {
                visibilityReloadButton = value;
                RaisePropertyChanged("VisibilityReloadButton");
            }
        }

        private string currentUsername;

        public string CurrentUsername
        {
            get { return currentUsername; }
            set
            {
                currentUsername = value;
                RaisePropertyChanged("CurrentUsername");
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
                RaisePropertyChanged("ProfileImage");
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
                RaisePropertyChanged("ParentItem");
            }
        }

        public MainPageViewModel()
        {
            DriveList = null;
            this.graph = new GraphHelper();
            //DisplayMessageCommand = new RelayCommand(DisplayMessage, CanExecuteCommand);
            GetDrivesCommand = new RelayCommand(GetDrives, CanExecuteCommand);
            GetChildrenFomItemCommand = new RelayCommand(GetChildrenFomItem, CanExecuteCommand);
            GetChildrenFomDriveCommand = new RelayCommand(GetChildrenFomDrive, CanExecuteCommand);
            ReloadCommand = new RelayCommand(ReloadCache, CanExecuteCommand);
            LogoutCommand = new RelayCommand(Logout, CanExecuteCommand);
            ToParentFolderCommand = new RelayCommand(ToParentFolder, CanExecuteCommand);
            //GetProfileInfoCommand = new CommandHandler(GetProfileInfo, CanExecuteMethod);
            //GetProfileInfoCommand.Execute(null);
            // OnLoad runs the login and gets the shared drives
            GetUserInformation();
        }

        /// <summary>
        /// Retrieves the user information to display the user's name and his profile picture.
        /// </summary>
        public async void GetUserInformation()
        {
            CurrentUsername = "Hi, " + (await graph.GetOneDriveUserInformationAsync()).GivenName;
            try
            {
                ProfileImage = await graph.GetOneDriveOwnerPhotoAsync();
            }
            catch (ServiceException)
            {
                // A user may not have a picture.
            }
        }

        /// <summary>
        /// Reloads the cache of the drives and removes the item explorer view so the user needs to select a drive again to access explorer items.
        /// </summary>
        /// <param name="obj"></param>
        private void ReloadCache()
        {
            Console.WriteLine("Reload Cache called");
            VisibilityReloadButton = "Collapsed";
            new Thread(async () =>
            {
                await App.Current.CacheHelper.UpdateDriveCache();
                DriveList = App.Current.CacheHelper.CurrentUserCache.Drives;
                VisibilityReloadButton = "Visible";
                // reset the current item list so we don't get an exception
                ExplorerItemsList = null;
            }).Start();
        }

        private bool CanExecuteCommand()
        {
            return true;
        }

        /// <summary>
        /// Sign the user out and clear the cache before switching to the login window
        /// </summary>
        /// <param name="obj"></param>
        private async void Logout()
        {
            GraphAuthHelper auth = new GraphAuthHelper();
            await auth.SignOut();
            App.Current.CacheHelper.ResetCache();
            //App.Current.SwitchWindows(new LoginWindow());
        }

        /// <summary>
        /// Creates a list of the drives that are shared with the user.
        /// </summary>
        /// <returns></returns>
        public async void GetDrives()
        {
            //Creates local list to store the user drive and shared drives of the user.
            List<CachedDrive> localDriveList = await App.Current.CacheHelper.GetDrives();

            //Sets the DriveItemList property so it updates the UI.
            DriveList = localDriveList;
        }

        /// <summary>
        /// Retrieves the children that are inside the drive and fills the the Childrenlist property with those items.
        /// </summary>
        /// <param name="obj"></param>
        public async void GetChildrenFomDrive()
        {
            Debug.WriteLine("Called GetChildrenFomDriveCommand method");
            ParentItem = null;

            //Prevents exception when user clicks an empty space in the ListBox.
            if (SelectedDriveFolder is null) { return; };

            //Sets the SelectedDriveId field with the driveid of the selected drive.
            SelectedDriveId = SelectedDriveFolder.DriveId;

            //Sets the item id of the selectedItem variable.
            string itemId = SelectedDriveFolder.Id;

            //IDriveItemChildrenCollectionPage driveItemsCollection = await graph.GetChildrenOfItemAsync(SelectedDriveId, itemId);
            List<CachedDriveItem> driveItems = await App.Current.CacheHelper.GetCachedChildrenFromDrive(SelectedDriveId, itemId);

            if (driveItems is null)
            {
                // Show dialog and return
                //MessageBox.Show("An error has occured while entering this shared folder. Please try again later.");
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
        public async void GetChildrenFomItem()
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
                List<CachedDriveItem> driveItems = await App.Current.CacheHelper.GetCachedChildrenFromItem(SelectedDriveFolder, ItemId);

                Console.WriteLine(" + Loaded folder.");

                //Sets the ExplorerItemsList with the items that are inside the folder. This also updates the UI.
                ExplorerItemsList = driveItems;
            }
            else
            {
                //App.Current.MainWindow.Hide();
                //OpenItemWithVideoPlayer(SelectedExplorerItem);
            }
            Console.WriteLine(" + Loaded children from folder item.");
        }

        /// <summary>
        /// Opens the selected item with the videoplayer.
        /// </summary>
        private void OpenItemWithVideoPlayer(CachedDriveItem SelectedExplorerItem)
        {
            //new VideoPlayerWindow(SelectedDriveFolder.DriveId, SelectedExplorerItem.ItemId);
        }

        private void ToParentFolder()
        {
            if (SelectedDriveFolder is null) { return; }

            if (ParentItem is null) { return; }

            //App.Current.CacheHandler.GetCachedChildrenFromDrive
            //App.Current.CacheHandler.GetCachedChildrenFromItem


            string id = ParentItem.ParentItemId;
            List<CachedDriveItem> ParentItemList = App.Current.CacheHelper.GetDriveOrItemsWithParentId(SelectedDriveFolder, id);
            ExplorerItemsList = ParentItemList;

            if (App.Current.CacheHelper.IsParentChildOfDrive(SelectedDriveFolder, id))
            {
                ParentItem = null;
            }
            else
            {
                // Every time we go up a folder, we need to set the ParentItem to one higher
                ParentItem = App.Current.CacheHelper.GetParentItemByParentItemId(SelectedDriveFolder, ParentItem.ParentItemId);
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
        public object Convert(object value, Type targetType, object parameter, string language)
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
                if (size > 1000 && size < 1000000000)
                {

                    return Math.Round((size / (double)Math.Pow(1024, 2))) + " MB";
                }
                else if (size > 1000000000)
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
