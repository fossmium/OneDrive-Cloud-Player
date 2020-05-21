using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OneDrive_Cloud_Player.API
{
    class Graph
    {
        private GraphServiceClient GraphClient { get; set; }
        private string ClientAccessToken { get; set; }

        public Graph()
        {

        }

        /// <summary>
        ///    Create the graph client used to call the microsoft graph api.
        /// </summary>
        public void CreateGraphClient(string ClientAccessToken)
        {
            GraphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("Bearer", ClientAccessToken);

                return Task.FromResult(0);
            }));
        }
    }
}
