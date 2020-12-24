using Newtonsoft.Json;
using System.Collections.Generic;

namespace OneDrive_Cloud_Player.Models.GraphData
{
	/// <summary>
	/// This class represents a cached drive. This can either be your own OneDrive folder, or a shared folder.
	/// </summary>
	public class CachedDrive
	{
		public CachedDrive()
		{
			ItemList = new List<CachedDriveItem>();
		}

		/// <summary>
		/// The id of the drive, used to uniquely identify a drive
		/// </summary>
		[JsonProperty("driveId")]
		public string DriveId { get; set; }

		/// <summary>
		/// This is the id of the drive. In case of a personal drive, this will be the root id.
		/// In case of a shared folder, the id of this drive will be the itemId
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; }

		/// <summary>
		/// The name of the drive
		/// </summary>
		[JsonProperty("driveName")]
		public string DriveName { get; set; }

		/// <summary>
		/// The name of the owner of this drive
		/// </summary>
		[JsonProperty("ownerName")]
		public string OwnerName { get; set; }

		/// <summary>
		/// Whether or not this drive is actually a shared folder, shared with the user by someone else
		/// </summary>
		[JsonProperty("isSharedFolder")]
		public bool IsSharedFolder { get; set; }

		[JsonProperty("childrenCount")]
		public int? ChildrenCount { get; set; }

		/// <summary>
		/// All the items stored in this drive
		/// </summary>
		[JsonProperty("items")]
		public List<CachedDriveItem> ItemList { get; set; }

	}
}
