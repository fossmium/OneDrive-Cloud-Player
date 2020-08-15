
using CommonServiceLocator;
using Microsoft.Graph;
using OneDrive_Cloud_Player.Models.GraphData;
using OneDrive_Cloud_Player.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OneDrive_Cloud_Player.Views.UserControls
{
    public sealed partial class ExplorerListViewControl : UserControl
    {

        public List<CachedDriveItem> ExplorerItemsList
        {
            get { return (List<CachedDriveItem>)GetValue(ExplorerItemsListProperty); }
            set { SetValue(ExplorerItemsListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExplorerItemsList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExplorerItemsListProperty =
            DependencyProperty.Register("ExplorerItemsList", typeof(List<CachedDriveItem>), typeof(ExplorerListViewControl), new PropertyMetadata(0));

        public ExplorerListViewControl()
        {
            this.InitializeComponent();
        }


        public event EventHandler UserControlClicked;

        private void ExplorerListBox_MouseDown(object sender, DoubleTappedRoutedEventArgs e)
        {
            UserControlClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
