using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using OneDrive_Cloud_Player.Models;
using OneDrive_Cloud_Player.Models.GraphData;
using OneDrive_Cloud_Player.Models.Interfaces;
using OneDrive_Cloud_Player.Services.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Security.Authentication.Web.Provider;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace OneDrive_Cloud_Player.ViewModels
{
    /// <summary>
    /// Main view model
    /// </summary>
    public class VideoPlayerPageViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable, INavigable
    {
        private readonly INavigationService _navigationService;
        private readonly GraphHelper graphHelper;
        public bool IsSeeking { get; set; }
        private LibVLC LibVLC { get; set; }
        private MediaPlayer mediaPlayer;
        private VideoPlayerArgumentWrapper videoPlayerArgumentWrapper = null;

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
            Debug.WriteLine("Constructor called!");
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
        }

        private bool CanExecuteCommand()
        {
            return true;
        }

        private void InitializeLibVLC(InitializedEventArgs eventArgs)
        {
            Debug.WriteLine("Initializer called!");
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

                    //Fixes the problem that the video starts with its own audio value instead of our own.
                    MediaVolumeLevel = mediaVolumeLevel;

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
                                // Sometimes the mediaPlayer is still null when you exist the videoplayer page and this still gets called.
                                if(mediaPlayer != null)
                                {
                                    TimeLineValue = mediaPlayer.Time;
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

            //Retrieve the download URL from the drive item to be used for the video,
            return (string)driveItem.AdditionalData["@microsoft.graph.downloadUrl"];
        }

        /// <summary>
        /// Plays the media.
        /// </summary>
        private async Task PlayMedia(long startTime = 0)
        {
            string mediaDownloadURL = await RetrieveDownloadURLMedia(this.videoPlayerArgumentWrapper);
            //this.PlayVideo(VideoURL, VideoStartTime);

            MediaPlayer.Play(new Media(LibVLC, new Uri(mediaDownloadURL)));


            //Waits for the stream to be parsed so we do not raise a nullpointer exception.
            await mediaPlayer.Media.Parse(MediaParseOptions.ParseNetwork);
            MediaVolumeLevel = MediaVolumeLevel;

            if (mediaPlayer != null)
            {
                mediaPlayer.Time = startTime;
            }
        }

        private void SetMediaVolume(int volumeLevel)
        {
            if (mediaPlayer is null) return; // Return when the mediaPlayer is null so it does not cause exception.

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
                await PlayMedia(TimeLineValue);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Ëxception with reloading current media: " + e);
            }
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
            TimeLineValue = 0;
            Dispose();
            // Go back to the last page.
            _navigationService.GoBack();
        }

        /// <summary>
        /// Gets called when a user presses a key when the videoplayer page is open.
        /// </summary>
        /// <param name="e"></param>
        private void KeyDownEvent(KeyEventArgs e)
        {
            Debug.WriteLine(e.VirtualKey.ToString());
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
