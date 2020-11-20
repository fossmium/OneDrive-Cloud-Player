using Microsoft.Graph;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OneDrive_Cloud_Player.Services.Helpers
{
    /// <summary>
    /// Used to create calls and retrieve information from the Graph API.
    /// Try to save this data and use this class as little as possible for speed purposes.
    /// </summary>
    class GraphHelper
    {
        private GraphServiceClient GraphClient { get; set; }

        private GraphAuthHelper Auth { get; set; }

        private static GraphHelper _instance = null;
        private static readonly object _instanceLock = new object();

        /// <summary>
        /// Returns the GraphHelper instance. Creates a new instance if it doesn't exist already.
        /// This method is thread-safe.
        /// </summary>
        /// <returns>GraphHelper instance</returns>
        public static GraphHelper Instance()
        {
            lock (_instanceLock)
            {
                if (_instance is null)
                {
                    _instance = new GraphHelper();
                }
            }
            return _instance;
        }

        private GraphHelper()
        {
            Auth = new GraphAuthHelper();
            InitializeGraphHelperAsync();
        }

        /// <summary>
        /// Create the Graph client.
        /// </summary>
        private void InitializeGraphHelperAsync()
        {
            // Set the access token to null to prevent crashing in case the msalcache.dat doesn't exist yet.
            GraphClient = new GraphServiceClient(GetNewAuthenticationHeaderAsync(null));
        }

        /// <summary>
        /// Retreive a new access token and update the authentication header of the GraphServiceClient.
        /// </summary>
        private async Task RefreshAccesTokenAsync()
        {
            // Acquire accesstoken.
            string AccessToken = await Auth.GetAccessToken();
            GraphClient.AuthenticationProvider = GetNewAuthenticationHeaderAsync(AccessToken);
        }

        /// <summary>
        /// Return a new AuthenticationHeaderValue based upon the specified access token.
        /// </summary>
        /// <param name="AccessToken">Access token to use with the new AuthenticationHeader</param>
        /// <returns></returns>
        private IAuthenticationProvider GetNewAuthenticationHeaderAsync(string AccessToken)
        {
            return new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Get the information of the OneDrive.
        /// </summary>
        /// <param name="DriveId"></param>
        /// <returns></returns>
        public async Task<Drive> GetDriveInformationAsync(string DriveId)
        {
            // Refresh the access token.
            await RefreshAccesTokenAsync();
            // Return the name of the drive of type string.
            return await GraphClient.Me.Drives[DriveId].Request().GetAsync();

        }

        /// <summary>
        /// Get the information of the user of the onedrive.
        /// </summary>
        /// <returns></returns>
        public async Task<User> GetOneDriveUserInformationAsync()
        {
            // Refresh the access token.
            await RefreshAccesTokenAsync();
            // Return the user information in type Microsoft.Graph.User.
            return await GraphClient.Me.Request().GetAsync();
        }

        /// <summary>
        /// Get the children that are inside a drive item. Returns null upon Graph ServiceException
        /// </summary>
        /// <param name="ItemId"></param>
        /// <param name="DriveId"></param>
        /// <returns></returns>
        public async Task<IDriveItemChildrenCollectionPage> GetChildrenOfItemAsync(string DriveId, string ItemId)
        {
            // Refresh the access token.
            await RefreshAccesTokenAsync();
            // Return the children of the item on the given drive.
            try
            {
                return await GraphClient.Me.Drives[DriveId].Items[ItemId].Children.Request().GetAsync();
            }
            catch (ServiceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the root item inside the user drive.
        /// </summary>
        /// <returns></returns>
        public async Task<DriveItem> GetUserRootDrive()
        {
            // Refresh the access token.
            await RefreshAccesTokenAsync();

            return await GraphClient.Me.Drive.Root.Request().GetAsync();
        }

        /// <summary>
        /// Get the drives that are shared with the user.
        /// </summary>
        /// <returns></returns>
        public async Task<IDriveSharedWithMeCollectionPage> GetSharedDrivesAsync()
        {
            // Refresh the access token.
            await RefreshAccesTokenAsync();
            // Return shared items.
            return await GraphClient.Me.Drive.SharedWithMe().Request().GetAsync();
        }

        /// <summary>
        /// Get the profile picture of the OneDrive owner.
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> GetOneDriveOwnerPhotoAsync()
        {
            // Refresh the access token.
            await RefreshAccesTokenAsync();
            // Return photo of the owner in binary data.
            return await GraphClient.Me.Photo.Content.Request().GetAsync();
        }

        /// <summary>
        /// Get information about a Item that is inside a given Drive.
        /// </summary>
        /// <param name="ItemId"></param>
        /// <param name="DriveId"></param>
        /// <returns></returns>
        public async Task<DriveItem> GetItemInformationAsync(string DriveId, string ItemId)
        {
            // Refresh the access token.
            await RefreshAccesTokenAsync();
            // Return information about an item that resides in the given drive id.
            return await GraphClient.Me.Drives[DriveId].Items[ItemId].Request().GetAsync();
        }
    }
}
