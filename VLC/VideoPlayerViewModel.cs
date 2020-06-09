using LibVLCSharp.Shared;
using System;
using System.Windows;
using LibVLCSharp.WPF;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace OneDrive_Cloud_Player.VLC
{
    partial class VideoPlayerViewModel : Window
    {

        public static VideoView videoView { get; set; }

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
            set
            {
                volumeValue = value;
                NotifyStaticPropertyChanged("VolumeValue");
                SetVolume(value);
                //Console.WriteLine("New Volume Value!: " + volumeValue);
            }
        }

        private static long timeLineValue;

        public static long TimeLineValue
        {
            get { return timeLineValue; }
            set
            {
                timeLineValue = value;
                NotifyStaticPropertyChanged("TimeLineValue");
            }
        }

        private static long timeLineMaxLength;

        public static long TimeLineMaxLength
        {
            get { return timeLineMaxLength; }
            set
            {
                timeLineMaxLength = value;
                NotifyStaticPropertyChanged("TimeLineMaxLength");
                //Console.WriteLine(videoView.MediaPlayer.Time);
                //Console.WriteLine(videoView.MediaPlayer.Length);
                //Console.WriteLine("New timelinemaxlength Length!: " + timeLineMaxLength);
            }
        }

        private static bool isSeeking;

        public static bool IsSeeking
        {
            get { return isSeeking; }
            set { isSeeking = value; }
        }

        public static string itemId;
        public static string driveId;



        public static void Initialize(VideoView videoView)
        {
            VideoPlayerViewModel.videoView = videoView;
        }

        public static void PauseContinueButton(LibVLC _libVLC)
        {
            Console.WriteLine("CanPause: " + videoView.MediaPlayer.CanPause);

            if (!videoView.MediaPlayer.IsPlaying)
            {
                videoView.MediaPlayer.Play();
                PausePlayButtonTitle = "PAUSE";

                videoView.MediaPlayer.Play();
            }
            else
            {
                videoView.MediaPlayer.Pause();
                PausePlayButtonTitle = "PLAY";
            }
        }

        public static async void StartVideo(LibVLC _libVLC, string VideoURL)
        {
            //If video is not playing play video.
            if (!videoView.MediaPlayer.IsPlaying)
            {
                //Thread.Sleep(2000);
                Console.WriteLine("HWND: " + videoView.MediaPlayer.Hwnd);

                //Subscribe to IsPlaying event to set the TimeLineMaxLength only when the video is actually loaded.
                videoView.MediaPlayer.Playing += (sender, args) =>
                {
                    App.Current.Dispatcher.BeginInvoke(new MethodInvoker(() =>
                    {
                        TimeLineMaxLength = videoView.MediaPlayer.Length;
                        PausePlayButtonTitle = "PAUSE";
                    }));
                };

                //Sets volume on startup.
                videoView.MediaPlayer.Volume = VolumeValue;
                Console.WriteLine("Seekable: " + VideoPlayerViewModel.videoView.MediaPlayer.IsSeekable);

                //Plays the video from the url.
                videoView.MediaPlayer.Play(new Media(_libVLC, VideoURL, FromType.FromLocation));

                

                //Waits for the stream to be parsed so we do not raise a nullpointer exception.
                await videoView.MediaPlayer.Media.Parse(MediaParseOptions.ParseNetwork);


              
               

                //@todo Hier gebleven
                videoView.MediaPlayer.TimeChanged += (sender, args) =>
                {
                    App.Current.Dispatcher.BeginInvoke(new MethodInvoker(() =>
                    {
                        //Console.WriteLine("Time: " + videoView.MediaPlayer.Time + "Time changed");//Console.WriteLine("Time Changed!");
                        if (!IsSeeking)
                        {
                            TimeLineValue = videoView.MediaPlayer.Time;
                        }
                    }));
                };
            }
        }

        public static void ReloadVideo()
        {
            
        }

        private static void PrintTrackDetails()
        {
            //Print Track information.
            Console.WriteLine("\nTracks: \n");
            foreach (var item in videoView.MediaPlayer.Media.Tracks)
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
            Console.WriteLine("TrackS:::: " + videoView.MediaPlayer.Media.Tracks);
        }

        private static void SetVolume(int Volume)
        {
            videoView.MediaPlayer.Volume = Volume;
        }

        public static void DisposeVLC()
        {
            videoView.MediaPlayer.Stop();
            videoView.Dispose();
        }

        public static void StartSeeking()
        {
            IsSeeking = true;
        }

        public static void StopSeeking()
        {
            var newVideoTime = TimeLineValue;
            videoView.MediaPlayer.Time = newVideoTime;
            // Set video time to seek time
            IsSeeking = false;
            
        }


        public static event PropertyChangedEventHandler StaticPropertyChanged;

        private static void NotifyStaticPropertyChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
