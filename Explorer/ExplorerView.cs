using MahApps.Metro.Controls;
using Microsoft.Graph;
using OneDrive_Cloud_Player;
using OneDrive_Cloud_Player.API;
using OneDrive_Cloud_Player.Explorer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Explorer
{
    class ExplorerView : MetroWindow, INotifyPropertyChanged
    {

        public ICommand GetSharedDrivesCommand { get; set; }

        private GraphHandler graph;

        public event PropertyChangedEventHandler PropertyChanged;

        private IDriveSharedWithMeCollectionPage driveItemList;

        public IDriveSharedWithMeCollectionPage DriveItemList
        {
            get { return driveItemList; }
            set
            {
                driveItemList = value;
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

        public async Task GetSharedDrivesASyncCall()
        {
            
            DriveItemList = await graph.GetSharedItemsAsync();
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
