using Microsoft.Graph;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OneDrive_Cloud_Player.API
{
    /// <summary>
    /// Used to create calls and retrieve information from the graph api.
    /// Try to save this data and use this class as little as possible for speed purposes.
    /// </summary>
    class GraphHandler
    {
        private GraphServiceClient GraphClient { get; set; }

        private AuthenticationHandler Auth { get; set; }

        public GraphHandler()
        {
            Auth = new AuthenticationHandler();
        }

        /// <summary>
        ///    Create the graph client with including acquiring an access token used to call the microsoft graph api.
        /// </summary>
        private async Task CreateGraphClientAsync()
        {
            //Acquire accesstoken.
            string AccessToken = await Auth.GetAccessToken();

            GraphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                return Task.FromResult(0);
            }));
        }

        /// <summary>
        /// Get the information of the OneDrive.
        /// </summary>
        /// <param name="DriveId"></param>
        /// <returns></returns>
        public async Task<Drive> GetDriveInformationAsync(string DriveId = null)
        {
            //Create a new GraphServiceClient.
            await CreateGraphClientAsync();
            //Return the name of the drive of type string. Return caller information when DriveId is not filled in.
            if (DriveId == null)
            {
                return await GraphClient.Me.Drive.Request().GetAsync();
            }
            else
            {
                return await GraphClient.Me.Drives[DriveId].Request().GetAsync();
            }
        }

        /// <summary>
        /// Get the information of the owner of the onedrive.
        /// </summary>
        /// <returns></returns>
        public async Task<User> GetOneDriveOwnerInformationAsync()
        {
            //Create a new GraphServiceClient.
            await CreateGraphClientAsync();
            //Return the user information in type Microsoft.Graph.User.
            return await GraphClient.Me.Request().GetAsync();
        }

        /// <summary>
        /// Get the children that are inside a drive item.
        /// </summary>
        /// <param name="ItemId"></param>
        /// <param name="DriveId"></param>
        /// <returns></returns>
        public async Task<IDriveItemChildrenCollectionPage> GetChildrenOfItemAsync(string DriveId, string ItemId)
        {
            //Create a new GraphServiceClient.
            await CreateGraphClientAsync();
            //Return the children of the item on the given drive.
            return await GraphClient.Me.Drives[DriveId].Items[ItemId].Children.Request().GetAsync();
        }

        /// <summary>
        /// Gets the root item inside the user drive.
        /// </summary>
        /// <returns></returns>
        public async Task<DriveItem> GetUserRootDrive()
        {
            //Create a new GraphServiceClient.
            await CreateGraphClientAsync();

            return await GraphClient.Me.Drive.Root.Request().GetAsync();
        }

        /// <summary>
        /// Get the drives that are shared with the user.
        /// </summary>
        /// <returns></returns>
        public async Task<IDriveSharedWithMeCollectionPage> GetSharedDrivesAsync()
        {
            //Create a new GraphServiceClient.
            await CreateGraphClientAsync();
            //Return shared items.
            return await GraphClient.Me.Drive.SharedWithMe().Request().GetAsync();
        }

        /// <summary>
        /// Get the profile picture of the OneDrive owner.
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> GetOneDriveOwnerPhotoAsync()
        {
            //Create a new GraphServiceClient.
            await CreateGraphClientAsync();
            //Return photo of the owner in binary data.
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
            //Create a new GraphServiceClient.
            await CreateGraphClientAsync();
            //Return information about an item that resides in the given drive id.
            return await GraphClient.Me.Drives[DriveId].Items[ItemId].Request().GetAsync();
        }
    }
}
