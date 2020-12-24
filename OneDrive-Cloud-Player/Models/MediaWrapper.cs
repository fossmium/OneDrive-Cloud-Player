using OneDrive_Cloud_Player.Models.GraphData;

namespace OneDrive_Cloud_Player.Models
{
    /// <summary>
    /// Contains a <see cref="GraphData.CachedDriveItem"/> and a string that is the drive id.
    /// </summary>
    public class MediaWrapper
    {
        public CachedDriveItem CachedDriveItem { get; set; }
        public readonly string DriveId;

        public MediaWrapper(CachedDriveItem cachedDriveItem, string driveId)
        {
            DriveId = driveId;
            CachedDriveItem = cachedDriveItem;
        }
    }
}
