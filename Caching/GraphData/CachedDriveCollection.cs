using Newtonsoft.Json;
using System.Collections.Generic;

namespace OneDrive_Cloud_Player.Caching.GraphData
{
	/// <summary>
	/// This class represents a cached drive. This can either be your own OneDrive folder, or a shared folder.
	/// </summary>
	class CachedDriveCollection
	{
		/// <summary>
		/// The id of the drive, used to uniquely identify a drive
		/// </summary>
		[JsonProperty("driveId")]
		public string DriveId { get; private set; }

		/// <summary>
		/// The name of the drive
		/// </summary>
		[JsonProperty("driveName")]
		public string DriveName { get; private set; }

		/// <summary>
		/// The name of the owner of this drive
		/// </summary>
		[JsonProperty("ownerName")]
		public string OwnerName { get; private set; }

		/// <summary>
		/// All the items stored in this drive
		/// </summary>
		[JsonProperty("items")]
		public List<CachedDriveItem> ItemList { get; private set; }

	}
}
