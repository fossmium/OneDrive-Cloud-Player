
using Microsoft.Identity.Client;

using System.Windows;


namespace OneDrive_Cloud_Player
{
    public partial class InitProgram
    {
        public InitProgram()
        {
            CreateScopedPublicClientApplicationInstance();
        }

        public static new InitProgram Current => (InitProgram)Application.Current;
        public IPublicClientApplication PublicClientApplication { get; private set; }
        public string[] Scopes { get; private set; }

        /// <summary>
        /// Create a plublic client application instance and set it to the PublicClientApplication property.
        /// </summary>
        private void CreateScopedPublicClientApplicationInstance()
        {
            PublicClientApplication = PublicClientApplicationBuilder.Create("cfc49d19-b88e-4986-8862-8b5de253d0fd")
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                .Build();
            Scopes = new[]
                {
                    "offline_access",
                    "openid",
                    "profile",
                    "user.read",
                    "Files.Read.All"
                };
        }
    }
}
