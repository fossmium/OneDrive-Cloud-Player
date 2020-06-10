using LibVLCSharp.Shared;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using OneDrive_Cloud_Player.API;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using MethodInvoker = System.Windows.Forms.MethodInvoker;

namespace OneDrive_Cloud_Player.VLC
{
    partial class VideoPlayerWindow : MetroWindow, INotifyPropertyChanged
    {
        private int volumeValue = 20;

        public int VolumeValue
        {
            get { return volumeValue; }
            set
            {
                volumeValue = value;
                SetVolume(value);
                OnPropertyChanged("VolumeValue");
            }
        }

        private bool showLoadingCircle;

        public bool ShowLoadingCircle
        {
            get { return showLoadingCircle; }
            set
            {
                showLoadingCircle = value;
                OnPropertyChanged("ShowLoadingCircle");
            }
        }


        private long timeLineValue;

        public long TimeLineValue
        {
            get { return timeLineValue; }
            set
            {
                timeLineValue = value;
                OnPropertyChanged("TimeLineValue");
            }
        }

        private long timeLineMaxLength;

        public long TimeLineMaxLength
        {
            get { return timeLineMaxLength; }
            set
            {
                timeLineMaxLength = value;
                OnPropertyChanged("TimeLineMaxLength");
            }
        }

        private string pausePlayButtonTitle = "PLAY";

        public string PausePlayButtonTitle
        {
            get { return pausePlayButtonTitle; }
            set
            {
                pausePlayButtonTitle = value;
                OnPropertyChanged("PausePlayButtonTitle");
            }

        }

        private DispatcherTimer dispatcherTimer;
        private GraphHandler graphHandler;
        private readonly MediaPlayer mediaPlayer;
        private readonly LibVLC libVLC;
        private readonly string itemId;
        private readonly string driveId;
        private bool RunDispatcher;
        public string ButtonTitle;
        private string VideoURL;
        private bool IsSeeking;


        public VideoPlayerWindow(string driveId, string itemId)
        {
            InitializeComponent();

            Core.Initialize();

            //Initialize variables.
            this.driveId = driveId;
            this.itemId = itemId;
            RunDispatcher = true;
            libVLC = new LibVLC();
            mediaPlayer = new MediaPlayer(libVLC);
            graphHandler = new GraphHandler();
            videoView.MediaPlayer = mediaPlayer; // set the mediaplayer in the videoView.

            //Create a timer with interval.
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);

            //Call methods that need to be run at the start.
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

            this.PlayVideo(libVLC, VideoURL, VideoStartTime);

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
            this.PauseContinueButton();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            this.DisposeVLC();
        }

        protected override void OnClosed(EventArgs e)
        {
            this.DisposeVLC();
        }

        /// <summary>
        /// Activates the loading circle in the UI.
        /// </summary>
        private void StartedProcessing()
        {
            ShowLoadingCircle = true;
        }

        /// <summary>
        /// Deactivates the loading circle in the UI.
        /// </summary>
        private void EndedProcessing()
        {
            ShowLoadingCircle = false;
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
            if (VideoGrid.IsMouseOver)
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
            this.StartSeeking();
            Console.WriteLine("Started seekingnee DRAG started");
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            this.StopSeeking();
            Console.WriteLine("Stopped seekingnee DRAG completed");
        }

        private void ReloadVideoButton_Click(object sender, RoutedEventArgs e)
        {
            //Get the time of the video.
            long videoTime = this.TimeLineValue;
            videoView.MediaPlayer.Stop();
            //Start the video again with a new start time.
            StartVideoAsync(videoTime);
        }


        /// <summary>
        /// Contains the logic for playing a nework video.
        /// </summary>
        /// <param name="libVLC"></param>
        /// <param name="VideoURL"></param>
        /// <param name="VideoStartTime"></param>
        private async void PlayVideo(LibVLC libVLC, string VideoURL, long VideoStartTime = 0)
        {
            //If video is not playing play video.
            if (!videoView.MediaPlayer.IsPlaying)
            {
                StartedProcessing();

                //Thread.Sleep(2000);
                Console.WriteLine("HWND: " + videoView.MediaPlayer.Hwnd);

                //Subscribe to IsPlaying event to set the TimeLineMaxLength only when the video is actually loaded.
                videoView.MediaPlayer.Playing += (sender, args) =>
                {
                    App.Current.Dispatcher.BeginInvoke(new MethodInvoker(() =>
                    {
                        TimeLineMaxLength = videoView.MediaPlayer.Length;
                        PausePlayButtonTitle = "PAUSE";
                    }));
                };

                //Sets volume on startup.
                videoView.MediaPlayer.Volume = VolumeValue;
                Console.WriteLine("Seekable: " + this.videoView.MediaPlayer.IsSeekable);

                //Plays the video from the url.
                videoView.MediaPlayer.Play(new Media(libVLC, VideoURL, FromType.FromLocation));

                //Waits for the stream to be parsed so we do not raise a nullpointer exception.
                await videoView.MediaPlayer.Media.Parse(MediaParseOptions.ParseNetwork);

                EndedProcessing();

                //Set the video start time.
                videoView.MediaPlayer.Time = VideoStartTime;

                //@todo Hier gebleven
                videoView.MediaPlayer.TimeChanged += (sender, args) =>
                {
                    App.Current.Dispatcher.BeginInvoke(new MethodInvoker(() =>
                    {
                        if (!IsSeeking)
                        {
                            TimeLineValue = videoView.MediaPlayer.Time;
                        }
                    }));
                };
            }
        }

        /// <summary>
        /// Handles the logic for playing and pausing a video and updating the button in the UI.
        /// </summary>
        private void PauseContinueButton()
        {
            Console.WriteLine("CanPause: " + videoView.MediaPlayer.CanPause);

            if (!videoView.MediaPlayer.IsPlaying)
            {
                videoView.MediaPlayer.Play();
                PausePlayButtonTitle = "PAUSE";

                videoView.MediaPlayer.Play();
            }
            else
            {
                videoView.MediaPlayer.Pause();
                PausePlayButtonTitle = "PLAY";
            }
        }
        private void SetVolume(int Volume)
        {
            videoView.MediaPlayer.Volume = Volume;
        }

        public void DisposeVLC()
        {
            videoView.MediaPlayer.Stop();
            videoView.Dispose();
        }

        public void StartSeeking()
        {
            IsSeeking = true;
        }

        public void StopSeeking()
        {
            var newVideoTime = TimeLineValue;
            videoView.MediaPlayer.Time = newVideoTime;
            // Set video time to seek time
            IsSeeking = false;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
    }
}
