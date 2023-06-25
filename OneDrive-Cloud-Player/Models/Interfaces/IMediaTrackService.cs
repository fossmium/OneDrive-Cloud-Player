using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using System.Threading.Tasks;

namespace OneDrive_Cloud_Player.Models.Interfaces
{
    public interface IMediaTrackService
    {
        /// <summary>
        /// Needs to be called before using the service and before subscribing to other libvlcsharp events.<br />
        /// Due to LibVLCSharp's inability for the MediaPlayer to be put into a service, we need to get a reference to the MediaPlayer ourselves instead.<br />
        /// Initialize this service by parsing the MediaPlayer object reference for its media tracks.
        /// </summary>
        /// <param name="mediaPlayer"></param>
        /// <returns></returns>
        IMediaTrackService Initialize(ref MediaPlayer mediaPlayer);

        /// <summary>
        /// Get the preferred subtitle track. When no preferred subtitle track is found, a default will be returned.
        /// <br />
        /// <i>Note: This method should not be called on a LibVLC thread.
        /// <see href="https://code.videolan.org/videolan/LibVLCSharp/-/blob/3.x/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first">Here</see> is more info on why.</i>
        /// </summary>
        /// <returns><see cref="TrackDescription"/> Preferred subtitle track or default</returns>
        /// <exception cref="ServiceNotInitializedException"></exception>
        TrackDescription GetPreferredSubtitleTrack();

        /// <summary>
        /// Get the embedded subtitle tracks in the media file.
        /// <br />
        /// <i>Note: This method should not be called on a LibVLC thread.
        /// <see href="https://code.videolan.org/videolan/LibVLCSharp/-/blob/3.x/docs/best_practices.md#do-not-call-libvlc-from-a-libvlc-event-without-switching-thread-first">Here</see> is more info on why.</i>
        /// </summary>
        /// <returns>Array containing <see cref="TrackDescription"/>'s or empty when no tracks are available</returns>
        TrackDescription[] GetEmbeddedSubtitleTracks();
    }
}
