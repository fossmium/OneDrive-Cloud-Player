using Microsoft.Graph;
using OneDrive_Cloud_Player.Services.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
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

        public GraphHelper()
        {
            Auth = new GraphAuthHelper();
            InitializeGraphHelperAsync();
        }

        /// <summary>
        /// Create the Graph client and acquire an access token used to call the Graph API.
        /// </summary>
        private async void InitializeGraphHelperAsync()
        {
            GraphClient = new GraphServiceClient(await GetNewAuthenticationHeaderAsync());
        }

        /// <summary>
        /// Update the authentication header of the GraphServiceClient with a new access token.
        /// </summary>
        private async Task RefreshAccesTokenAsync()
        {
            GraphClient.AuthenticationProvider = await GetNewAuthenticationHeaderAsync();
        }

        /// <summary>
        /// Retrieve a new access token and construct an AuthenticationHeaderValue based upon it.
        /// </summary>
        /// <returns></returns>
        private async Task<IAuthenticationProvider> GetNewAuthenticationHeaderAsync()
        {
            // Acquire accesstoken.
            string AccessToken = await Auth.GetAccessToken();

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
        /// Get the children that are inside a drive item. Rreturns null upon Graph ServiceException
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
