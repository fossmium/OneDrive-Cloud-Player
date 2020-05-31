using OneDrive_Cloud_Player.API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Explorer
{
    class ExplorerView : MainWindow, INotifyPropertyChanged
    {

        private Graph graph;
        List<Show> showList { get; set; }
        
        public ExplorerView()
        {
            this.graph = new Graph();
            graph.GetDriveInformationAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
