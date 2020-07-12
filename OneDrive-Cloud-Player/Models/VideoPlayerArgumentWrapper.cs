using OneDrive_Cloud_Player.Models.GraphData;

namespace OneDrive_Cloud_Player.Models
{
    /// <summary>
    /// Contains a <see cref="GraphData.CachedDriveItem"/> and a string that is the drive id.
    /// </summary>
    public class VideoPlayerArgumentWrapper
    {
        public readonly CachedDriveItem CachedDriveItem;
        public readonly string DriveId;

        public VideoPlayerArgumentWrapper(CachedDriveItem CachedDriveItem, string DriveId)
        {
            this.CachedDriveItem = CachedDriveItem;
            this.DriveId = DriveId;
        }
    }
}
