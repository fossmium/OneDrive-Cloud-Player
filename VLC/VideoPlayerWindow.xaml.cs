using System;
using LibVLCSharp.Shared;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OneDrive_Cloud_Player.VLC;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using LibVLCSharp.WPF;

namespace OneDrive_Cloud_Player.VLC
{
	partial class VideoPlayerWindow : Window
	{

		private MediaPlayer _mediaPlayer;
		private LibVLC _libVLC;

		public VideoPlayerWindow()
		{

			InitializeComponent();
			var label = new Label
			{
				Content = "TEwdsdasST",
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Bottom,
				Foreground = new SolidColorBrush(Colors.Red)
			};
			test.Children.Add(label);

			Core.Initialize();

			_libVLC = new LibVLC();
			_mediaPlayer = new MediaPlayer(_libVLC);
			// we need the VideoView to be fully loaded before setting a MediaPlayer on it.
			videoView.Loaded += (sender, e) => videoView.MediaPlayer = _mediaPlayer;
		}


		void StopButton_Click(object sender, RoutedEventArgs e)
		{
			if (videoView.MediaPlayer.IsPlaying)
			{
				videoView.MediaPlayer.Stop();
			}
		}

		void PlayButton_Click(object sender, RoutedEventArgs e)
		{
			if (!videoView.MediaPlayer.IsPlaying)
			{
				videoView.MediaPlayer.Play(new Media(_libVLC,
					"http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4", FromType.FromLocation));
			}
		}
		protected override void OnClosed(EventArgs e)
		{
			videoView.Dispose();
		}


	}
}
