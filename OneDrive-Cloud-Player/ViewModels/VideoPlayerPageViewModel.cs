using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OneDrive_Cloud_Player.Models;
using OneDrive_Cloud_Player.Models.Interfaces;
using OneDrive_Cloud_Player.Services;
using OneDrive_Cloud_Player.Services.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Timer = System.Timers.Timer;

namespace OneDrive_Cloud_Player.ViewModels
{
    /// <summary>
    /// Main view model
    /// </summary>
    public class VideoPlayerPageViewModel : ObservableRecipient, IDisposable, INavigable, INotifyPropertyChanged
    {
        /// <summary>
        /// Fires every time the OneDrive download URL has expired (two minutes).
        /// </summary>
        private readonly Timer reloadIntervalTimer = new Timer();

        /// <summary>
        /// Single-shot timer to hide the filename shortly after playing a video.
        /// </summary>
        private readonly Timer fileNameOverlayTimer = new Timer()
        {
            AutoReset = false
        };

        /// <summary>
        /// Locks the volume-updater.
        /// </summary>
        private readonly object volumeLocker = new object();

        private readonly IMediaTrackService _mediaTrackService;
        /// <summary>
        /// Used to make sure that the volume is initialized once after starting video
        /// playback. This needs to happen every time when creating a new LibVLC object,
        /// so upon every navigation action to this page.
        /// </summary>
        private bool volumeUpdated = false;
        private MediaWrapper MediaWrapper = null;
        private bool InvalidOneDriveSession = false;
        private MediaPlayer _mediaPlayer;
        private int MediaListIndex;
        private bool _isFirstPlaying = true;
        private bool _isReloading = false;

        public bool IsSeeking { get; set; }
        private LibVLC LibVLC { get; set; }

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
        public ICommand SeekForwardCommand { get; }
        public ICommand PlayPreviousVideoCommand { get; }
        public ICommand PlayNextVideoCommand { get; }
        public ICommand OkClickedCommand { get; }

        private long timeLineValue;

        public long TimeLineValue
        {
            get { return timeLineValue; }
            set
            {
                timeLineValue = value;
                OnPropertyChanged();
            }
        }

        private long videoLength;

