using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using OneDrive_Cloud_Player.Models.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;

namespace OneDrive_Cloud_Player.Services
{
    /// <summary>
    /// Manage the media tracks of the libvlcsharp media player instance.
    /// </summary>
    public class MediaTrackService : IMediaTrackService
    {
        private MediaPlayer _mediaPlayer;
        private bool _isInitialized = false;
        private readonly ApplicationDataContainer _userSettings;

        public MediaTrackService()
        {
            _userSettings = ApplicationData.Current.LocalSettings;
        }

        /// <summary>
        /// Due to LibVLCSharp's inability for the MediaPlayer to be put into a service, we need to get a reference to the MediaPlayer ourselves instead.
        /// Initialize this service by parsing the MediaPlayer object reference for its media tracks.
        /// </summary>
        public IMediaTrackService Initialize(ref MediaPlayer mediaPlayer)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("Service already initialized!");
            }

            Debug.WriteLine(String.Format("initializing {0}", GetType().Name));
            _mediaPlayer = mediaPlayer;
            _isInitialized = true;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void CheckInitializationState()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Service not initialized!");
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns> <inheritdoc/></returns>
        public TrackDescription GetPreferredSubtitleTrack()
        {
            CheckInitializationState();
            TrackDescription[] subtitleTracks = _mediaPlayer.SpuDescription;

            if (_mediaPlayer.SpuCount <= 1)
            {
                return default;
            }

            // Return default track subtitle (when possible, like in mkv) depending on the user setting.
            if ((bool)_userSettings.Values["ShowDefaultSubtitles"])
            {
                return subtitleTracks.First(subtitleTrack => subtitleTrack.Id == _mediaPlayer.Spu);
            }
            else
            {
                // Return the "disabled" indicator track positioned at index 0.
                return _mediaPlayer.SpuDescription.ElementAt(0);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public TrackDescription[] GetEmbeddedSubtitleTracks()
        {
            CheckInitializationState();
            return _mediaPlayer.SpuDescription;
        }

        public TrackDescription GetPreferredAudioTrack()
        {
            CheckInitializationState();
            TrackDescription[] audioTracks = _mediaPlayer.AudioTrackDescription;

            if (_mediaPlayer.AudioTrackCount <= 1)
            {
                return default;
            }

            // Return the first actual audio track.
            return _mediaPlayer.AudioTrackDescription.ElementAt(1);
        }

        public TrackDescription[] GetEmbeddedAudioTracks()
        {
            CheckInitializationState();
            return _mediaPlayer.AudioTrackDescription;
        }
    }
}
