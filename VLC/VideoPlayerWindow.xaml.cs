using LibVLCSharp.Shared;
using Microsoft.Graph;
using OneDrive_Cloud_Player.API;
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace OneDrive_Cloud_Player.VLC
{
    partial class VideoPlayerWindow : Window
    {
        private DispatcherTimer dispatcherTimer;
        private MediaPlayer mediaPlayer;
        private LibVLC libVLC;
        private GraphHandler graphHandler;
        private string VideoURL;
        private bool RunDispatcher;
        public string ButtonTitle { set; get; }

        public string itemId;
        public string driveId;

        public VideoPlayerWindow(string driveId, string itemId)
        {
            InitializeComponent();

            RunDispatcher = true;

            //Create a timer with interval of 2 secs
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);

            Core.Initialize();

            libVLC = new LibVLC();
            mediaPlayer = new MediaPlayer(libVLC);
            graphHandler = new GraphHandler();

            // set the mediaplayer in the videoView.
            videoView.MediaPlayer = mediaPlayer;

            //Initialize variables in the viewmodel.
            VideoPlayerViewModel.videoView = videoView;
            this.driveId = driveId;
            this.itemId = itemId;

            StartVideoAsync();

            //Start the timer
            dispatcherTimer.Start();

            SeekBar.ApplyTemplate();
            Thumb thumb = (SeekBar.Template.FindName("PART_Track", SeekBar) as Track).Thumb;
            thumb.MouseEnter += new MouseEventHandler(Thumb_MouseEnter);
        }

        private async void StartVideoAsync(long VideoStartTime = 0)
        {
            //Gets the drive item with a graph call.
            var driveItem = await graphHandler.GetItemInformationAsync(driveId, itemId);
            //Retrieve the download URL from the drive item to be used for the video,
            VideoURL = (string)driveItem.AdditionalData["@microsoft.graph.downloadUrl"];

            VideoPlayerViewModel.StartVideo(libVLC, VideoURL, VideoStartTime);
        }

        /// <summary>
        /// Enables the user to click and drag everywhere on the slider track.
        /// </summary>
        /// Code from: https://social.msdn.microsoft.com/Forums/vstudio/en-US/5fa7cbc2-c99f-4b71-b46c-f156bdf0a75a/making-the-slider-slide-with-one-click-anywhere-on-the-slider
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.MouseDevice.Captured == null)
            {
                // the left button is pressed on mouse enter
                // but the mouse isn't captured, so the thumb
                // must have been moved under the mouse in response
                // to a click on the track.
                // Generate a MouseLeftButtonDown event.
                MouseButtonEventArgs args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);
                args.RoutedEvent = MouseLeftButtonDownEvent;
                (sender as Thumb).RaiseEvent(args);
            }
        }

        private void PauseContinueButton_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayerViewModel.PauseContinueButton();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            VideoPlayerViewModel.DisposeVLC();
        }

        protected override void OnClosed(EventArgs e)
        {
            VideoPlayerViewModel.DisposeVLC();
        }

        /// <summary>
        /// Event for when the mouse moves. Resets the dispatcherTimer for hiding the controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoControls_MouseMove(object sender, MouseEventArgs e)
        {
            //Make controls anc cursor visible again
            VideoControls.Visibility = Visibility.Visible;
            Mouse.OverrideCursor = null;

            //Starts dispatcher timer.
            if (RunDispatcher)
            {
                //Start the timer
                dispatcherTimer.Start();
            }
        }

        /// <summary>
        /// Start Counting down to hide the control elements.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            //Things which happen after 1 timer interval
            VideoControls.Visibility = System.Windows.Visibility.Collapsed;

            //Only hide cursor when it is directly above the video grid.
            if (Mouse.DirectlyOver == this.VideoGrid)
            {
                //Hides Cursor.
                Mouse.OverrideCursor = Cursors.None;
            }
            //Disable the timer
            dispatcherTimer.IsEnabled = false;
        }

        private void StopDispatcher()
        {
            dispatcherTimer.Stop();
        }

        private void VideoControls_MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Enter");
            RunDispatcher = false;
            StopDispatcher();

            //WindowStyle = WindowStyle.None;
            //WindowState = WindowState.Maximized;
        }

        private void VideoControls_MouseLeave(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Leave");
            RunDispatcher = true;
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            VideoPlayerViewModel.StartSeeking();
            Console.WriteLine("Started seekingnee DRAG started");
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            VideoPlayerViewModel.StopSeeking();
            Console.WriteLine("Stopped seekingnee DRAG completed");
        }

        private void ReloadVideoButton_Click(object sender, RoutedEventArgs e)
        {
            //Get the time of the video.
            long videoTime = VideoPlayerViewModel.TimeLineValue;
            videoView.MediaPlayer.Stop();
            //Start the video again with a new start time.
            StartVideoAsync(videoTime);
        }
    }
}
