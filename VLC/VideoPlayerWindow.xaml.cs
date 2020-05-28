using System;
using LibVLCSharp.Shared;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OneDrive_Cloud_Player.VLC;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using LibVLCSharp.WPF;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Input;

namespace OneDrive_Cloud_Player.VLC
{
    partial class VideoPlayerWindow : Window
    {
        private DispatcherTimer dispatcherTimer;
        private MediaPlayer _mediaPlayer;
        private LibVLC _libVLC;
        private string VideoURL;
        private bool RunDispatcher;
        public string ButtonTitle { set; get; }

        public VideoPlayerWindow(string VideoURL)
        {
            InitializeComponent();
            //Create a timer with interval of 2 secs
            dispatcherTimer = new DispatcherTimer();
            RunDispatcher = true;
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            ButtonTitle = "TestPause";
            this.VideoURL = VideoURL;
            var label = new Label
            {
                Content = "TEwdsdasST",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Foreground = new SolidColorBrush(Colors.Red)
            };
            test.Children.Add(label);

            Core.Initialize();

            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            //we need the VideoView to be fully loaded before setting a MediaPlayer on it.
            //videoView.Loaded += (sender, e) => videoView.MediaPlayer = _mediaPlayer;

            //Set videoview field int the static VideoPlayerViewModel class.
            VideoPlayerViewModel.VideoView = videoView;


            videoView.MediaPlayer = _mediaPlayer;

            AutoStartVideo();
        }
        private void OnCaptureMouseRequest(object sender, RoutedEventArgs e)
        {
            Mouse.Capture(test);
            Console.WriteLine("MoveMent");
        }
        private void PauseContinueButton_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayerViewModel.PauseContinueButton(_libVLC);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayerViewModel.StopButton();
        }

        protected override void OnClosed(EventArgs e)
        {
            VideoPlayerViewModel.DisposeVLC();
        }

        private void NewVideoButton_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayerViewModel.NewVideoButton(_libVLC, this.VideoURL);
        }

        private void AutoStartVideo()
        {
            VideoPlayerViewModel.AutoStartVideo(this._libVLC, this.VideoURL);
        }

       /// <summary>
       /// Event for when the move moves. Resets the dispatcherTimer for hiding the controls.
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void VideoControls_MouseMove(object sender, MouseEventArgs e)
        {
            VideoControls.Visibility = Visibility.Visible;
            //Shows cursor again.
            Mouse.OverrideCursor = null;
            //Resets timer if its running when you move.
            //Console.WriteLine(Mouse.GetPosition(test));
            if (dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Stop();
            }
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

            //Hides Cursor.
            Mouse.OverrideCursor = Cursors.None;

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
        }

        private void VideoControls_MouseLeave(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Leave");
            RunDispatcher = true;
        }
    }
}
