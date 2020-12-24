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
using Windows.System;
using Windows.UI.Core;

namespace OneDrive_Cloud_Player.ViewModels
{
    /// <summary>
    /// Main view model
    /// </summary>
    public class VideoPlayerPageViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable, INavigable
    {
        private readonly INavigationService _navigationService;
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
        public ICommand SeekBackwardCommand { get; }
        public ICommand SeekForewardCommand { get; }

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

        private string volumeButtonFontIcon = "\xE992";

        public string VolumeButtonFontIcon
        {
            get { return volumeButtonFontIcon; }
            set
            {
                volumeButtonFontIcon = value;
                RaisePropertyChanged("VolumeButtonFontIcon");
            }
        }

        private string playPauseButtonFontIcon = "\xE768";

        public string PlayPauseButtonFontIcon
        {
            get { return playPauseButtonFontIcon; }
            set
            {
                playPauseButtonFontIcon = value;
                RaisePropertyChanged("PlayPauseButtonFontIcon");
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
            StartedDraggingThumbCommand = new RelayCommand(StartedDraggingThumb, CanExecuteCommand);
            StoppedDraggingThumbCommand = new RelayCommand(StoppedDraggingThumb, CanExecuteCommand);
            ChangePlayingStateCommand = new RelayCommand(ChangePlayingState, CanExecuteCommand);
            SeekedCommand = new RelayCommand(Seeked, CanExecuteCommand);
            ReloadCurrentMediaCommand = new RelayCommand(ReloadCurrentMedia, CanExecuteCommand);
            StopMediaCommand = new RelayCommand(StopMedia, CanExecuteCommand);
            KeyDownEventCommand = new RelayCommand<KeyEventArgs>(KeyDownEvent);
            SeekBackwardCommand = new RelayCommand<double>(SeekBackward);
            SeekForewardCommand = new RelayCommand<double>(SeekForeward);
        }

        private bool CanExecuteCommand()
        {
            return true;
        }

        /// <summary>
        /// Gets called every time when navigated to this page.
        /// </summary>
        /// <param name="eventArgs"></param>
        private async void InitializeLibVLC(InitializedEventArgs eventArgs)
        {
            Debug.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + ": InitializeLibVLC called");

            // Reset properties.
            VideoLength = 0;
            PlayPauseButtonFontIcon = "\xE768";
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
                Debug.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + ": Media is playing");
                await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    MediaVolumeLevel = (int) App.Current.UserSettings.Values["MediaVolume"];
                    Debug.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + ": Set volume in container: " + App.Current.UserSettings.Values["MediaVolume"]);
                    Debug.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + ": Set volume in our property: " + MediaVolumeLevel);
                    Debug.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + ": Actual volume: " + MediaPlayer.Volume);
                    //Sets the max value of the seekbar.
                    VideoLength = MediaPlayer.Length;

                    PlayPauseButtonFontIcon = "\xE769";

                    //Enable or disable default subtitle based on user setting.
                    if (!(bool) App.Current.UserSettings.Values["ShowDefaultSubtitles"])
                    {
                        MediaPlayer.SetSpu(-1);
                    }
                });
            };

            //Subscribes to the Paused event.
            MediaPlayer.Paused += async (sender, args) =>
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    PlayPauseButtonFontIcon = "\xE768";
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

            await PlayMedia();
        }

        /// <summary>
        /// Retrieves the download url of the media file to be played.
        /// </summary>
        /// <param name="videoPlayerArgumentWrapper"></param>
        /// <returns></returns>
        private async Task<string> RetrieveDownloadURLMedia(VideoPlayerArgumentWrapper videoPlayerArgumentWrapper)
        {
            var driveItem = await GraphHelper.Instance().GetItemInformationAsync(
                videoPlayerArgumentWrapper.DriveId,
                videoPlayerArgumentWrapper.CachedDriveItem.ItemId);

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
            App.Current.UserSettings.Values["MediaVolume"] = volumeLevel; // Set the new volume in the MediaVolume setting.
            MediaPlayer.Volume = volumeLevel;
            UpdateVolumeButtonFontIcon(volumeLevel);
        }

        //TODO: Better alternative than this.
        /// <summary>
        /// Updates the icon of the volume button to a icon that fits by the volume level.
        /// </summary>
        private void UpdateVolumeButtonFontIcon(int volumeLevel)
        {
            if (volumeLevel <= 25 && !VolumeButtonFontIcon.Equals("\xE992"))
            {
                VolumeButtonFontIcon = "\xE992";
            }
            else if (volumeLevel > 25 && volumeLevel <= 50 && !VolumeButtonFontIcon.Equals("\xE993"))
            {
                VolumeButtonFontIcon = "\xE993";
            }
            else if (volumeLevel > 50 && volumeLevel <= 75 && !VolumeButtonFontIcon.Equals("\xE994"))
            {
                VolumeButtonFontIcon = "\xE994";
            }
            else if (volumeLevel > 75 && !VolumeButtonFontIcon.Equals("\xE995"))
            {
                VolumeButtonFontIcon = "\xE995";
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
        /// Seek backwards in the media by given miliseconds.
        /// </summary>
        /// <param name="ms"></param>
        public void SeekBackward(double ms)
        {
            SetVideoTime(MediaPlayer.Time - ms);
        }

        /// <summary>
        /// Seek foreward in the media by given miliseconds.
        /// </summary>
        /// <param name="ms"></param>
        public void SeekForeward(double ms)
        {
            SetVideoTime(MediaPlayer.Time + ms);
        }

        /// <summary>
        /// Sets the time of the media with the given time.
        /// </summary>
        /// <param name="time"></param>
        private void SetVideoTime(double time)
        {
            if (InvalidOneDriveSession)
            {
                Debug.WriteLine(" + OneDrive link expired.");
                Debug.WriteLine("   + Reloading media.");
                ReloadCurrentMedia();
            }
            MediaPlayer.Time = Convert.ToInt64(time);
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
                case VirtualKey.Left:
                    SeekBackward(5000);
                    break;
                case VirtualKey.Right:
                    SeekForeward(5000);
                    break;
                case VirtualKey.J:
                    SeekBackward(10000);
                    break;
                case VirtualKey.L:
                    SeekForeward(10000);
                    break;
            }
        }

        /// <summary>
        /// Gets the parameters that are sended with the navigation to the videoplayer page.
        /// </summary>
        /// <param name="parameter"></param>
        public void Activate(object videoPlayerArgumentWrapper)
        {
            Debug.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + ": Activate called");
            // Set the field so the playmedia method can use it.
            this.videoPlayerArgumentWrapper = (VideoPlayerArgumentWrapper)videoPlayerArgumentWrapper;
            Debug.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + ": Activate completed");
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
