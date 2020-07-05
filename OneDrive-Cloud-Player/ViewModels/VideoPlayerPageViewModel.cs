using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        //public event PropertyChangedEventHandler PropertyChanged;


        public RelayCommand DisplayMessageCommand { get; private set; }

        private long timeLineValue;

        public long TimeLineValue
        {
            get { return timeLineValue; }
            set
            {
                timeLineValue = value;
                RaisePropertyChanged("TimeLineValue");
                //Debug.WriteLine("Value updated! " + "[" + value + "]");
            }
        }

        private bool isSeeking;

        public bool IsSeeking
        {
            get { return isSeeking; }
            set
            {
                isSeeking = value;
            }
        }


        private LibVLC LibVLC { get; set; }

        private MediaPlayer _mediaPlayer;

        /// <summary>
        /// Gets the media player
        /// </summary>
        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            private set => Set(nameof(MediaPlayer), ref _mediaPlayer, value);
        }


        /// <summary>
        /// Gets the commands for the initialization
        /// </summary>
        public ICommand InitializedCommand { get; }
        public ICommand StartedDraggingThumbCommand { get; }
        public ICommand StoppedDraggingThumbCommand { get; }
        public ICommand SeekedCommand { get; }


        /// <summary>
        /// Initialized a new instance of <see cref="MainViewModel"/> class
        /// </summary>
        public VideoPlayerPageViewModel()
        {
            InitializedCommand = new RelayCommand<InitializedEventArgs>(Initialize);

            DisplayMessageCommand = new RelayCommand(this.DisplayMessage, CanExecuteCommand);
            StartedDraggingThumbCommand = new RelayCommand(this.StartedDraggingThumb, CanExecuteCommand);
            StoppedDraggingThumbCommand = new RelayCommand(this.StoppedDraggingThumb, CanExecuteCommand);
            SeekedCommand = new RelayCommand(this.Seeked, CanExecuteCommand);
        }

        private bool CanExecuteCommand()
        {
            return true;
        }

        private void DisplayMessage()
        {
            Debug.WriteLine("Message");
        }

        public void VideoPlayerLoaded()
        {
            Debug.WriteLine("Loaded player");
        }

        //private void Set<T>(string propertyName, ref T field, T value)
        //{
        //    if (field == null && value != null || field != null && !field.Equals(value))
        //    {
        //        field = value;
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}

        private void Initialize(InitializedEventArgs eventArgs)
        {
            LibVLC = new LibVLC(eventArgs.SwapChainOptions);
            MediaPlayer = new MediaPlayer(LibVLC);
            PlayVideo();
            UpdateSeekBarFromVideoTime();
        }

        private void PlayVideo()
        {
            MediaPlayer.Play(new Media(LibVLC, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")));
        }


        private async void UpdateSeekBarFromVideoTime()
        {
            CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;


            int i = 0;

            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //Set the video start time.
                //_mediaPlayer.Time = VideoStartTime;
                //VideoStartTime = 100;

                _mediaPlayer.TimeChanged += async (sender, args) =>
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (!IsSeeking)
                        {
                            //if(i > 1)
                            //{
                                TimeLineValue = _mediaPlayer.Time;
                            //    i = 0;
                            //    Debug.WriteLine(i);
                            //}
                            //else
                            //{
                            //    i++;
                            //}
                            //Debug.WriteLine(TimeLineValue);
                        }
                    });
                };
            });
        }

        public void StartedDraggingThumb()
        {
            Debug.WriteLine("Value" + TimeLineValue);
            IsSeeking = true;
        }

        public void StoppedDraggingThumb()
        {
            IsSeeking = false;
        }

        private void Seeked()
        {
            Debug.WriteLine("Clicked!");
            SetVideoTime(TimeLineValue);
        }

        private void SetVideoTime(long time)
        {
            _mediaPlayer.Time = time;
        }

        private void SwitchScreenMode()
        {
            var view = ApplicationView.GetForCurrentView();
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
                    ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
                    // The SizeChanged event will be raised when the entry to full-screen mode is complete.
                }
            }
            Debug.WriteLine("Switched Fullscreen State.");
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
