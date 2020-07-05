﻿using GalaSoft.MvvmLight;
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
        private LibVLC LibVLC { get; set; }

        private MediaPlayer _mediaPlayer;

        public RelayCommand DisplayMessageCommand { get; private set; }

        /// <summary>
        /// Gets the commands for the initialization
        /// </summary>
        public ICommand InitializedCommand { get; }
        public ICommand SwitchScreenModeCommand { get; }
        public ICommand StartedDraggingThumbCommand { get; }
        public ICommand StoppedDraggingThumbCommand { get; }
        public ICommand SeekedCommand { get; }

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

        private bool isSeeking;

        public bool IsSeeking
        {
            get { return isSeeking; }
            set
            {
                isSeeking = value;
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


        /// <summary>
        /// Gets the media player
        /// </summary>
        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            private set => Set(nameof(MediaPlayer), ref _mediaPlayer, value);
        }

        /// <summary>
        /// Initialized a new instance of <see cref="MainViewModel"/> class
        /// </summary>
        public VideoPlayerPageViewModel()
        {
            InitializedCommand = new RelayCommand<InitializedEventArgs>(Initialize);

            DisplayMessageCommand = new RelayCommand(DisplayMessage, CanExecuteCommand);
            SwitchScreenModeCommand = new RelayCommand(SwitchScreenMode, CanExecuteCommand);
            StartedDraggingThumbCommand = new RelayCommand(StartedDraggingThumb, CanExecuteCommand);
            StoppedDraggingThumbCommand = new RelayCommand(StoppedDraggingThumb, CanExecuteCommand);
            SeekedCommand = new RelayCommand(Seeked, CanExecuteCommand);
        }

        private bool CanExecuteCommand()
        {
            return true;
        }

        private void DisplayMessage()
        {
            Debug.WriteLine("Message");
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

        /// <summary>
        /// Plays the video.
        /// </summary>
        private async void PlayVideo()
        {
            CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            //Waits for the video to start playing to update the maximum value of the seekbar.
            _mediaPlayer.Playing += async (sender, args) =>
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    //Sets the max value of the seekbar.
                    VideoLength = _mediaPlayer.Length;

                    //PausePlayButtonImageSource = "/Assets/Icons/pause.png";
                });
            };

            MediaPlayer.Play(new Media(LibVLC, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")));

            //Waits for the stream to be parsed so we do not raise a nullpointer exception.
            await _mediaPlayer.Media.Parse(MediaParseOptions.ParseNetwork);
        }

        /// <summary>
        /// Updates the value of the seekbar with the value of the video time every timechanged event.
        /// </summary>
        private async void UpdateSeekBarFromVideoTime()
        {
            CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                //Set the video start time.
                //_mediaPlayer.Time = VideoStartTime;
                //VideoStartTime = 100;

                _mediaPlayer.TimeChanged += async (sender, args) =>
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        if (!IsSeeking)
                        {
                            TimeLineValue = _mediaPlayer.Time;
                        }
                    });
                };
            });
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
        /// Sets the time of the video with the time of the seekbar value.
        /// </summary>
        private void Seeked()
        {
            SetVideoTime(TimeLineValue);
        }

        /// <summary>
        /// Sets the time of the video with the given time.
        /// </summary>
        /// <param name="time"></param>
        private void SetVideoTime(long time)
        {
            _mediaPlayer.Time = time;
        }

        /// <summary>
        /// Switches the screen from windowed mode to fullscreen and reversed.
        /// </summary>
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
            Debug.WriteLine(" + Switched Fullscreen State.");
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
