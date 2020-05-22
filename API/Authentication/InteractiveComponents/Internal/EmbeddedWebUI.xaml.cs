using Microsoft.Identity.Client.Extensibility;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Navigation;

namespace OneDrive_Cloud_Player.API.Authentication.InteractiveComponents.Internal
{
    public partial class EmbeddedWebUI : Window
    {
        private readonly Uri _authorizationUri;
        private readonly Uri _redirectUri;
        private readonly TaskCompletionSource<Uri> _taskCompletionSource;
        private readonly CancellationToken _cancellationToken;
        private CancellationTokenRegistration _token;

        public EmbeddedWebUI(
            Uri authorizationUri,
            Uri redirectUri,
            TaskCompletionSource<Uri> taskCompletionSource,
            CancellationToken cancellationToken)
        {
            InitializeComponent();
            _authorizationUri = authorizationUri;
            _redirectUri = redirectUri;
            _taskCompletionSource = taskCompletionSource;
            _cancellationToken = cancellationToken;
        }

        private void WebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (!e.Uri.ToString().StartsWith(_redirectUri.ToString()))
            {
                // not redirect uri case
                return;
            }

            // parse query string
            var query = HttpUtility.ParseQueryString(e.Uri.Query);
            if (query.AllKeys.Any(x => x == "code"))
            {
                // It has a code parameter.
                _taskCompletionSource.SetResult(e.Uri);
            }
            else
            {
                // error.
                _taskCompletionSource.SetException(
                    new MsalExtensionException(
                        $"An error occurred, error: {query.Get("error")}, error_description: {query.Get("error_description")}"));
            }

            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _token = _cancellationToken.Register(() => _taskCompletionSource.SetCanceled());
            // navigating to an uri that is entry point to authorization flow.
            webBrowser.Navigate(_authorizationUri);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _taskCompletionSource.TrySetCanceled();
            _token.Dispose();
        }
    }
}
