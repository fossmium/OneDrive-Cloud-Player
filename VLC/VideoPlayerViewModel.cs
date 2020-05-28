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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OneDrive_Cloud_Player.VLC
{
   partial class VideoPlayerViewModel : Window
   {

        private static string pausePlayButtonTitle = "PLAY";

        

        public static string PausePlayButtonTitle
        {
            get { return pausePlayButtonTitle; }
            set { pausePlayButtonTitle = value;
                //@Todo: uitzoeken waarom het hier niet werkt.
                NotifyStaticPropertyChanged("PausePlayButtonTitle");
            }

        }


        public static void PauseContinueButton(LibVLC _libVLC, VideoView VideoView)
        {
            Console.WriteLine("CanPause: " + VideoView.MediaPlayer.CanPause);

            if (!VideoView.MediaPlayer.IsPlaying)
            {
                VideoView.MediaPlayer.Play();
                pausePlayButtonTitle = "PAUSE";
                NotifyStaticPropertyChanged("PausePlayButtonTitle");
            }
            else
            {
                VideoView.MediaPlayer.Pause();
                pausePlayButtonTitle = "PLAY";
                NotifyStaticPropertyChanged("PausePlayButtonTitle");
            }
        }
        
        public static void NewVideoButton(LibVLC _libVLC, VideoView VideoView, string VideoURL)
        {
            //If video is pausable play video.
            if (!VideoView.MediaPlayer.IsPlaying)
            {
                VideoView.MediaPlayer.Play(new Media(_libVLC,
                    VideoURL, FromType.FromLocation));
                pausePlayButtonTitle = "PAUSE";
                NotifyStaticPropertyChanged("PausePlayButtonTitle");
            }
        }

        public static void StopButton(VideoView VideoView)
        {
            if (VideoView.MediaPlayer.IsPlaying)
            {
                VideoView.MediaPlayer.Stop();
            }
        }

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        private static void NotifyStaticPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
