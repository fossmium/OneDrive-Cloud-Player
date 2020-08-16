
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using Microsoft.Graph;
using OneDrive_Cloud_Player.Models.GraphData;
using OneDrive_Cloud_Player.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public sealed partial class ExplorerListViewControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Codebehind property that can be set from the xaml code
        /// </summary>
        public List<CachedDriveItem> CurrentExplorerItems
        {
            get 
            { 
                return (List<CachedDriveItem>)GetValue(ExplorerItemsDependency); 
            }
            set 
            {
                SetValueDependency(ExplorerItemsDependency, value);
            }
        }

        /// <summary>
        /// Codebehind property that can be set from the xaml code
        /// </summary>
        public CachedDriveItem CurrentSelectedExplorerItem
        {
            get 
            { 
                return (CachedDriveItem)GetValue(SelectedExplorerItemDependency); 
            }
            set 
            {
                SetValueDependency(SelectedExplorerItemDependency, value);
            }
        }

        // Using a DependencyProperty as the backing store for ExplorerItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExplorerItemsDependency =
            DependencyProperty.Register("CurrentExplorerItems", typeof(List<CachedDriveItem>), typeof(ExplorerListViewControl), null);

        public static readonly DependencyProperty SelectedExplorerItemDependency =
            DependencyProperty.Register("CurrentSelectedExplorerItem", typeof(CachedDriveItem), typeof(ExplorerListViewControl), null);
 

        public ExplorerListViewControl()
        {
            this.InitializeComponent();

            //required for using this element as a bindable datacontext
            (this.Content as FrameworkElement).DataContext = this;
        }

        public event DoubleTappedEventHandler UserControlClicked;
		public event PropertyChangedEventHandler PropertyChanged;

		private void ExplorerListBox_MouseDown(object sender, DoubleTappedRoutedEventArgs e)
        {
            UserControlClicked?.Invoke(this, e);         
        }

        /// <summary>
        /// Sets the value of the depenency and updates it to the view
        /// </summary>
        /// <param name="propertyName"></param>
		private void SetValueDependency(DependencyProperty property, object value, [CallerMemberName] String p = null)
        {
            SetValue(property, value);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
		}
    }
}
