using Microsoft.Graph;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace OneDrive_Cloud_Player.API
{
    /// <summary>
    /// Used to create calls and retrieve information from the graph api.
    /// Try to save this data and use this class as little as possible for speed purposes.
    /// </summary>
    class Graph
    {
        private GraphServiceClient GraphClient { get; set; }

        private Authenticate Auth { get; set; }

        public Graph()
        {
            Auth = new Authenticate();
        }

        /// <summary>
        ///    Create the graph client with including acquiring an access token used to call the microsoft graph api.
        /// </summary>
        private async Task CreateGraphClientAsync()
        {
            //Acquire accesstoken.
            string AccessToken = await Auth.AcquireAccessToken();

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
        public async Task<IDriveItemChildrenCollectionPage> GetChildrenOfItem(string ItemId, string DriveId = null)
        {
            //Create a new GraphServiceClient.
            await CreateGraphClientAsync();
            //Return the children of the item on the given drive. When no DriveId is given it uses the personal drive to search for the item.
            if (DriveId == null)
            {
                return await GraphClient.Me.Drive.Items[ItemId].Children.Request().GetAsync();
            }
            else
            {
                return await GraphClient.Me.Drives[DriveId].Items[ItemId].Children.Request().GetAsync();
            }

        }

        /// <summary>
        /// Get the items that are shared with the user.
        /// </summary>
        /// <returns></returns>
        public async Task<IDriveSharedWithMeCollectionPage> GetSharedItemsAsync()
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
    }
}
