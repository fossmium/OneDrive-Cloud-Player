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
using Windows.UI.Core.Preview;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OneDrive_Cloud_Player.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlayerPage : Page
    {
        private readonly DispatcherTimer pointerMovementDispatcherTimer;
        private bool isPointerOverMediaControlGrid;

        public VideoPlayerPage()
        {
            this.InitializeComponent();

            SeekBar.AddHandler(PointerPressedEvent, new PointerEventHandler(SeekBar_PointerPressed), true);

            //Create a dispatch timer with interval.
            pointerMovementDispatcherTimer = new DispatcherTimer();
            pointerMovementDispatcherTimer.Tick += PointerMovementDispatcherTimer_Tick;
            pointerMovementDispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            pointerMovementDispatcherTimer.Start();
        }

        /// <summary>
        /// Retrieves the parameter that could be send when you navigate to this page.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameter = e.Parameter as CachedDriveItem;
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// When user clicks it needs to set the IsSeeking boolean to true.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeekBar_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ((VideoPlayerPageViewModel)(this.DataContext)).IsSeeking = true;
        }

        /// <summary>
        /// When the user releases the click is needs to set the IsSeeking boolean to false.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeekBar_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            ((VideoPlayerPageViewModel)(this.DataContext)).IsSeeking = false;
        }

        /// <summary>
        /// Gets called when a pointer enters the media control grid of the mediaplayer.
        /// </summary>
        private void MediaControlGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            isPointerOverMediaControlGrid = true;
            Debug.WriteLine(" + Pointer entered control grid.");
        }

        /// <summary>
        /// Gets called when a pointer exits the media control grid of the mediaplayer.
        /// </summary>
        private void MediaControlGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            isPointerOverMediaControlGrid = false;
            Debug.WriteLine(" + Pointer exited control grid.");
        }

        /// <summary>
        /// Gets called when a pointer moves across the mediaplayer.
        /// </summary>
        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (MediaControlGrid.Visibility == Visibility.Collapsed)
            {
                MediaControlGrid.Visibility = Visibility.Visible;
            }
            pointerMovementDispatcherTimer.Start();
        }

        /// <summary>
        /// Gets called when the dispatcher event fires of the pointer movement.
        /// It collapses the mediaplayer control grid on certain conditions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerMovementDispatcherTimer_Tick(object sender, object e)
        {
            if (!isPointerOverMediaControlGrid)
            {
                MediaControlGrid.Visibility = Visibility.Collapsed;
            }
            pointerMovementDispatcherTimer.Stop();
        }

        /// <summary>
        /// Switches the screen from windowed mode to fullscreen and vice versa.
        /// </summary>
        private void SwitchFullscreenModeButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
                // The SizeChanged event will be raised when the exit from full-screen mode is complete.
            }
            else
            {
                if (view.TryEnterFullScreenMode())
                {
                    ApplicationView.GetForCurrentView().FullScreenSystemOverlayMode = FullScreenSystemOverlayMode.Minimal;
                    // The SizeChanged event will be raised when the entry to full-screen mode is complete.
                }
            }
            Debug.WriteLine(" + Switched screen mode.");
        }
    }
}
