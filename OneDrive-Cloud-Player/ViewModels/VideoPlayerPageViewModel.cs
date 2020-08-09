using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using OneDrive_Cloud_Player.Models;
using OneDrive_Cloud_Player.Models.Interfaces;
using OneDrive_Cloud_Player.Services.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;

namespace OneDrive_Cloud_Player.ViewModels
{
    /// <summary>
    /// Main view model
    /// </summary>
    public class VideoPlayerPageViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable, INavigable
    {
        private readonly ApplicationDataContainer localMediaVolumeLevelSetting;
        private readonly INavigationService _navigationService;
        private readonly GraphHelper graphHelper;
        private VideoPlayerArgumentWrapper videoPlayerArgumentWrapper = null;
        private bool InvalidOneDriveSession = false;
        private Timer reloadIntervalTimer;
        public bool IsSeeking { get; set; }
        private LibVLC LibVLC { get; set; }
        private MediaPlayer mediaPlayer;

        /// <summary>
        /// Gets the commands for the initialization
        /// </summary>
        public ICommand InitializeLibVLCCommand { get; }
        public ICommand StartedDraggingThumbCommand { get; }
        public ICommand StoppedDraggingThumbCommand { get; }
        public ICommand ChangePlayingStateCommand { get; }
        public ICommand SeekedCommand { get; }
        public ICommand ReloadCurrentMediaCommand { get; }
        public ICommand StopMediaCommand { get; }
        public ICommand KeyDownEventCommand { get; }

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

        private int mediaVolumeLevel;

        public int MediaVolumeLevel
        {
            get { return mediaVolumeLevel; }
            set
            {
                SetMediaVolume(value);
                mediaVolumeLevel = value;
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
            graphHelper = new GraphHelper();
            InitializeLibVLCCommand = new RelayCommand<InitializedEventArgs>(InitializeLibVLC);
            StartedDraggingThumbCommand = new RelayCommand(StartedDraggingThumb, CanExecuteCommand);
            StoppedDraggingThumbCommand = new RelayCommand(StoppedDraggingThumb, CanExecuteCommand);
            ChangePlayingStateCommand = new RelayCommand(ChangePlayingState, CanExecuteCommand);
            SeekedCommand = new RelayCommand(Seeked, CanExecuteCommand);
            ReloadCurrentMediaCommand = new RelayCommand(ReloadCurrentMedia, CanExecuteCommand);
            StopMediaCommand = new RelayCommand(StopMedia, CanExecuteCommand);
            KeyDownEventCommand = new RelayCommand<KeyEventArgs>(KeyDownEvent);

            this.localMediaVolumeLevelSetting = ApplicationData.Current.LocalSettings;

            // Sets the MediaVolume setting to 100 when its not already set before in the setting. (This is part of an audio workaround).
            if (localMediaVolumeLevelSetting.Values["MediaVolume"] is null)
            {
                localMediaVolumeLevelSetting.Values["MediaVolume"] = 100;
            }
        }

        private bool CanExecuteCommand()
        {
            return true;
        }

        /// <summary>
        /// Gets called every time when navigated to this page.
        /// </summary>
        /// <param name="eventArgs"></param>
        private void InitializeLibVLC(InitializedEventArgs eventArgs)
        {
            CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            LibVLC = new LibVLC(eventArgs.SwapChainOptions);
            MediaPlayer = new MediaPlayer(LibVLC);

            // Initialize timers.
            // Create a timer that fires the elapsed event when its time to retrieve and play the media from a new OneDrive download URL (2 minutes).
            reloadIntervalTimer = new Timer(120000);
            // Hook up the Elapsed event for the timer. 
            reloadIntervalTimer.Elapsed += (sender, e) =>
            {
                InvalidOneDriveSession = true;
            };
            reloadIntervalTimer.Enabled = true;

            /*
             * Subscribing to LibVLC events.
             */
            //Subscribes to the Playing event.
            MediaPlayer.Playing += async (sender, args) =>
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    MediaVolumeLevel = (int)this.localMediaVolumeLevelSetting.Values["MediaVolume"];
                    //Sets the max value of the seekbar.
                    VideoLength = MediaPlayer.Length;

                    PlayPauseButtonIconSource = "../Assets/Icons/pause.png";
                });
            };

