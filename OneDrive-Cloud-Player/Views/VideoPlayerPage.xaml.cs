using OneDrive_Cloud_Player.ViewModels;
using System;
using System.Diagnostics;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace OneDrive_Cloud_Player.Views
{
    /// <summary>
    /// The page containing the video / media player.
    /// </summary>
    public partial class VideoPlayerPage
    {
        private readonly DispatcherTimer pointerMovementDispatcherTimer;
        private bool isPointerOverMediaControlGrid;
        private bool isPointerOverVideoPlayerPage = true;

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
        }

        /// <summary>
        /// Gets called when a pointer exits the media control grid of the mediaplayer.
        /// </summary>
        private void MediaControlGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            isPointerOverMediaControlGrid = false;
        }

        /// <summary>
        /// Gets called when a pointer enters the videoplayer page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            isPointerOverVideoPlayerPage = true;
        }

        /// <summary>
        /// Gets called when a pointer exits the videoplayer page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            isPointerOverVideoPlayerPage = false;
        }

        /// <summary>
        /// Gets called when a pointer moves across the mediaplayer.
        /// </summary>
        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
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

                // Only hide the cursor when it is over the videoplayer page.
                if (isPointerOverVideoPlayerPage)
                {
                    Window.Current.CoreWindow.PointerCursor = null;
                }
            }
            pointerMovementDispatcherTimer.Stop();
        }

        /// <summary>
        /// Switch to fullscreen mode and vice versa.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchFullscreenModeButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchFullscreenMode();
        }

        /// <summary>
        /// Switches the screen from windowed mode to fullscreen and vice versa.
        /// </summary>
        private void SwitchFullscreenMode()
        {
            ApplicationView view = ApplicationView.GetForCurrentView();

            if (view.IsFullScreenMode)
            {
                ExitFullscreenMode();
            }
            else
            {
                EnterFullscreenMode();
            }
            Debug.WriteLine(" + Switched screen mode.");
        }

        /// <summary>
        /// Enter fullscreen mode.
        /// </summary>
        private void EnterFullscreenMode()
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.TryEnterFullScreenMode())
            {
                ApplicationView.GetForCurrentView().FullScreenSystemOverlayMode = FullScreenSystemOverlayMode.Minimal;
                // The SizeChanged event will be raised when the entry to full-screen mode is complete.
            }
        }

        /// <summary>
        /// Leave fullscreen mode if currently in fullscreen mode.
        /// </summary>
        private void ExitFullscreenMode()
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
                // The SizeChanged event will be raised when the exit from full-screen mode is complete.
            }
        }

        /// <summary>
        /// Gets called when a user presses down a key on the videoplayer page. When the key is not listed in the switch case, it calls the view model to handle the key event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keyEvent"></param>
        void VideoPlayerPage_KeyDown(CoreWindow sender, KeyEventArgs keyEvent)
        {
            var viewModel = (VideoPlayerPageViewModel)DataContext;

            switch (keyEvent.VirtualKey)
            {
                case VirtualKey.F:
                    SwitchFullscreenMode();
                    break;
                case VirtualKey.Escape:
                    ExitFullscreenMode();
                    break;
                default:
                    // Pass the key event to the view model.
                    if (viewModel.KeyDownEventCommand.CanExecute(null))
                    {
                        viewModel.KeyDownEventCommand.Execute(keyEvent);
                    }
                    break;
            }
        }

        /// <summary>
        /// Executes after the user navigates to this page. This is used to add an event handler for the keydown event on this page.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown += VideoPlayerPage_KeyDown;

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Executes before the user navigates away from this page. This is used to remove an event handler for the keydown event on this page,
        /// and exit out of fullscreen if the app is still in fullscreen mode.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ExitFullscreenMode();

            Window.Current.CoreWindow.KeyDown -= VideoPlayerPage_KeyDown;

            base.OnNavigatingFrom(e);
        }
    }
}
