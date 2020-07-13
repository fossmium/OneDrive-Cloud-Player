using OneDrive_Cloud_Player.ViewModels;
using System;
using System.Diagnostics;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OneDrive_Cloud_Player.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlayerPage
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
            SwitchFullscreenMode();
        }

        private void SwitchFullscreenMode()
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

        public void Activate(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Deactivate(object parameter)
        {
            throw new NotImplementedException();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
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
        void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs keyEvent)
        {
            //todo
            Debug.WriteLine("Key down");
            var viewModel = (VideoPlayerPageViewModel)DataContext;

            switch (keyEvent.VirtualKey)
            {
                case VirtualKey.F:
                    SwitchFullscreenMode();
                    break;
                case VirtualKey.Escape:
                    SwitchFullscreenMode(); //TODO: Create dedicated method to close fullscreen or use parameter.
                    break;
                default:
                    if (viewModel.KeyDownEventCommand.CanExecute(null))
                    {
                        viewModel.KeyDownEventCommand.Execute(keyEvent);
                    }
                    break;
            }

        }

        /// <summary>
        /// Executes when the page unloads. For example when the user navigates to another page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParamNavigationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
        }

        /// <summary>
        /// Executes when the page loads. For example when the user navigates to this page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParamNavigationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }
    }
}
