using LibVLCSharp.Shared.Structures;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OneDrive_Cloud_Player.Views
{
    public sealed partial class SubtitleAudioTrackSelector : UserControl, INotifyPropertyChanged
    {
        public event EventHandler ItemClicked;

        private bool _isSubtitlesAvailable = false;
        public bool IsSubtitlesAvailable
        {
            get { return _isSubtitlesAvailable; }
            set
            {
                _isSubtitlesAvailable = value;
                OnPropertyChanged(nameof(IsSubtitlesAvailable));
            }
        }

        public ObservableCollection<TrackDescription> SubtitleTracks
        {
            get { return (ObservableCollection<TrackDescription>)GetValue(SubtitleTracksProperty); }
            set { SetValue(SubtitleTracksProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SubtitleTracks. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubtitleTracksProperty =
            DependencyProperty.Register("SubtitleTracks", typeof(ObservableCollection<TrackDescription>), typeof(SubtitleAudioTrackSelector), new PropertyMetadata(null, SubtitleTracksPropertyChangedCallback));

        public TrackDescription SelectedSubtitleTrack
        {
            get { return (TrackDescription)GetValue(SelectedSubtitleTrackProperty); }
            set { SetValue(SelectedSubtitleTrackProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedTrack. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedSubtitleTrackProperty =
            DependencyProperty.Register("SelectedSubtitleTrack", typeof(TrackDescription), typeof(SubtitleAudioTrackSelector), new PropertyMetadata(null));

        public SubtitleAudioTrackSelector()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Callback for SubtitleTracksProperty property changes.
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="eventArgs"></param>
        private static void SubtitleTracksPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            SubtitleAudioTrackSelector subtitleAudioTrackSelector = (SubtitleAudioTrackSelector)dependencyObject;
            if (((ObservableCollection<TrackDescription>)eventArgs.NewValue).Count > 0)
            {
                subtitleAudioTrackSelector.IsSubtitlesAvailable = true;
            }

            if (eventArgs.NewValue is INotifyCollectionChanged newCollection)
            {
                // Subscribe to changes inside the new collection.
                newCollection.CollectionChanged += subtitleAudioTrackSelector.SubtitleTracks_CollectionChanged;
            }

            if (eventArgs.OldValue is INotifyCollectionChanged oldCollection)
            {
                // Unsubscribe to changes inside the old collection.
                oldCollection.CollectionChanged -= subtitleAudioTrackSelector.SubtitleTracks_CollectionChanged;
            }
        }

        /// <summary>
        /// Hnadling changes that occur inside the subtitle tracks collection (i.e. added or removed).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubtitleTracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Check for Default value.
            if (e.NewItems == null)
            {
                IsSubtitlesAvailable = false;
                return;
            }

            // Check if there are subtitles.
            if (e.NewItems.Count <= 0)
            {
                IsSubtitlesAvailable = false;
                return;
            }

            IsSubtitlesAvailable = true;
        }

        /// <summary>
        /// Invoke the custom <see cref="ItemClicked"/> event when a subtitle item in the listview is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubtitleTrackListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("Subtitle item clicked from listview.");
            ItemClicked?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// When called, notifies the UI that the property value has changed.
        /// </summary>
        /// <param name="prop">Name of the property which value has changed</param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
