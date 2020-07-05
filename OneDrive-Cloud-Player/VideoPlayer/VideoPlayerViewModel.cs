using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace OneDrive_Cloud_Player.VideoPlayer
{
    /// <summary>
    /// Main view model
    /// </summary>
    public class VideoPlayerViewModel : INotifyPropertyChanged, IDisposable
    {

        private long videoStartTime;

        public long VideoStartTime
        {
            get { return videoStartTime; }
            set
            {
                videoStartTime = value;
                NotifyPropertyChanged();
            }
        }

        private long timeLineValue;

        public long TimeLineValue
        {
            get { return timeLineValue; }
            set
            {
                timeLineValue = value;
                NotifyPropertyChanged();
            }
        }

        private long videoLength;

        public long VideoLength
        {
            get { return videoLength; }
            set
            {
                videoLength = value;
                NotifyPropertyChanged();
            }
        }

        private bool isSeeking = false;

        public bool IsSeeking
        {
            get { return isSeeking = false; }
            set
            {
                isSeeking = value;
                NotifyPropertyChanged();
            }
        }


        //public ICommand GreetMeCommand
        //{
        //    get
        //    {
        //        return new CommandHandler(() => this.GreetMeAction());
        //    }
        //}

        ICommand GreetMeCommand;

        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initialized a new instance of <see cref="MainViewModel"/> class
        /// </summary>
        public VideoPlayerViewModel()
        {
            InitializedCommand = new RelayCommand<InitializedEventArgs>(Initialize);
            StartedSeekingCommand = new RelayCommand<EventArgs>(StartedSeeking);
            GreetMeCommand = new CommandHandler(() => this.GreetMeAction());
            Window.Current.CoreWindow.KeyDown += KeyDownEventHandler;
        }


        private bool CanExecuteMethod(object obj)
        {
           return true;
        }



        private void GreetMeAction()
        {

            Debug.WriteLine("Hello");

        }
        /// <summary>
        /// Gets the command for the initialization
        /// </summary>
        public ICommand InitializedCommand { get; }

        public ICommand StartedSeekingCommand { get; }

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

        private void Set<T>(string propertyName, ref T field, T value)
        {
            if (field == null && value != null || field != null && !field.Equals(value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private async void Initialize(InitializedEventArgs eventArgs)
        {
            LibVLC = new LibVLC(eventArgs.SwapChainOptions);
            MediaPlayer = new MediaPlayer(LibVLC);
            MediaPlayer.Play(new Media(LibVLC, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")));
            CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            VideoLength = _mediaPlayer.Length;

            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
             {
                 //Set the video start time.
                 _mediaPlayer.Time = VideoStartTime;
                 //VideoStartTime = 100;

                 _mediaPlayer.TimeChanged += async (sender, args) =>
                 {
                     await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (!IsSeeking)
                        {
                            TimeLineValue = _mediaPlayer.Time;
                            Debug.WriteLine(TimeLineValue);
                        }
                    });
                 };
             });
        }



        /// <summary>
        /// Handles the key down events in the page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void KeyDownEventHandler(CoreWindow sender, KeyEventArgs args)
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
            Debug.WriteLine("Switched Fullscreen State");
        }

        public void StartedSeeking(EventArgs e)
        {
            IsSeeking = true;
            Debug.WriteLine("Seeking started!");
        }

        public void EndedDraggingSeekbar()
        {
            IsSeeking = false;
        }


        /// <summary>
        /// Destructor
        /// </summary>
        ~VideoPlayerViewModel()
        {
            Dispose();
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

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
