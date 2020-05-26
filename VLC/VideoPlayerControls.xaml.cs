using LibVLCSharp.Shared;
using System;
using System.Windows;
using System.Windows.Controls;


namespace OneDrive_Cloud_Player.VLC
{
    /// <summary>
    /// Interaction logic for VideoPlayerControls.xaml
    /// </summary>
    public partial class VideoPlayerControls : UserControl
    {
        public VideoPlayerControls()
        {

        }
        //readonly VideoPlayerWindow parent;
        //LibVLC _libVLC;
        //MediaPlayer _mediaPlayer;

        //public VideoPlayerControls(VideoPlayerWindow Parent)
        //{
        //    parent = Parent;

        //    InitializeComponent();

        //    // we need the VideoView to be fully loaded before setting a MediaPlayer on it.
        //    parent.videoView.Loaded += VideoView_Loaded;
        //    PlayButton.Click += PlayButton_Click;
        //    StopButton.Click += StopButton_Click;
        //}

        //private void VideoView_Loaded(object sender, RoutedEventArgs e)
        //{
        //    _libVLC = new LibVLC();
        //    _mediaPlayer = new MediaPlayer(_libVLC);

        //    parent.videoView.MediaPlayer = _mediaPlayer;
        //}

        //void StopButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (parent.videoView.MediaPlayer.IsPlaying)
        //    {
        //        parent.videoView.MediaPlayer.Stop();
        //    }
        //}

        //void PlayButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!parent.videoView.MediaPlayer.IsPlaying)
        //    {
        //        var uri = new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4");
        //        parent.videoView.MediaPlayer.Play(new Media(_libVLC, uri));
        //    }
        //}

    }
}
