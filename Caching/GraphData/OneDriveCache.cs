using System.Collections.Generic;
using Newtonsoft.Json;

namespace OneDrive_Cloud_Player.Caching.GraphData
{
	class OneDriveCache
	{
		/// <summary>
		/// This represents a collection of all the drives (owned or shared) of the currently signed in user.
		/// </summary>
		public List<CachedDriveCollection> DriveList { get; private set; }

	}
}