        public long VideoLength
        {
            get { return videoLength; }
            set
            {
                videoLength = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private string volumeButtonFontIcon = "\xE992";

        public string VolumeButtonFontIcon
        {
            get { return volumeButtonFontIcon; }
            set
            {
                volumeButtonFontIcon = value;
                OnPropertyChanged();
            }
        }

        private string fileName;

        public string FileName
        {
            get { return fileName; }
            private set
            {
                fileName = value;
                OnPropertyChanged();
            }
        }

        private Visibility fileNameOverlayVisiblity;

        public Visibility FileNameOverlayVisiblity
        {
            get { return fileNameOverlayVisiblity; }
            set
            {
                fileNameOverlayVisiblity = value;
                OnPropertyChanged();
            }
        }

        private string playPauseButtonFontIcon = "\xE768";

        public string PlayPauseButtonFontIcon
        {
            get { return playPauseButtonFontIcon; }
            set
            {
                playPauseButtonFontIcon = value;
                OnPropertyChanged();
            }
        }

        private string mediaControlGridVisibility = "Visible";

        public string MediaControlGridVisibility
        {
            get { return mediaControlGridVisibility; }
            set
            {
                mediaControlGridVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility visibilityPreviousMediaBtn;

        public Visibility VisibilityPreviousMediaBtn
        {
            get { return visibilityPreviousMediaBtn; }
            set
            {
                visibilityPreviousMediaBtn = value;
                OnPropertyChanged();
            }
        }

        private Visibility visibilityNextMediaBtn;

        public Visibility VisibilityNextMediaBtn
        {
            get { return visibilityNextMediaBtn; }
            set
            {
                visibilityNextMediaBtn = value;
                OnPropertyChanged();
            }
        }

        private bool isBackBtnEnabled = false;

        public bool IsBackBtnEnabled
        {
            get { return isBackBtnEnabled; }
            set
            {
                isBackBtnEnabled = value;
                OnPropertyChanged();
            }
        }

        private TrackDescription _selectedSubtitleTrack;

        public TrackDescription SelectedSubtitleTrack
        {
            get { return _selectedSubtitleTrack; }
            set
            {
                _selectedSubtitleTrack = value;
                SetSubtitleTrackById(value.Id);
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TrackDescription> _subtitleTracks = new ObservableCollection<TrackDescription>();

        public ObservableCollection<TrackDescription> SubtitleTracks
        {
            get { return _subtitleTracks; }
            set
            {
                _subtitleTracks = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the media player
        /// </summary>
        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            set => SetProperty(ref _mediaPlayer, value);
        }

        /// <summary>
        /// Initialized a new instance of <see cref="MainViewModel"/> class
        /// </summary>
        public VideoPlayerPageViewModel(IMediaTrackService mediaTrackService)
        {
            _mediaTrackService = mediaTrackService;
            InitializeLibVLCCommand = new RelayCommand<InitializedEventArgs>(InitializeLibVLC);
            StartedDraggingThumbCommand = new RelayCommand(StartedDraggingThumb, CanExecuteCommand);
            StoppedDraggingThumbCommand = new RelayCommand(StoppedDraggingThumb, CanExecuteCommand);
            ChangePlayingStateCommand = new RelayCommand(ChangePlayingState, CanExecuteCommand);
            SeekedCommand = new RelayCommand(Seeked, CanExecuteCommand);
            ReloadCurrentMediaCommand = new RelayCommand(ReloadCurrentMedia, CanExecuteCommand);
            StopMediaCommand = new RelayCommand(StopMedia, CanExecuteCommand);
            KeyDownEventCommand = new RelayCommand<KeyEventArgs>(KeyDownEvent);
            SeekBackwardCommand = new RelayCommand<double>(SeekBackward);
            SeekForwardCommand = new RelayCommand<double>(SeekForward);
            PlayPreviousVideoCommand = new RelayCommand(PlayPreviousVideo, CanExecuteCommand);
            PlayNextVideoCommand = new RelayCommand(PlayNextVideo, CanExecuteCommand);
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
            Debug.Assert(volumeUpdated == false);

            // Reset properties.
            VideoLength = 0;
            PlayPauseButtonFontIcon = "\xE768";

            // Create LibVLC instance.
            LibVLC = new LibVLC(eventArgs.SwapChainOptions);
            _mediaPlayer = new MediaPlayer(LibVLC);
            _mediaTrackService.Initialize(ref _mediaPlayer);
            // Subscribe to events only once.
            _mediaPlayer.Playing += MediaPlayer_Playing;
            _mediaPlayer.Paused += MediaPlayer_Paused;
            _mediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            _mediaPlayer.MediaChanged += MediaPlayer_MediaChanged;
            // Listen to the first volumechanged event, after which we can initialise the volume level correctly.
            _mediaPlayer.VolumeChanged += UpdateInitialVolume;
            reloadIntervalTimer.Elapsed += ReloadIntervalTimer_Elapsed;
            fileNameOverlayTimer.Elapsed += FileNameOverlayTimer_Elapsed;

            // Finally, play the media.
            await PlayMedia();
        }

        /// <summary>
        /// When called, update the volume in the GUI and mediaplayer with the saved volume
        /// in the user preferences. Ensures that unsubscribing only happens once using a lock.
        /// </summary>
        /// <remarks>
        /// This function is called once after creating a new LibVLC object, and listens to the first
        /// volumechanged event. The first time playback is started, LibVLC emits a volumechanged event
        /// with the initial volume level. When we have received this event, we know we can apply our own
        /// volume, since every write to the LibVLC volume before this event is thrown away when LibVLC
        /// initializes itself.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateInitialVolume(object sender, MediaPlayerVolumeChangedEventArgs e)
        {
            // Ensure this only gets executed once by unsubscribing while holding the lock.
            lock (volumeLocker)
            {
                if (volumeUpdated)
                {
                    return;
                }
                volumeUpdated = true;
            }

            // Unsubscribing and setting the volume need to happen on a different thread,
            // because unsubscribing calls into LibVLC, as does setting the volume.
            // Updating the GUI should happen on the dispatcher.
            await App.Current.UIDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                _mediaPlayer.VolumeChanged -= UpdateInitialVolume;
                MediaVolumeLevel = (int)App.Current.UserSettings.Values["MediaVolume"];
                IsBackBtnEnabled = true;
            });
        }

        /// <summary>
        /// Media playing event handler. Gets called when the media player state is changed to playing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MediaPlayer_Playing(object sender, EventArgs e)
        {
            Debug.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + ": Media is playing");

            if (_isFirstPlaying || _isReloading)
            {
                TrackDescription[] subtitleTracks = null;
                TrackDescription selectedSubtitleTrack = default;

                await Task.Run(() =>
                {
                    subtitleTracks = _mediaTrackService.GetEmbeddedSubtitleTracks();
                    selectedSubtitleTrack = _mediaTrackService.GetPreferredSubtitleTrack();
                });

                if (_isFirstPlaying)
                {
                    _isFirstPlaying = false;
                }

                if (_isReloading)
                {
                    _isReloading = false;

                    // Check if there is a selected subtitle track, for re-selection, to begin with.
                    if (_selectedSubtitleTrack.Id != 0 && _selectedSubtitleTrack.Name != null)
                    {
                        // Retrieve the same selected subtitle track again as the one that was used with the former subtitle tracks collection.
                        selectedSubtitleTrack = subtitleTracks.FirstOrDefault(subtitleTrack => subtitleTrack.Id == _selectedSubtitleTrack.Id);
                    }
                }

                await App.Current.UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // Clear collection as we want to refill it with updated tracks from the new media source.
                    SubtitleTracks.Clear();
                    foreach (TrackDescription subtitleTrack in subtitleTracks)
                    {
                        SubtitleTracks.Add(subtitleTrack);
                    }

                    // Select the correct subtitle track.
                    SelectedSubtitleTrack = selectedSubtitleTrack;
                });
            }

            await App.Current.UIDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                //Sets the max value of the seekbar.
                VideoLength = _mediaPlayer.Length;

                PlayPauseButtonFontIcon = "\xE769";
            });
        }

