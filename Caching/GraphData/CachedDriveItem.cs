using Newtonsoft.Json;

namespace OneDrive_Cloud_Player.Caching.GraphData
{
	/// <summary>
	/// This class represents a cached drive item. This can either be a folder or an item.
	/// </summary>
	class CachedDriveItem
	{
		/// <summary>
		/// This id is used to uniquely identidy an item.
		/// </summary>
		[JsonProperty("itemId")]
		public string ItemId { get; set; }

		/// <summary>
		/// This id will reference the id of another item in case this item has a parent. If this item does not have a parent, this id will be null.
		/// </summary>
		[JsonProperty("parentId")]
		public string ParentId { get; private set; }

		/// <summary>
		/// This boolean indicates whether or not this item is a folder.
		/// </summary>
		[JsonProperty("isFolder")]
		public bool IsFolder { get; private set; }

		/// <summary>
		/// This is the name of the item. In case of a folder, it will be the folder name. In case of a file, it will be the file name.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; private set; }
	}
}
