using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace OneDrive_Cloud_Player.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public ICommand ToMainPageCommand { get; set; }
        public ICommand DisplayWhatsNewDialogCommand { get; set; }
        public string AppVersion { get; }
        public string PackageDisplayName { get; }

        private readonly ApplicationDataContainer settings = App.Current.UserSettings;

        private bool showDefaultSubtitles;

        public bool ShowDefaultSubtitles
        {
            get
            { return showDefaultSubtitles; }
            set
            {
                settings.Values["ShowDefaultSubtitles"] = value;
                showDefaultSubtitles = value;
                RaisePropertyChanged("ShowDefaultSubtitles");
            }
        }


        private readonly INavigationService _navigationService;


        public SettingsPageViewModel(INavigationService navigationService)
        {
            //Initialize settings
            ShowDefaultSubtitles = (bool)settings.Values["ShowDefaultSubtitles"];

            Package package = Package.Current;
            PackageVersion version = package.Id.Version;

            AppVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            PackageDisplayName = package.DisplayName;
            _navigationService = navigationService;
            ToMainPageCommand = new RelayCommand(ToMainPage, CanExecuteCommand);
            DisplayWhatsNewDialogCommand = new RelayCommand(DisplayWhatsNewDialog, CanExecuteCommand);
        }

        private bool CanExecuteCommand()
        {
            return true;
        }

        public void ToMainPage()
        {
            _navigationService.NavigateTo("MainPage");
        }

        /// <summary>
        /// Displays a content dialog showing the new features added in the current version.
        /// </summary>
        private async void DisplayWhatsNewDialog()
        {
            ContentDialog whatsNewDialog = new ContentDialog
            {
                Title = $"What's new in {PackageDisplayName} {AppVersion}",
                PrimaryButtonText = "Ok",
                DefaultButton = ContentDialogButton.Primary,
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 49)),
            };
            whatsNewDialog.Content += "* Fixed an issue where sometimes the audio would be heard playing when \n   it was expected to be quiet\n";
            whatsNewDialog.Content += "* Updated several libraries and dependencies to the latest version\n";

            await whatsNewDialog.ShowAsync();
        }
    }
}