        private async void MediaPlayer_Paused(object sender, EventArgs e)
        {
            await App.Current.UIDispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                PlayPauseButtonFontIcon = "\xE768";
            });
        }

        private async void MediaPlayer_TimeChanged(object sender, EventArgs e)
        {
            await App.Current.UIDispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                // Updates the value of the seekbar on TimeChanged event
                // when the user is not seeking.
                if (!IsSeeking)
                {
                    // Sometimes the _mediaPlayer is already null upon
                    // navigating away and this still gets called.
                    if (_mediaPlayer != null)
                    {
                        TimeLineValue = _mediaPlayer.Time;
                    }
                }
            });
        }

        private void MediaPlayer_MediaChanged(object sender, EventArgs e)
        {
            if (!_isReloading)
            {
                _isFirstPlaying = true;
            }
        }

        private void ReloadIntervalTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            InvalidOneDriveSession = true;
        }

        private async void FileNameOverlayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await App.Current.UIDispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                FileNameOverlayVisiblity = Visibility.Collapsed;
            });
        }

        /// <summary>
        /// Retrieves the download url of the media file to be played.
        /// </summary>
        /// <param name="MediaWrapper"></param>
        /// <returns></returns>
        private async Task<string> RetrieveDownloadURLMedia(MediaWrapper mediaWrapper)
        {
            var driveItem = await GraphHelper.Instance().GetItemInformationAsync(
                mediaWrapper.DriveId,
                mediaWrapper.CachedDriveItem.ItemId);

            //Retrieve a temporary download URL from the drive item.
            return (string)driveItem.AdditionalData["@microsoft.graph.downloadUrl"];
        }

        /// <summary>
        /// Plays the media and starts a timer to temporarily show the filename
        /// of the file being played.
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        private async Task PlayMedia(long startTime = 0)
        {
            CheckPreviousNextMediaInList();

            // If the starttime is not 0, a reload is performed, so the filename is
            // already set and should stay hidden. The fileNameOverlayTimer should
            // also not be reset.
            if (startTime == 0)
            {
                FileName = MediaWrapper.CachedDriveItem.Name;
                FileNameOverlayVisiblity = Visibility.Visible;

                fileNameOverlayTimer.Interval = 5 * 1000;
                fileNameOverlayTimer.Start();
            }

            // The reloadIntervalTimer should be reset regardless of a reload.
            reloadIntervalTimer.Interval = 2 * 60 * 1000;
            reloadIntervalTimer.Start();

            string mediaDownloadURL = await RetrieveDownloadURLMedia(MediaWrapper);
            // Play the OneDrive file.
            using (Media media = new Media(LibVLC, new Uri(mediaDownloadURL)))
            {
                _mediaPlayer.Play(media);
            }

            if (_mediaPlayer is null)
            {
                Debug.WriteLine("PlayMedia: Error: Could not set start time.");
                return;
            }

            _mediaPlayer.Time = startTime;
        }

        /// <summary>
        /// Set the subtitle track used in the <see cref="_mediaPlayer"/> by id.
        /// </summary>
        /// <param name="subtitleTrackId">Subtitle track id</param>
        private void SetSubtitleTrackById(int subtitleTrackId)
        {
            _mediaPlayer.SetSpu(subtitleTrackId);
        }

        private void SetMediaVolume(int volumeLevel)
        {
            if (_mediaPlayer is null)
            {
                Debug.WriteLine("Error: SetMediaVolumeLevel: Could not set the volume.");
                return; // Return when the _mediaPlayer is null so it does not cause exception.
            }
            App.Current.UserSettings.Values["MediaVolume"] = volumeLevel; // Set the new volume in the MediaVolume setting.
            _mediaPlayer.Volume = volumeLevel;
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
            SetVideoTime(_mediaPlayer.Time - ms);
        }

        /// <summary>
        /// Seek forward in the media by given miliseconds.
        /// </summary>
        /// <param name="ms"></param>
        public void SeekForward(double ms)
        {
            SetVideoTime(_mediaPlayer.Time + ms);
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
            _mediaPlayer.Time = Convert.ToInt64(time);
        }

        //TODO: Implement a Dialog system that shows a dialog when there is an error.
        /// <summary>
        /// Restart the media from the currently timestamp.
        /// </summary>
        private async void ReloadCurrentMedia()
        {
            _isReloading = true;
            await PlayMedia(TimeLineValue);
            InvalidOneDriveSession = false;
        }

        /// <summary>
        /// Changes the media playing state from paused to playing and vice versa. 
        /// </summary>
        private void ChangePlayingState()
        {
            bool pause = MediaPlayer.IsPlaying;
            MediaPlayer.SetPause(pause);
        }

        public void StopMedia()
        {
            _mediaPlayer.Stop();
            TimeLineValue = 0;
            Dispose();
            // Go back to the last page.
            NavigationService.GoBack();
        }

        /// <summary>
        /// Gets called when a user presses a key when the videoplayer page is open.
        /// </summary>
        /// <param name="e"></param>
        private void KeyDownEvent(KeyEventArgs keyEvent)
        {
            CoreVirtualKeyStates shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);

            if (!shift.HasFlag(CoreVirtualKeyStates.Down))
                switch (keyEvent.VirtualKey)
                {
                    case VirtualKey.Space:
                        ChangePlayingState();
                        break;
                    case VirtualKey.Left:
                        SeekBackward(5000);
                        break;
                    case VirtualKey.Right:
                        SeekForward(5000);
                        break;
                    case VirtualKey.J:
                        SeekBackward(10000);
                        break;
                    case VirtualKey.L:
                        SeekForward(10000);
                        break;
                }

            if (shift.HasFlag(CoreVirtualKeyStates.Down))
            {
                switch (keyEvent.VirtualKey)
                {
                    case VirtualKey.N:
                        PlayNextVideo();
                        break;
                    case VirtualKey.P:
                        PlayPreviousVideo();
                        break;
                }
            }
        }

        /// <summary>
        /// Play the previous video in the list.
        /// </summary>
        private async void PlayPreviousVideo()
        {
            if ((MediaListIndex - 1) < 0)
            {
                return;
            }

            MediaWrapper.CachedDriveItem = App.Current.MediaItemList[--MediaListIndex];
            await PlayMedia();
        }

        /// <summary>
        /// Play the next video in the list.
        /// </summary>
        private async void PlayNextVideo()
        {
            if ((MediaListIndex + 1) >= App.Current.MediaItemList.Count)
            {
                return;
            }

            MediaWrapper.CachedDriveItem = App.Current.MediaItemList[++MediaListIndex];
            await PlayMedia();
        }

        /// <summary>
        /// Checks if there is an upcoming or a previous media file available and
        /// change the visibility status of the previous / next buttons accordingly.
        /// </summary>
        private void CheckPreviousNextMediaInList()
        {
            if ((MediaListIndex - 1) < 0)
            {
                VisibilityPreviousMediaBtn = Visibility.Collapsed;
            }
            else
            {
                VisibilityPreviousMediaBtn = Visibility.Visible;
            }

            if ((MediaListIndex + 1) >= App.Current.MediaItemList.Count)
            {
                VisibilityNextMediaBtn = Visibility.Collapsed;
            }
            else
            {
                VisibilityNextMediaBtn = Visibility.Visible;
            }
        }

        /// <summary>
        /// Called upon navigating to the videoplayer page and is used for
        /// passing arguments from the main page to the video player page.
        /// </summary>
        /// <param name="parameter"></param>
        public void Activate(object mediaWrapper)
        {
            // Set the field so the playmedia method can use it.
            MediaWrapper = (MediaWrapper)mediaWrapper;
            var list = App.Current.MediaItemList;

            // TODO: fix underlying issue where all properties of MediaWrapper.CachedDriveItem match
            // with an item in the list, but the hashcode doesn't. List.Contains works around this,
            // where IndexOf would return -1.
            if (list.Contains(MediaWrapper.CachedDriveItem))
            {
                MediaListIndex = list.IndexOf(MediaWrapper.CachedDriveItem);
            }
        }

        /// <summary>
        /// Called upon navigating away from the view associated with this viewmodel.
        /// </summary>
        /// <param name="parameter"></param>
        public void Deactivate(object parameter)
        {

        }

        /// <summary>
        /// Cleaning
        /// </summary>
        public void Dispose()
        {
            // TODO: Reproduce _mediaPlayer == null
            Debug.Assert(_mediaPlayer != null);
            Debug.Assert(LibVLC != null);

            // Unsubscribe from event handlers.
            _mediaPlayer.Playing -= MediaPlayer_Playing;
            _mediaPlayer.Paused -= MediaPlayer_Paused;
            _mediaPlayer.TimeChanged -= MediaPlayer_TimeChanged;
            reloadIntervalTimer.Elapsed -= ReloadIntervalTimer_Elapsed;
            fileNameOverlayTimer.Elapsed -= FileNameOverlayTimer_Elapsed;

            // Dispose of the LibVLC instance and the mediaplayer.
            var mediaPlayer = _mediaPlayer;
            _mediaPlayer = null;
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
