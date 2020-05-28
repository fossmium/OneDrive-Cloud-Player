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
            set
            {
                pausePlayButtonTitle = value;
                //@Todo: uitzoeken waarom het hier niet werkt.
                NotifyStaticPropertyChanged("PausePlayButtonTitle");
            }

        }

        private static int volumeValue = 20;

        public static int VolumeValue
        {
            get { return volumeValue; }
            set { volumeValue = value;
                NotifyStaticPropertyChanged("VolumeValue");
                SetVolume(value);
                Console.WriteLine("New Volume Value!: " + volumeValue);
            }
        }


        public static VideoView VideoView {get;set;}


        public static void PauseContinueButton(LibVLC _libVLC)
        {
            Console.WriteLine("CanPause: " + VideoView.MediaPlayer.CanPause);

            if (!VideoView.MediaPlayer.IsPlaying)
            {
                VideoView.MediaPlayer.Play();
                pausePlayButtonTitle = "PAUSE";
                NotifyStaticPropertyChanged("PausePlayButtonTitle");

                VideoView.MediaPlayer.Play();

            }
            else
            {
                VideoView.MediaPlayer.Pause();
                pausePlayButtonTitle = "PLAY";
                NotifyStaticPropertyChanged("PausePlayButtonTitle");
            }
        }

        public static async void NewVideoButton(LibVLC _libVLC, string VideoURL)
        {
            //If video is pausable play video.
            if (!VideoView.MediaPlayer.IsPlaying)
            {
                //Sets volume on startup.
                VideoView.MediaPlayer.Volume = VolumeValue;

                VideoView.MediaPlayer.Play(new Media(_libVLC,
                VideoURL, FromType.FromLocation));


                pausePlayButtonTitle = "PAUSE";
                NotifyStaticPropertyChanged("PausePlayButtonTitle");

                //await VideoView.MediaPlayer.Media.Parse();
                await VideoView.MediaPlayer.Media.Parse(MediaParseOptions.ParseNetwork);

                //Print Track information.
                Console.WriteLine("\nTracks: \n");
                foreach (var item in VideoView.MediaPlayer.Media.Tracks)
                {
                    Console.WriteLine();
                    Console.WriteLine("ID:              |   " + item.Id);
                    Console.WriteLine("Type:            |   " + item.TrackType);
                    Console.WriteLine("Description:     |   " + item.Description);
                    Console.WriteLine("Data:            |   " + item.Data);
                    Console.WriteLine("Codec:           |   " + item.Codec);
                    Console.WriteLine("Language:        |   " + item.Language);
                    Console.WriteLine("Profile:         |   " + item.Profile);
                    Console.WriteLine("OriginalFourcc:  |   " + item.OriginalFourcc);

                }
                Console.WriteLine("TrackS:::: " + VideoView.MediaPlayer.Media.Tracks);
            }
        }

        public static void AutoStartVideo(LibVLC _libVLC, string VideoURL)
        {
            //If video is pausable play video.
            if (!VideoView.MediaPlayer.IsPlaying)
            {
                Console.WriteLine();
                VideoView.MediaPlayer.Play(new Media(_libVLC, VideoURL, FromType.FromLocation));


                VideoPlayerViewModel.NewVideoButton(_libVLC, VideoURL);
                Console.WriteLine("Hardware Decoding: " + VideoView.MediaPlayer.EnableHardwareDecoding);
            }
        }

        private static void SetVolume(int Volume)
        {
            VideoView.MediaPlayer.Volume = Volume;
        }

        public static void StopButton()
        {
            if (VideoView.MediaPlayer.IsPlaying)
            {
                VideoView.MediaPlayer.Stop();
            }
        }

        public static void DisposeVLC()
        {
            VideoView.MediaPlayer.Stop();
            VideoView.Dispose();
        }

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        private static void NotifyStaticPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }


    }
}
