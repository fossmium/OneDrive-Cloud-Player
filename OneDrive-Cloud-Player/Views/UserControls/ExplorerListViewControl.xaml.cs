
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
        public enum ViewType
        {
            Grid = 1000,
            List = 2000
        }

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

        public ViewType ControlViewType
        {
            get
            {
                return (ViewType)GetValue(ControlViewTypeDependency);
            }
            set
            {
                SetValueDependency(ControlViewTypeDependency, value);
                ViewTypeChanged(); //trigger an update with custom code
            }
        }


        /// <summary>
        /// Dependency Property for the ExplorerItems, this allows binding to it from a parent class
        /// </summary>
        public static readonly DependencyProperty ExplorerItemsDependency =
            DependencyProperty.Register("CurrentExplorerItems", typeof(List<CachedDriveItem>), typeof(ExplorerListViewControl), null);

        /// <summary>
        /// Dependency Property for the SelectedExplorerItem, this allows binding to it from a parent class.
        /// </summary>
        public static readonly DependencyProperty SelectedExplorerItemDependency =
            DependencyProperty.Register("CurrentSelectedExplorerItem", typeof(CachedDriveItem), typeof(ExplorerListViewControl), null);

        /// <summary>
        /// Dependency Property for the SelectedExplorerItem, this allows binding to it from a parent class.
        /// </summary>
        public static readonly DependencyProperty ControlViewTypeDependency =
            DependencyProperty.Register("ControlViewType", typeof(CachedDriveItem), typeof(ExplorerListViewControl), null);


        public event DoubleTappedEventHandler UserControlClicked;
        public event PropertyChangedEventHandler PropertyChanged;

        public ExplorerListViewControl()
        {
            this.InitializeComponent();

            //required for using this element as a bindable datacontext
            (this.Content as FrameworkElement).DataContext = this;
        }

        /// <summary>
        /// The event used for double clicking a list item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Handles the viewtype changed event, this function also fires the on the first load, as mvvm is slow and needs time to load its dependency values :)
        /// </summary>
        private void ViewTypeChanged()
        {
            Console.WriteLine("MainPage display type: " + ControlViewType);

            switch (ControlViewType)
            {
                case ViewType.Grid:
                    //enable the gridview
                    EnableGridView();

                    //disable the listview (even if this was never enabled)
                    DisableListView();
                    break;
                case ViewType.List:
                    //enable the listview
                    EnableListView();

                    //disable the gridview (even if this was never enabled)
                    DisableGridView();
                    break;
            }
        }

        private void EnableListView()
        {
            ExplorerListBox.Visibility = Visibility.Visible;

            //create a selectedItem binding for the listbox
            Binding selectedItemBinding = new Binding();
            selectedItemBinding.Path = new PropertyPath("CurrentSelectedExplorerItem");
            selectedItemBinding.Mode = BindingMode.TwoWay;
            ExplorerListBox.SetBinding(Selector.SelectedItemProperty, selectedItemBinding);

            //create a listsource binding for the listbox
            Binding listSource = new Binding();
            listSource.Path = new PropertyPath("CurrentExplorerItems");
            ExplorerListBox.SetBinding(ItemsControl.ItemsSourceProperty, listSource);
        }

        private void DisableListView()
        {
            ExplorerListBox.Visibility = Visibility.Collapsed;
            ExplorerListBox.ClearValue(SelectedExplorerItemDependency); //reset possible bindings
            ExplorerListBox.ClearValue(ExplorerItemsDependency); //reset possible bindings
        }

        private void EnableGridView()
        {
            ExplorerGridBox.Visibility = Visibility.Visible;

            //create a selectedItem binding for the listbox
            Binding selectedItemBinding = new Binding();
            selectedItemBinding.Path = new PropertyPath("CurrentSelectedExplorerItem");
            selectedItemBinding.Mode = BindingMode.TwoWay;
            ExplorerGridBox.SetBinding(Selector.SelectedItemProperty, selectedItemBinding);

            //create a listsource binding for the listbox
            Binding listSource = new Binding();
            listSource.Path = new PropertyPath("CurrentExplorerItems");
            ExplorerGridBox.SetBinding(ItemsControl.ItemsSourceProperty, listSource);
        }

        private void DisableGridView()
        {
            ExplorerGridBox.Visibility = Visibility.Collapsed;
            ExplorerGridBox.ClearValue(SelectedExplorerItemDependency); //reset possible bindings
            ExplorerGridBox.ClearValue(ExplorerItemsDependency); //reset possible bindings
        }
    }
}
