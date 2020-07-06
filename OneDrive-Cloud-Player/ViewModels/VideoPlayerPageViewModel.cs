using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace OneDrive_Cloud_Player.ViewModels
{
    /// <summary>
    /// Main view model
    /// </summary>
    public class VideoPlayerPageViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
    {
        public bool IsSeeking { get; set; }
        public RelayCommand DisplayMessageCommand { get; private set; }
        private LibVLC LibVLC { get; set; }
        private MediaPlayer mediaPlayer;
        private bool isPointerOverMediaControlGrid;
        private readonly DispatcherTimer pointerMovementDispatcherTimer;

        /// <summary>
        /// Gets the commands for the initialization
        /// </summary>
        public ICommand InitializeLibVLCCommand { get; }
        public ICommand SwitchScreenModeCommand { get; }
        public ICommand StartedDraggingThumbCommand { get; }
        public ICommand StoppedDraggingThumbCommand { get; }
        public ICommand ChangePlayingStateCommand { get; }
        public ICommand SeekedCommand { get; }
        public ICommand PointerEnteredMediaControlGridCommand { get; }
        public ICommand PointerExitedMediaControlGridCommand { get; }
        public ICommand PointerMovedMediaPlayerCommand { get; }

        private long timeLineValue;

        public long TimeLineValue
        {
            get { return timeLineValue; }
            set
            {
                timeLineValue = value;
                RaisePropertyChanged("TimeLineValue");
            }
        }

        private long videoLength;

        public long VideoLength
        {
            get { return videoLength; }
            set
            {
                videoLength = value;
                RaisePropertyChanged("VideoLength");
            }
        }

        private int videoVolume = 10;

        public int VideoVolume
        {
            get { return videoVolume; }
            set
            {
                SetMediaVolume(value);
                RaisePropertyChanged("VideoVolume");
            }
        }

        private string volumeButtonIconSource;

        public string VolumeButtonIconSource
        {
            get { return volumeButtonIconSource; }
            set
            {
                volumeButtonIconSource = value;
                RaisePropertyChanged("VolumeButtonIconSource");
            }
        }

        private string playPauseButtonImageSource = "../Assets/Icons/play_arrow.png";

        public string PlayPauseButtonIconSource
        {
            get { return playPauseButtonImageSource; }
            set
            {
                playPauseButtonImageSource = value;
                RaisePropertyChanged("PlayPauseButtonIconSource");
            }
        }

        private string mediaControlGridVisibility = "Visible";

        public string MediaControlGridVisibility
        {
            get { return mediaControlGridVisibility; }
            set
            {
                mediaControlGridVisibility = value;
                RaisePropertyChanged("MediaControlGridVisibility");
            }
        }

        /// <summary>
        /// Gets the media player
        /// </summary>
        public MediaPlayer MediaPlayer
        {
            get => mediaPlayer;
            private set => Set(nameof(MediaPlayer), ref mediaPlayer, value);
        }

        /// <summary>
        /// Initialized a new instance of <see cref="MainViewModel"/> class
        /// </summary>
        public VideoPlayerPageViewModel()
        {
            InitializeLibVLCCommand = new RelayCommand<InitializedEventArgs>(InitializeLibVLC);
            DisplayMessageCommand = new RelayCommand(DisplayMessage, CanExecuteCommand);
            SwitchScreenModeCommand = new RelayCommand(SwitchScreenMode, CanExecuteCommand);
            StartedDraggingThumbCommand = new RelayCommand(StartedDraggingThumb, CanExecuteCommand);
            StoppedDraggingThumbCommand = new RelayCommand(StoppedDraggingThumb, CanExecuteCommand);
            ChangePlayingStateCommand = new RelayCommand(ChangePlayingState, CanExecuteCommand);
            SeekedCommand = new RelayCommand(Seeked, CanExecuteCommand);
            PointerEnteredMediaControlGridCommand = new RelayCommand(PointerEnteredMediaControlGrid, CanExecuteCommand);
            PointerExitedMediaControlGridCommand = new RelayCommand(PointerExitedMediaControlGrid, CanExecuteCommand);
            PointerMovedMediaPlayerCommand = new RelayCommand(PointerMovedMediaPlayer, CanExecuteCommand);

            //Create a timer with interval.
            pointerMovementDispatcherTimer = new DispatcherTimer();
            pointerMovementDispatcherTimer.Tick += PointerMovementDispatcherTimer_Tick;
            pointerMovementDispatcherTimer.Interval = new TimeSpan(0, 0, 2);

            pointerMovementDispatcherTimer.Start();
        }

        private bool CanExecuteCommand()
        {
            return true;
        }

        private void DisplayMessage()
        {
            Debug.WriteLine("Message");
        }

        private void InitializeLibVLC(InitializedEventArgs eventArgs)
        {
            CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            LibVLC = new LibVLC(eventArgs.SwapChainOptions);
            MediaPlayer = new MediaPlayer(LibVLC);

            /*
             * Subscribing to LibVLC events.
             */
            //Subscribes to the Playing event.
            mediaPlayer.Playing += async (sender, args) =>
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    //Sets the max value of the seekbar.
                    VideoLength = mediaPlayer.Length;

                    PlayPauseButtonIconSource = "../Assets/Icons/pause.png";
                });
            };

            //Subscribes to the Paused event.
            mediaPlayer.Paused += async (sender, args) =>
                    {
                        await dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            PlayPauseButtonIconSource = "../Assets/Icons/play_arrow.png";
                        });
                    };

            //Subscribes to the Paused event.
            mediaPlayer.VolumeChanged += async (sender, args) =>
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    //UpdateVolumeButtonIconSource(mediaPlayer.Volume);
                    Debug.WriteLine("Volume changed event fired!");
                });
            };
            //Set the video start time.
            //mediaPlayer.Time = VideoStartTime;
            //VideoStartTime = 100;

            mediaPlayer.TimeChanged += async (sender, args) =>
                    {
                        await dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            //Updates the value of the seekbar on TimeChanged event when the user is not seeking.
                            if (!IsSeeking)
                            {
                                TimeLineValue = mediaPlayer.Time;
                            }
                        });
                    };

            //Play the media.
            PlayMedia();
            SetMediaVolume(30);
        }

        /// <summary>
        /// Plays the media.
        /// </summary>
        private void PlayMedia()
        {
            MediaPlayer.Play(new Media(LibVLC, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4")));
            //Waits for the stream to be parsed so we do not raise a nullpointer exception.
            //await mediaPlayer.Media.Parse(MediaParseOptions.ParseNetwork);
        }

        private void SetMediaVolume(int volumeLevel)
        {
            Debug.WriteLine("Volume changed to: " + volumeLevel);
            mediaPlayer.Volume = volumeLevel;
        }

        /// <summary>
        /// Updates the icon of the volume button to a icon that fits by the volume level.
        /// </summary>
        private void UpdateVolumeButtonIconSource(int volumeLevel)
        {
            if (volumeLevel <= 33)
            {
                VolumeButtonIconSource = "../Assets/Icons/VolumeLevels/volume_low.png";
            }
            else if (volumeLevel > 33 && volumeLevel <= 66)
            {

            }
            else if (volumeLevel > 66)
            {

            }
        }

        /// <summary>
        /// Sets the IsSeekig boolean on true so the seekbar value does not get updated.
        /// </summary>
        public void StartedDraggingThumb()
        {
            IsSeeking = true;
        }

        /// <summary>
        /// Sets the IsIseeking boolean on false so the seekbar value can gets updates again.
        /// </summary>
        public void StoppedDraggingThumb()
        {
            IsSeeking = false;
        }

        /// <summary>
        /// Sets the time of the media with the time of the seekbar value.
        /// </summary>
        private void Seeked()
        {
            SetVideoTime(TimeLineValue);
        }

        /// <summary>
        /// Sets the time of the media with the given time.
        /// </summary>
        /// <param name="time"></param>
        private void SetVideoTime(long time)
        {
            mediaPlayer.Time = time;
        }

        /// <summary>
        /// Switches the screen from windowed mode to fullscreen and reversed.
        /// </summary>
        private void SwitchScreenMode()
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

        /// <summary>
        /// Changes the media playing state from paused to playing and vice versa. 
        /// </summary>
        private void ChangePlayingState()
        {
            bool isPlaying = mediaPlayer.IsPlaying;

            if (isPlaying)
            {
                mediaPlayer.SetPause(true);
            }
            else if (!isPlaying)
            {
                mediaPlayer.SetPause(false);
            }
        }

        /// <summary>
        /// Gets called when a pointer moves across the mediaplayer.
        /// </summary>
        private void PointerMovedMediaPlayer()
        {
            if (MediaControlGridVisibility.Equals("Collapsed"))
            {
                MediaControlGridVisibility = "Visible";
            }
            pointerMovementDispatcherTimer.Start();
        }

        /// <summary>
        /// Gets called when a pointer enters the media control grid of the mediaplayer.
        /// </summary>
        private void PointerEnteredMediaControlGrid()
        {
            isPointerOverMediaControlGrid = true;
            Debug.WriteLine(" + Pointer entered control grid.");
            //MediaControlGridVisibility = "Collapsed";

        }

        /// <summary>
        /// Gets called when a pointer exits the media control grid of the mediaplayer.
        /// </summary>
        private void PointerExitedMediaControlGrid()
        {
            isPointerOverMediaControlGrid = false;
            Debug.WriteLine(" + Pointer exited control grid.");
        }

        /// <summary>
        /// Gets called when the dispatcher event fires of the pointer movement.
        /// It collapses the mediaplayer control grid on certain conditions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerMovementDispatcherTimer_Tick(object sender, object e)
        {
            Debug.WriteLine("Timer ticked");
            if (!isPointerOverMediaControlGrid)
            {
                MediaControlGridVisibility = "Collapsed";
            }
            pointerMovementDispatcherTimer.Stop();
        }

        /// <summary>
        /// Cleaning
        /// </summary>
        public void Dispose()
        {
            var mediaPlayer = MediaPlayer;
            MediaPlayer = null;
            mediaPlayer?.Dispose();
            LibVLC?.Dispose();
            LibVLC = null;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~VideoPlayerPageViewModel()
        {
            Dispose();
        }
    }
}
