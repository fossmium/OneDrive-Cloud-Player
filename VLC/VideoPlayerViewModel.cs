using LibVLCSharp.Shared;
using OneDrive_Cloud_Player.Login;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using OneDrive_Cloud_Player.VLC;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using LibVLCSharp.WPF;
using System.Diagnostics;
using Microsoft.Graph;

namespace OneDrive_Cloud_Player.VLC
{
   partial class VideoPlayerViewModel : Window
    {
        public ICommand PlayVideoCommand { get; set; }
        public ICommand StopVideoCommand { get; set; }
        //public VideoView videoView { get; private set; }


        public VideoPlayerViewModel()
        {
           
            PlayVideoCommand = new VideoPlayerModel(PlayVideo, CanExecutePlayVideo);
            StopVideoCommand = new VideoPlayerModel(StopVideo, CanExecutePlayStopVideo);


            //var label = new Label
            //{
            //    Content = "TEwdsdasST",
            //    HorizontalAlignment = HorizontalAlignment.Right,
            //    VerticalAlignment = VerticalAlignment.Bottom,
            //    Foreground = new SolidColorBrush(Colors.Red)
            //};
            //test.Children.Add(label);

            //Core.Initialize();

            //_libVLC = new LibVLC();
            //_mediaPlayer = new MediaPlayer(_libVLC);


            //videoView.Loaded += VideoView_Loaded;

            // we need the VideoView to be fully loaded before setting a MediaPlayer on it.
            //videoView.Loaded += (sender, e) => videoView.MediaPlayer = _mediaPlayer;

        }

        private bool CanExecutePlayStopVideo(object arg)
        {
            return true;
        }

        private bool CanExecutePlayVideo(object arg)
        {
            return true;
        }

        private void StopVideo(object parameter)
        {
            Console.WriteLine("Stop video");
        }

        private void PlayVideo(object parameter)
        {

        }
    }
}
