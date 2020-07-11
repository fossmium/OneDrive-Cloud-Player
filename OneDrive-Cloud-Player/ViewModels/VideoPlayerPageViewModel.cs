using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace OneDrive_Cloud_Player.ViewModels
{
    /// <summary>
    /// Main view model
    /// </summary>
    public class VideoPlayerPageViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
    {
        private readonly INavigationService _navigationService;
        public bool IsSeeking { get; set; }
        private LibVLC LibVLC { get; set; }
        private MediaPlayer mediaPlayer;

        /// <summary>
        /// Gets the commands for the initialization
        /// </summary>
        public ICommand InitializeLibVLCCommand { get; }
        public ICommand SwitchScreenModeCommand { get; }
        public ICommand StartedDraggingThumbCommand { get; }
        public ICommand StoppedDraggingThumbCommand { get; }
        public ICommand ChangePlayingStateCommand { get; }
        public ICommand SeekedCommand { get; }
        public ICommand ReloadCurrentMediaCommand { get; }
        public ICommand StopMediaCommand { get; }

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

        private int mediaVolumeLevel = 0;

        public int MediaVolumeLevel
        {
            get { return mediaVolumeLevel; }
            set
            {
                SetMediaVolume(value);
                RaisePropertyChanged("MediaVolumeLevel");
            }
        }

        private Uri volumeButtonIconSource = new Uri("ms-appx:///Assets/Icons/VolumeLevels/volume_low.png");

        public Uri VolumeButtonIconSource
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
            set => Set(nameof(MediaPlayer), ref mediaPlayer, value);
        }

        /// <summary>
        /// Initialized a new instance of <see cref="MainViewModel"/> class
        /// </summary>
        public VideoPlayerPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            
            InitializeLibVLCCommand = new RelayCommand<InitializedEventArgs>(InitializeLibVLC);
            SwitchScreenModeCommand = new RelayCommand(SwitchScreenMode, CanExecuteCommand);
            StartedDraggingThumbCommand = new RelayCommand(StartedDraggingThumb, CanExecuteCommand);
            StoppedDraggingThumbCommand = new RelayCommand(StoppedDraggingThumb, CanExecuteCommand);
            ChangePlayingStateCommand = new RelayCommand(ChangePlayingState, CanExecuteCommand);
            SeekedCommand = new RelayCommand(Seeked, CanExecuteCommand);
            ReloadCurrentMediaCommand = new RelayCommand(ReloadCurrentMedia, CanExecuteCommand);
            StopMediaCommand = new RelayCommand(StopMedia, CanExecuteCommand);
        }

        private bool CanExecuteCommand()
        {
            return true;
        }

        private async void InitializeLibVLC(InitializedEventArgs eventArgs)
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
            await PlayMedia("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4");

            SetMediaVolume(MediaVolumeLevel);
        }

        /// <summary>
        /// Plays the media.
        /// </summary>
        private async Task PlayMedia(string networkLocation, long startTime = 0)
        {
            MediaPlayer.Play(new Media(LibVLC, new Uri(networkLocation)));
            //Waits for the stream to be parsed so we do not raise a nullpointer exception.
            await mediaPlayer.Media.Parse(MediaParseOptions.ParseNetwork);

            mediaPlayer.Time = startTime;
        }

        private void SetMediaVolume(int volumeLevel)
        {
            Debug.WriteLine("+ Volume level changed to: " + volumeLevel);
            mediaPlayer.Volume = volumeLevel;
            UpdateVolumeButtonIconSource(volumeLevel);
        }

        //TODO: Better alternative than this ugly code.
        /// <summary>
        /// Updates the icon of the volume button to a icon that fits by the volume level.
        /// </summary>
        private void UpdateVolumeButtonIconSource(int volumeLevel)
        {
            if (volumeLevel <= 33 && !VolumeButtonIconSource.Equals(new Uri("ms-appx:///Assets/Icons/VolumeLevels/volume_low.png")))
            {
                VolumeButtonIconSource = new Uri("ms-appx:///Assets/Icons/VolumeLevels/volume_low.png");
            }
            else if (volumeLevel > 33 && volumeLevel <= 66 && !VolumeButtonIconSource.Equals(new Uri("ms-appx:///Assets/Icons/VolumeLevels/volume_medium.png")))
            {
                VolumeButtonIconSource = new Uri("ms-appx:///Assets/Icons/VolumeLevels/volume_medium.png");
            }
            else if (volumeLevel > 66 && !VolumeButtonIconSource.Equals(new Uri("ms-appx:///Assets/Icons/VolumeLevels/volume_high.png")))
            {
                VolumeButtonIconSource = new Uri("ms-appx:///Assets/Icons/VolumeLevels/volume_high.png");
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

        //TODO: Implement a Dialog system that shows a dialog when there is an error.
        /// <summary>
        /// Tries to restart the media that is currently playing.
        /// </summary>
        private async void ReloadCurrentMedia()
        {
            try
            {
                await PlayMedia("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4", TimeLineValue);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
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

        public void StopMedia()
        {
            ChangePlayingState();
            mediaPlayer.Stop();
            Dispose();
            // Go back to the last page.
            _navigationService.GoBack();
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
