using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Graph;
using OneDrive_Cloud_Player.Models.GraphData;
using OneDrive_Cloud_Player.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Input;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace OneDrive_Cloud_Player.ViewModels
{
    public class MainPageViewModel : ViewModelBase
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
        private readonly INavigationService _navigationService;

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
                //Retrieve the items of the drive
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

        private bool isReloadButtonEnabled = true;

        public bool IsReloadButtonEnabled
        {
            get { return isReloadButtonEnabled; }
            set
            {
                isReloadButtonEnabled = value;
                RaisePropertyChanged("IsReloadButtonEnabled");
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
        private BitmapImage profileImage;

        public BitmapImage ProfileImage
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

        public MainPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            DriveList = null;
            this.graph = new GraphHelper();
            //DisplayMessageCommand = new RelayCommand(DisplayMessage, CanExecuteCommand);
            GetDrivesCommand = new RelayCommand(GetDrives, CanExecuteCommand);
            GetChildrenFomItemCommand = new RelayCommand(GetChildrenFomItem, CanExecuteCommand);
            GetChildrenFomDriveCommand = new RelayCommand(GetChildrenFomDrive, CanExecuteCommand);
            ReloadCommand = new RelayCommand(ReloadCache, CanExecuteCommand);
            LogoutCommand = new RelayCommand(Logout, CanExecuteCommand);
            ToParentFolderCommand = new RelayCommand(ToParentFolder, CanExecuteCommand);

            // OnLoad runs the login and gets the shared drives
            GetUserInformation();

            // Retrieve the drives so the View has them on load
            GetDrives();
        }

        /// <summary>
        /// Retrieves the user information to display the user's name and his profile picture.
        /// </summary>
        public async void GetUserInformation()
        {
            CurrentUsername = "Hi, " + (await graph.GetOneDriveUserInformationAsync()).GivenName;
            try
            {
                var bitmapImage = new BitmapImage();
                IRandomAccessStream imageRandomAccessStream = (await graph.GetOneDriveOwnerPhotoAsync()).AsRandomAccessStream();
                await bitmapImage.SetSourceAsync(imageRandomAccessStream);

                ProfileImage = bitmapImage;
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
            CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            Console.WriteLine(" + Reloading drives.");
            IsReloadButtonEnabled = false;
            new Thread(async () =>
            {
                await App.Current.CacheHelper.UpdateDriveCache();

                //A call to the UI thread is needed because of the properties that are being set.
                await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    DriveList = App.Current.CacheHelper.CurrentUserCache.Drives;
                    IsReloadButtonEnabled = true;
                    // reset the current item list so we don't get an exception
                    ExplorerItemsList = null;
                });
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
            // Unregister this view model so when a person logs in as a new user it has reset the view model.
            ViewModelLocator.ResetMainPageViewModel();

            _navigationService.NavigateTo("LoginPage");
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

            Debug.WriteLine(" + Retrieved drives.");
        }

        /// <summary>
        /// Retrieves the children that are inside the drive and fills the the Childrenlist property with those items.
        /// </summary>
        /// <param name="obj"></param>
        public async void GetChildrenFomDrive()
        {
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
            Debug.WriteLine(" + Loaded children from selected Drive.");

            //Sets the ExplorerItemsList with the items that are inside the folder. This also updates the UI.
            ExplorerItemsList = driveItems;
        }

        /// <summary>
        /// Retrieves the children that are inside an item and fills the the Childrenlist property with those items.
        /// </summary>
        /// <param name="obj"></param>
        public async void GetChildrenFomItem()
        {
            Debug.WriteLine("Called GetChildrenFomItem() method!");
            //Prevents exception when user clicks an empty space in the ListBox.
            if (SelectedExplorerItem is null) { return; }

            if (SelectedDriveFolder is null) { return; }

            //Checks if the SelectedExplorerItem is an folder.
            if (SelectedExplorerItem.IsFolder)
            {
                //Sets the current selected item as a parent item.
                ParentItem = SelectedExplorerItem;

                string ItemId = SelectedExplorerItem.ItemId;
                //IDriveItemChildrenCollectionPage driveItemsCollection = await graph.GetChildrenOfItemAsync(SelectedDriveId, ItemId);
                List<CachedDriveItem> driveItems = await App.Current.CacheHelper.GetCachedChildrenFromItem(SelectedDriveFolder, ItemId);

                Debug.WriteLine(" + Loaded folder.");

                //Sets the ExplorerItemsList with the items that are inside the folder. This also updates the UI.
                ExplorerItemsList = driveItems;
            }
            else
            {
                OpenItemWithVideoPlayer(SelectedExplorerItem);
            }
            Debug.WriteLine(" + Loaded children from folder item.");
        }

        /// <summary>
        /// Opens the selected item with the videoplayer.
        /// </summary>
        private void OpenItemWithVideoPlayer(CachedDriveItem SelectedExplorerItem)
        {
            // Navigate to the VideoPlayerPage
            _navigationService.NavigateTo("VideoPlayerPage", SelectedExplorerItem);
        }

        /// <summary>
        /// Update the explorer listview met items that resides in the parent folder.
        /// </summary>
        private void ToParentFolder()
        {
            if (SelectedDriveFolder is null) { return; }

            if (ParentItem is null) { return; }

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


    }
}
