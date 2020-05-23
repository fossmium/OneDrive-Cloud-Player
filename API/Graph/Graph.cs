using Microsoft.Graph;
using OneDrive_Cloud_Player.DataStructure;
using OneDrive_Cloud_Player.Login;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
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
        public async Task<Drive> GetDriveInformationAsync(string DriveId)
        {
            //Create a new GraphServiceClient.
            await CreateGraphClientAsync();
            //Return the name of the drive of type string.
            return await GraphClient.Me.Drives[DriveId].Request().GetAsync();
        }

        /// <summary>
        /// Get the information of the owner of the onedrive. ex: jobTitle, givenName and mail. It returns a <c>User</c> type.
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
        /// Get the children that are inside the item.
        /// </summary>
        /// <returns></returns>
        public async Task<IDriveItemChildrenCollectionPage> GetChildrenOfItem(string ItemId)
        {
            //Create a new GraphServiceClient.
            await CreateGraphClientAsync();
            //Return the children of the item.
            return await GraphClient.Me.Drive.Items[ItemId].Children.Request().GetAsync();
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
