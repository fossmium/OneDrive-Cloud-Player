using System.Collections.Generic;
using Newtonsoft.Json;

namespace OneDrive_Cloud_Player.Models.GraphData
{
	/// <summary>
	/// This class represents the OneDrive cache of a user, as such it has a user id and a drive collection
	/// </summary>
	public class OneDriveCache
	{
		public OneDriveCache(string UserId)
		{
			Drives = new List<CachedDrive>();
			this.UserId = UserId;
		}

		/// <summary>
		/// This is the unique id used to identify an account
		/// </summary>
		[JsonProperty("userId")]
		public string UserId { get; set; }

		/// <summary>
		/// This represents a collection of all the drives (owned or shared) of the currently signed in user.
		/// </summary>
		[JsonProperty("drives")]
		public List<CachedDrive> Drives { get; set; }

	}
}
