using MahApps.Metro.Controls;
using Microsoft.Graph;
using System.Windows;
using System.Windows.Controls;

namespace Explorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ExplorerWindow : MetroWindow
    {
        public ExplorerWindow()
        {
            InitializeComponent();
        }

        private void ListBoxItem_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Console.WriteLine("Selected");
        }

        private void ListBox1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void ListBox1_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void ListBox1_PreviewMouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = (ItemsControl.ContainerFromElement(ListBox2, e.OriginalSource as DependencyObject) as ListBoxItem).Content as DriveItem;
            if (item != null)
            {
                //((ExplorerViewModel)(this.DataContext)).FolderChild = item;
                ((ExplorerViewModel)(this.DataContext)).GetSharedFolderChildren(item);


            }

        }
    }
}
