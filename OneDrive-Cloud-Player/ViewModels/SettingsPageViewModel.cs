using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OneDrive_Cloud_Player.Services;
using OneDrive_Cloud_Player.Views;
using System;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace OneDrive_Cloud_Player.ViewModels
{
    public class SettingsPageViewModel : ObservableRecipient
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
                OnPropertyChanged();
            }
        }

        private bool enableDiagnostics;

        public bool EnableDiagnostics
        {
            get
            { return enableDiagnostics; }
            set
            {
                settings.Values["EnableDiagnostics"] = value;
                enableDiagnostics = value;
                OnPropertyChanged();
            }
        }

        public SettingsPageViewModel()
        {
            //Initialize settings
            ShowDefaultSubtitles = (bool)settings.Values["ShowDefaultSubtitles"];
            EnableDiagnostics = (bool)settings.Values["EnableDiagnostics"];

            Package package = Package.Current;
            PackageVersion version = package.Id.Version;

            AppVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            PackageDisplayName = package.DisplayName;
            ToMainPageCommand = new RelayCommand(ToMainPage, CanExecuteCommand);
            DisplayWhatsNewDialogCommand = new RelayCommand(DisplayWhatsNewDialog, CanExecuteCommand);
        }

        private bool CanExecuteCommand()
        {
            return true;
        }

        public void ToMainPage()
        {
            NavigationService.Navigate<MainPage>();
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
            whatsNewDialog.Content += "* Bug fixes and stability improvements\n";

            await whatsNewDialog.ShowAsync();
        }
    }
}
