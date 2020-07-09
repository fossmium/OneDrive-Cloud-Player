using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using OneDrive_Cloud_Player.Models.GraphData;
using OneDrive_Cloud_Player.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OneDrive_Cloud_Player.Services
{
	public class CacheHelper
	{
		GraphHelper Graph { get; set; }

		public string CurrentUserId { get; private set; }

		public OneDriveCache CurrentUserCache { get; private set; }

		public List<OneDriveCache> Cache { get; private set; }

		public CacheHelper()
		{
			Cache = new List<OneDriveCache>();
			Graph = new GraphHelper();
		}

		/// <summary>
		/// This method initializes the cache depending on whether or not the user has already logged in or not
		/// </summary>
		/// <param name="HasAlreadyLoggedIn">This value indicates whether or not the user should use or has already used the Login Window.
		/// This could be false in the following circumstances: 1) The user credentials are cached and the user does not have to authenticate again,
		/// or 2) the user has clicked on the logout button and has therefore already logged in.</param>
		/// <returns></returns>
		public async Task Initialize(bool HasAlreadyLoggedIn)
		{
			// Update the current user id.
			await SetCurrentUserId();

			// Check whether or not the user has already logged in. If not, we need to read cache from disk.
			if (!HasAlreadyLoggedIn)
			{
				ReadGraphCache();
			}
			
			// Check whether or not we succesfully loaded cache from disk.
			if (Cache.Count == 0)
			{
				// Now we need to create the user cache
				CreateEmptyUserCache(CurrentUserId);
			}
			else
			{
				// The Cache.Count is always not null if the cache is not read from disk.
				SetCurrentCache(CurrentUserId);
			}
			Console.WriteLine(" + Cache initialized");
		}

		public async Task SetCurrentUserId()
		{
			CurrentUserId = (await App.Current.PublicClientApplication.GetAccountsAsync()).
				FirstOrDefault<IAccount>().HomeAccountId.ObjectId.ToString();
		}

		/// <summary>
		/// Update the current user cache by looking through the loaded cache
		/// </summary>
		/// <param name="CurrentUserId"></param>
		public void SetCurrentCache(string CurrentUserId)
		{
			CurrentUserCache = null;
			using (IEnumerator<OneDriveCache> Enumerator = Cache.GetEnumerator())
			{
				bool FoundCurrentUser = false;
				while (!FoundCurrentUser && Enumerator.MoveNext())
				{
					OneDriveCache UserCache = Enumerator.Current;
					if (UserCache.UserId.Equals(CurrentUserId))
					{
						CurrentUserCache = UserCache;
						FoundCurrentUser = true;
					}
				}
			}
			if (CurrentUserCache is null)
			{
				// the current user has not been found in the cache, so now we need to create a user cache
				CreateEmptyUserCache(CurrentUserId);
			}
		}

		/// <summary>
		/// Create a brand new user cache and add it to the cache list
		/// </summary>
		/// <param name="CurrentUserId"></param>
		public void CreateEmptyUserCache(string CurrentUserId)
		{
			CurrentUserCache = new OneDriveCache(CurrentUserId);
			Cache.Add(CurrentUserCache);
		}

		/// <summary>
		/// Called upon logout to clear the current user cache and id
		/// </summary>
		public void ResetCache()
		{
			CurrentUserCache = null;
			CurrentUserId = null;
		}

		/// <summary>
		/// Deserialize a JSON string loaded from disk to set the cache.
		/// Can be null in case the cache does not exist on disk.
		/// </summary>
		public void ReadGraphCache()
		{
			string JsonGraphCache = IO.JsonHandler.ReadJson("graphcache.json");
			// check for non-existant file
			if (JsonGraphCache != null)
			{
				Cache = JsonConvert.DeserializeObject<List<OneDriveCache>>(JsonGraphCache);
			}
		}

		/// <summary>
		/// Save the cache in it's current state by serializing the current cache and writing it to the disk.
		/// </summary>
		public void WriteGraphCache()
		{
			new Thread(() =>
			{
				string JsonToWrite = JsonConvert.SerializeObject(Cache, Formatting.None);
				IO.JsonHandler.WriteJson(JsonToWrite, "graphcache.json");
			}).Start();	
		}

		static readonly object CacheLock = new object();

		/// <summary>
		/// First check if there is cache. If so, load that. If not, request data from Graph.
		/// In any case, the cache is freshly updated on a new thread after the data has been fetched,
		/// so the next time it won't have to call Graph. For Drives, this won't have a big impact,
		/// but DriveItem loading might benefit significantly from caching.
		/// </summary>
		/// <returns></returns>
		public async Task<List<CachedDrive>> GetDrives()
		{
			List<CachedDrive> drives = null;
			bool HasCache = true;
			lock (CacheLock)
			{
				HasCache = CurrentUserCache.Drives.Count != 0;
			}
			if (HasCache)
			{
				lock (CacheLock)
				{
					drives = CurrentUserCache.Drives;
				}
			}
			else
			{
				drives = await GetDrivesFromGraph();
				lock (CacheLock)
				{
					CurrentUserCache.Drives = drives;
				}
			}
			new Thread(async () =>
			{
				await UpdateDriveCache();
			}).Start();
			return drives;
		}

		public async Task<List<CachedDrive>> GetDrivesFromGraph()
		{
			// Compile a list of drives, straight from Graph
			List<DriveItem> localDriveList = new List<DriveItem>();
			localDriveList.Add(await Graph.GetUserRootDrive());

			IDriveSharedWithMeCollectionPage sharedDrivesCollection = await Graph.GetSharedDrivesAsync();
			foreach (DriveItem drive in sharedDrivesCollection)
			{
				if (drive.Folder != null)
				{
					localDriveList.Add(drive);
				}
			}

			// Convert the Graph format
			string CurrentUsername = (await Graph.GetOneDriveUserInformationAsync()).DisplayName;
			List<CachedDrive> newlyCachedDrives = new List<CachedDrive>();
			foreach (DriveItem graphDrive in localDriveList)
			{
				CachedDrive driveToAdd = new CachedDrive
				{
					Id = graphDrive.Id,
					ChildrenCount = graphDrive.Folder.ChildCount
				};
				// Check if the current graphDrive in the localDriveList is the personal graphDrive
				if (graphDrive.RemoteItem is null)
				{
					// this is the personal drive
					driveToAdd.DriveName = "Your Drive";
					driveToAdd.DriveId = graphDrive.ParentReference.DriveId;
					driveToAdd.IsSharedFolder = false;
					driveToAdd.OwnerName = CurrentUsername;
				}
				else
				{
					driveToAdd.DriveName = graphDrive.Name;
					driveToAdd.DriveId = graphDrive.RemoteItem.ParentReference.DriveId;
					driveToAdd.IsSharedFolder = true;
					driveToAdd.OwnerName = graphDrive.RemoteItem.Shared.SharedBy.User.DisplayName;
				}
				newlyCachedDrives.Add(driveToAdd);
			}
			return newlyCachedDrives;
		}

		public async Task UpdateDriveCache()
		{
			List<CachedDrive> UpdatedDrives = await GetDrivesFromGraph();
			// We need to restore the ItemList from the current cache, since otherwise we overwrite the current cache with drives that contain an empty ItemList.
			// Graph doesn't return item information here.
			Dictionary<string, List<CachedDriveItem>> drivesCombinedWithItems = new Dictionary<string, List<CachedDriveItem>>();
			lock (CacheLock)
			{
				CurrentUserCache.Drives.ForEach((currentDrive) =>
				{
					string key = currentDrive.DriveId + currentDrive.Id;
					drivesCombinedWithItems.Add(key, currentDrive.ItemList);
				});

				// Match the drive's DriveId and Id to a list wiht items belonging to that key, so the list of cached items is not throwed away.
				UpdatedDrives.ForEach((currentUpdatedDrive) =>
				{
					string key = currentUpdatedDrive.DriveId + currentUpdatedDrive.Id;
					if (drivesCombinedWithItems.ContainsKey(key))
					{
						List<CachedDriveItem> listToRestore = drivesCombinedWithItems.GetValueOrDefault(key);
						currentUpdatedDrive.ItemList = listToRestore;
						//currentUpdatedDrive.ChildrenCount = listToRestore.Count;
						// TODO: get childcount correct
					}
				});


				CurrentUserCache.Drives = UpdatedDrives;
			}
		}

		public async Task<List<CachedDriveItem>> GetCachedChildrenFromDrive(string SelectedDriveId, string ItemId)
		{
			List<CachedDriveItem> itemsToReturn = null;
			// Search for CachedDriveItems in the CachedDrive with the specified DriveId and ItemId
			try
			{
				CachedDrive DriveToSearch = null;
				IEnumerable<CachedDriveItem> ChildrenFromDrive = null;
				lock (CacheLock)
				{
					DriveToSearch = CurrentUserCache.Drives.First(drive => drive.DriveId.Equals(SelectedDriveId) && drive.Id.Equals(ItemId));
					// Get all items with a ParentItemId equal to the 2nd argument ItemId, so the direct children of a drive
					ChildrenFromDrive = DriveToSearch.ItemList.Where(item => item.ParentItemId.Equals(ItemId));
				}
				bool HasNoChildrenCache = false;
				lock (CacheLock)
				{
					HasNoChildrenCache = !ChildrenFromDrive.Any();
				}
				if (HasNoChildrenCache)
				{
					// Found no children from the specified drive in cache, so we need to fetch the latest from Graph
					itemsToReturn = await GetDriveChildrenFromGraph(SelectedDriveId, ItemId);
					lock (CacheLock)
					{
						// Since there are no items with a ParentItemId set to the drive, there cannot be any other items,
						// since they would be eventually be children of items which are children of the drive
						DriveToSearch.ItemList = itemsToReturn;
					}
				}
				else
				{
					lock (CacheLock)
					{
						itemsToReturn = ChildrenFromDrive.ToList();
					}
				}
				// We need to update the cache here on another thread without potentially destroying other saved items,
				// so we can't just overwrite the ItemList
				new Thread(async () =>
				{
					await UpdateDriveChildrenCache(SelectedDriveId, ItemId);
				}).Start();
				return itemsToReturn;
			}
			catch (InvalidOperationException)
			{
				// If there is no Drive witht the specified driveId, an InvalidOperationException will be thrown.
				// This means the user clicked on a drive in cache (but this cache was updated at startup)
				return null;
			}
		}

		public async Task<List<CachedDriveItem>> GetDriveChildrenFromGraph(string SelectedDriveId, string ItemId)
		{
			List<DriveItem> ItemsFromGraph = (await Graph.GetChildrenOfItemAsync(SelectedDriveId, ItemId)).ToList();
			// Convert Graph DriveItems to CachedDriveItems
			List<CachedDriveItem> DriveChildren = new List<CachedDriveItem>();
			ItemsFromGraph.ForEach((graphDriveItem) =>
			{
				CachedDriveItem ConvertedItem = this.ConvertGraphItem(graphDriveItem);
				if (ConvertedItem != null)
				{
					ConvertedItem.ParentItemId = ItemId;
					DriveChildren.Add(ConvertedItem);
				}
			});

			return DriveChildren;
		}

        public async Task UpdateDriveChildrenCache(string SelectedDriveId, string ItemId)
        {
            List<CachedDriveItem> NewlyFecthedItems = await GetDriveChildrenFromGraph(SelectedDriveId, ItemId);
			List<CachedDriveItem> CurrentlyCachedDriveItems = null;
			lock (CacheLock)
			{
				CurrentlyCachedDriveItems = CurrentUserCache.Drives.First(drive => drive.DriveId.Equals(SelectedDriveId) && drive.Id.Equals(ItemId)).ItemList;

				List<CachedDriveItem> itemsToRemove = new List<CachedDriveItem>();

				// Remove all cached items with the same ItemId
				// Now add all newly fetched items
				CurrentlyCachedDriveItems.ForEach((currentItem) =>
				{
					if (currentItem.ParentItemId.Equals(ItemId))
					{
						itemsToRemove.Add(currentItem);
					}
				});

				// remove all the items here
				itemsToRemove.ForEach((currentItem) =>
				{
					CurrentlyCachedDriveItems.Remove(currentItem);
				});

				CurrentlyCachedDriveItems.AddRange(NewlyFecthedItems);
			}
		}

		public async Task<List<CachedDriveItem>> GetCachedChildrenFromItem(CachedDrive SelectedDrive, string ItemId)
		{
			List<CachedDriveItem> CurrentlyCachedItems = null;
			List<CachedDriveItem> ChildrenFromItem = null;
			lock (CacheLock)
			{
				CurrentlyCachedItems = CurrentUserCache.Drives.First((currentItem) =>
					currentItem.DriveId.Equals(SelectedDrive.DriveId) && currentItem.Id.Equals(SelectedDrive.Id)
				).ItemList;

				ChildrenFromItem = new List<CachedDriveItem>();

				// There are children, so we need to search the ItemList for them by comparing the ParentItemId and ItemId
				CurrentlyCachedItems.ForEach((currentItem) =>
				{
					if (currentItem.ParentItemId.Equals(ItemId))
					{
						ChildrenFromItem.Add(currentItem);
					}
				});
			}

			if (ChildrenFromItem.Count == 0)
			{
				// There were no items with the correct ParentItemId, so we need to call Graph
				ChildrenFromItem = await GetItemChildrenFromGraph(SelectedDrive.DriveId, ItemId);
				// check for non-existant folder
				if (ChildrenFromItem is null)
				{
					ChildrenFromItem = new List<CachedDriveItem>();
				}
			}
			// Now update the cache on another thread.
			new Thread(async () =>
			{
				await UpdateItemChildrenCache(SelectedDrive.DriveId, SelectedDrive.Id, ItemId);
			}).Start();
			return ChildrenFromItem;
		}

		public async Task<List<CachedDriveItem>> GetItemChildrenFromGraph(string DriveId, string ItemId)
		{
			List<DriveItem> ItemsFromGraph = (await Graph.GetChildrenOfItemAsync(DriveId, ItemId)).ToList();

			List<CachedDriveItem> ChildrenFromItem = new List<CachedDriveItem>();

			ItemsFromGraph.ForEach((graphItem) =>
			{
				CachedDriveItem ConvertedItem = this.ConvertGraphItem(graphItem);
				if (ConvertedItem != null)
				{
					ConvertedItem.ParentItemId = graphItem.ParentReference.Id;
					ChildrenFromItem.Add(ConvertedItem);
				}
			});

			return ChildrenFromItem;
		}

		public async Task UpdateItemChildrenCache(string DriveId, string DriveItemId, string ItemId)
		{
			List<CachedDriveItem> ItemsFromGraph = await GetItemChildrenFromGraph(DriveId, ItemId);

			lock (CacheLock)
			{
				List <CachedDriveItem> ItemsInCache = CurrentUserCache.Drives.First((currentItem) => currentItem.DriveId.Equals(DriveId) && currentItem.Id.Equals(DriveItemId)).ItemList;

				// Create a list of all items in cache to remove, so we can simply append the items from Graph
				List<CachedDriveItem> ItemsToRemove = new List<CachedDriveItem>();

				// Match the ParentItemIds of items in cache to the ItemId of the item of which we want to get children
				ItemsInCache.ForEach((currentItem) =>
				{
					if (currentItem.ParentItemId.Equals(ItemId))
					{
						ItemsToRemove.Add(currentItem);
					}
				});

				ItemsToRemove.ForEach((itemToRemove) =>
				{
					ItemsInCache.Remove(itemToRemove);
				});

				ItemsInCache.AddRange(ItemsFromGraph);
			}
		}

		public CachedDriveItem ConvertGraphItem(DriveItem GraphItem)
		{
			// Check which items to convert: either folders, or files with the correct mimetype (either contains audio or video).
			if (GraphItem.Folder != null || (GraphItem.File != null && (GraphItem.File.MimeType.Contains("video") || GraphItem.File.MimeType.Contains("audio"))))
			{
				CachedDriveItem ConvertedItem = new CachedDriveItem
				{
					ItemId = GraphItem.Id,
					IsFolder = GraphItem.Folder != null,
					Name = GraphItem.Name,
					Size = GraphItem.Size ?? 0
				};
				if (ConvertedItem.IsFolder)
				{
					ConvertedItem.ChildCount = GraphItem.Folder.ChildCount;
				}
				else
				{
					// GraphItem.File won't be null since we checked that beforehand
					ConvertedItem.MimeType = GraphItem.File.MimeType;
				}
				return ConvertedItem;
			}
			else
			{
				// Don't return items which are not folders or cannot be played by libVLC.
				return null;
			}
		}

		public List<CachedDriveItem> GetDriveOrItemsWithParentId(CachedDrive SelectedDriveFolder, string ParentId)
		{
			// We need to get the parent of the current ParentItem. If we have that parent, we can get it's children.
			// That parent is either a CachedDrive or a CachedDriveItem
			lock (CacheLock)
			{
				try
				{
					// check to see if the SelectedDriveFolder is the parent of the current ParentItem
					CachedDrive CurrentDrive = CurrentUserCache.Drives.First((currentDrive) => SelectedDriveFolder.DriveId.Equals(currentDrive.DriveId) && SelectedDriveFolder.Id.Equals(currentDrive.Id));
					List<CachedDriveItem> ParentItems = new List<CachedDriveItem>();
					//if (CurrentDrive.Id.Equals(ParentId))
					//{
						CurrentDrive.ItemList.ForEach((currentItem) =>
						{
							if (currentItem.ParentItemId.Equals(ParentId))
							{
								ParentItems.Add(currentItem);
							}
						});
					//}
					//else
					//{
						//The parent of the current ParentItem is not a CachedDrive, so it must be a CachedDriveItem
					//	CurrentDrive.ItemList.ForEach((currentItem) =>
					//	{
					//		if (currentItem.ItemId.Equals(ParentId))
					//		{
					//			ParentItems.Add(currentItem);
					//		}
					//	});
					//}
					return ParentItems;
				}
				catch (InvalidOperationException)
				{
					return null;
				}
			}
		}

		public bool IsParentChildOfDrive(CachedDrive SelectedDriveFolder, string ParentId)
		{
			lock (CacheLock)
			{
				try
				{
					CachedDrive CurrentDrive = CurrentUserCache.Drives.First((currentDrive) => SelectedDriveFolder.DriveId.Equals(currentDrive.DriveId) && SelectedDriveFolder.Id.Equals(ParentId));
					return true;
				}
				catch (InvalidOperationException)
				{
					return false;
				}
			}
		}

		public CachedDriveItem GetParentItemByParentItemId(CachedDrive SelectedDriveFolder, string ParentId)
		{
			lock (CacheLock)
			{
				try
				{
					// Get the current drive from the cache
					CachedDrive CurrentDrive = CurrentUserCache.Drives.First((currentDrive) => currentDrive.DriveId.Equals(SelectedDriveFolder.DriveId) && currentDrive.Id.Equals(SelectedDriveFolder.Id));
					CachedDriveItem Parent = CurrentDrive.ItemList.First((currentItem) => currentItem.ItemId.Equals(ParentId));
					return Parent;
				}
				catch (InvalidOperationException)
				{
					// This should not happen.
					return null;
				}
			}
		}

	}
}
