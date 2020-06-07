using Microsoft.Graph;
using OneDrive_Cloud_Player.API;
using OneDrive_Cloud_Player.Explorer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Explorer
{
    class ExplorerView : INotifyPropertyChanged
    {

        public ICommand TestCallCommand { get; set; }

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
            TestCallCommand = new CommandHandler(ExecuteMethod, CanExecuteMethod);
        }

        private bool CanExecuteMethod(object arg)
        {
            return true;
        }

        private async void ExecuteMethod(object obj)
        {
            await TestCallAsync();
        }

        public async Task TestCallAsync()
        {
            DriveItemList = await graph.GetSharedItemsAsync();
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