            //Subscribes to the Paused event.
            MediaPlayer.Paused += async (sender, args) =>
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    PlayPauseButtonIconSource = "../Assets/Icons/play_arrow.png";
                });
            };

            MediaPlayer.TimeChanged += async (sender, args) =>
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    //Updates the value of the seekbar on TimeChanged event when the user is not seeking.
                    if (!IsSeeking)
                    {
                        // Sometimes the MediaPlayer is still null when you exist the videoplayer page and this still gets called.
                        if (MediaPlayer != null)
                        {
                            TimeLineValue = MediaPlayer.Time;
                        }
                    }
                });
            };
        }

        /// <summary>
        /// Retrieves the download url of the media file to be played.
        /// </summary>
        /// <param name="videoPlayerArgumentWrapper"></param>
        /// <returns></returns>
        private async Task<string> RetrieveDownloadURLMedia(VideoPlayerArgumentWrapper videoPlayerArgumentWrapper)
        {
            var driveItem = await graphHelper.GetItemInformationAsync(videoPlayerArgumentWrapper.DriveId, videoPlayerArgumentWrapper.CachedDriveItem.ItemId);

            //Retrieve a temporary download URL from the drive item.
            return (string)driveItem.AdditionalData["@microsoft.graph.downloadUrl"];
        }

        /// <summary>
        /// Plays the media.
        /// </summary>
        private async Task PlayMedia(long startTime = 0)
        {
            string mediaDownloadURL = await RetrieveDownloadURLMedia(this.videoPlayerArgumentWrapper);

            // Play the OneDrive file.
            MediaPlayer.Play(new Media(LibVLC, new Uri(mediaDownloadURL)));

            if (MediaPlayer != null)
            {
                MediaPlayer.Time = startTime;
            }
        }

        private void SetMediaVolume(int volumeLevel)
        {
            if (MediaPlayer is null)
            {
                Debug.WriteLine("Error: Sound problem, Returning without setting volume level!");
                return; // Return when the MediaPlayer is null so it does not cause exception.
            }
            this.localMediaVolumeLevelSetting.Values["MediaVolume"] = volumeLevel; // Set the new volume in the MediaVolume setting.
            MediaPlayer.Volume = volumeLevel;
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
            // Check wether or not the media should be reloaded.
            if (InvalidOneDriveSession)
            {
                //TODO: fix seekbar niet going instantly to the point where the user clicked. This gives the illusion that the seekbar is broken.
                ReloadCurrentMedia();
            }
            else
            {
                SetVideoTime(TimeLineValue);
            }
        }

        /// <summary>
        /// Sets the time of the media with the given time.
        /// </summary>
        /// <param name="time"></param>
        private void SetVideoTime(long time)
        {
            if (InvalidOneDriveSession)
            {
                Debug.WriteLine("OneDrive link expired.");
            }
            MediaPlayer.Time = time;
        }

        //TODO: Implement a Dialog system that shows a dialog when there is an error.
        /// <summary>
        /// Tries to restart the media that is currently playing.
        /// </summary>
        private async void ReloadCurrentMedia()
        {
            await PlayMedia(TimeLineValue);
            reloadIntervalTimer.Stop(); //In case a user reloads with the reload button. Stop the timer so we dont get multiple running.
            InvalidOneDriveSession = false;
            reloadIntervalTimer.Start();
        }

        /// <summary>
        /// Changes the media playing state from paused to playing and vice versa. 
        /// </summary>
        private void ChangePlayingState()
        {
            bool isPlaying = MediaPlayer.IsPlaying;

            if (isPlaying)
            {
                MediaPlayer.SetPause(true);
            }
            else if (!isPlaying)
            {
                MediaPlayer.SetPause(false);
            }
        }

        public void StopMedia()
        {
            MediaPlayer.Stop();
            TimeLineValue = 0;
            Dispose();
            // Go back to the last page.
            _navigationService.GoBack();
        }

        /// <summary>
        /// Gets called when a user presses a key when the videoplayer page is open.
        /// </summary>
        /// <param name="e"></param>
        private void KeyDownEvent(KeyEventArgs keyEvent)
        {
            switch (keyEvent.VirtualKey)
            {
                case VirtualKey.Space:
                    ChangePlayingState();
                    break;
            }
        }

        /// <summary>
        /// Gets the parameters that are sended with the navigation to the videoplayer page.
        /// </summary>
        /// <param name="parameter"></param>
        public async void Activate(object videoPlayerArgumentWrapper)
        {
            // Set the field so the playmedia method can use it.
            this.videoPlayerArgumentWrapper = (VideoPlayerArgumentWrapper)videoPlayerArgumentWrapper;
            await PlayMedia();
            //Debug.WriteLine(" + Activated: " + ((VideoPlayerArgumentWrapper)parameter).CachedDriveItem.ItemId);
        }

        //TODO: More research what this does.
        public void Deactivate(object parameter)
        {
            //throw new NotImplementedException();
            //Debug.WriteLine(" + Deactivated: " + parameter.ToString());
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
