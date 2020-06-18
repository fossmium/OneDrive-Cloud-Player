using Microsoft.Identity.Client;
using Newtonsoft.Json;
using OneDrive_Cloud_Player.API;
using OneDrive_Cloud_Player.Caching.GraphData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OneDrive_Cloud_Player.Caching
{
	public class CacheHandler
	{
		GraphHandler Graph = new GraphHandler();

		public string CurrentUserId { get; private set; }

		public OneDriveCache CurrentUserCache { get; private set; }

		public List<OneDriveCache> Cache { get; private set; }

		public CacheHandler()
		{
			Cache = new List<OneDriveCache>();
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
			if (CurrentUserCache == null)
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

		//public static async Task<Drive> GetCachedDriveInformationAsync(string DriveId)
		//{
		//	bool Found = false;
		//	foreach (CachedDrive DriveCollection in CurrentUserCache.Drives)
		//	{
		//		if (DriveCollection.DriveId.Equals(DriveId))
		//		{
		//			Found = true;
		//			// Convert the current DriveCollection to Microsoft.Graph.Drive
		//			Drive DriveInformation = new Drive();
		//			DriveItem i = new DriveItem();
		//			DriveInformation.Id = DriveId;

		//		}
		//	}
		//	if (!Found)
		//	{
		//		// The requested drive with DriveId was not found in the cache, we need to make a graph call
		//		return await Graph.GetDriveInformationAsync(DriveId);
		//	}
		//}

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
				Cache = JsonConvert.DeserializeObject<List<OneDriveCache>>(JsonGraphCache); ;
			}
		}

		/// <summary>
		/// Save the cache in it's current state by serializing the current cache and writing it to the disk.
		/// </summary>
		public void WriteGraphCache()
		{
			new Thread(() =>
			{
				string JsonToWrite = JsonConvert.SerializeObject(Cache, Formatting.Indented);
				IO.JsonHandler.WriteJson(JsonToWrite, "graphcache1.json");
			}).Start();	
		}
	}
}
