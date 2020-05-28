using System;
using LibVLCSharp.Shared;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OneDrive_Cloud_Player.VLC;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using LibVLCSharp.WPF;
using System.Threading;

namespace OneDrive_Cloud_Player.VLC
{
	partial class VideoPlayerWindow : Window
	{

		private MediaPlayer _mediaPlayer;
		private LibVLC _libVLC;
		private string VideoURL;
		public string ButtonTitle { set; get; }

		public VideoPlayerWindow(string VideoURL)
		{
			InitializeComponent();
			ButtonTitle = "TestPause";
			this.VideoURL = VideoURL;
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
			//we need the VideoView to be fully loaded before setting a MediaPlayer on it.
			videoView.Loaded += (sender, e) => videoView.MediaPlayer = _mediaPlayer;

			
		}

		private void PauseContinueButton_Click(object sender, RoutedEventArgs e)
		{
			VideoPlayerViewModel.PauseContinueButton(_libVLC, videoView);
		}

		private void StopButton_Click(object sender, RoutedEventArgs e)
		{
			VideoPlayerViewModel.StopButton(videoView);
		}

		protected override void OnClosed(EventArgs e)
		{
			videoView.Dispose();
		}

		private void NewVideoButton_Click(object sender, RoutedEventArgs e)
		{
			VideoPlayerViewModel.NewVideoButton(_libVLC, videoView, this.VideoURL);
		}
	}
}
