using Microsoft.Identity.Client.Extensibility;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using OneDrive_Cloud_Player.API.Authentication.InteractiveComponents.Internal;
using System.Windows.Threading;

namespace OneDrive_Cloud_Player.API.Authentication.InteractiveComponents
{
    /// <summary>
    /// Provides embedded web ui for WPF on .NET Core.
    /// The web ui is using WebBrowser control(Trident engine).
    /// </summary>
    public class EmbeddedBrowser : ICustomWebUi
    {
        public const int DefaultWindowWidth = 600;
        public const int DefaultWindowHeight = 800;

        private readonly Window _owner;
        private readonly string _title;
        private readonly int _windowWidth;
        private readonly int _windowHeight;
        private readonly WindowStartupLocation _windowStartupLocation;

        public EmbeddedBrowser(Window owner,
            string title = "Sign in",
            int windowWidth = DefaultWindowWidth,
            int windowHeight = DefaultWindowHeight,
            WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterOwner)
        {
            Dispatcher.FromThread(Thread.CurrentThread);
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _title = title;
            _windowWidth = windowWidth;
            _windowHeight = windowHeight;
            _windowStartupLocation = windowStartupLocation;
        }

        public Task<Uri> AcquireAuthorizationCodeAsync(Uri authorizationUri, Uri redirectUri, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<Uri>();
            _owner.Dispatcher.Invoke(() =>
            {
                new EmbeddedWebUI(authorizationUri,
                    redirectUri,
                    tcs,
                    cancellationToken)
                {
                    Owner = _owner,
                    Title = _title,
                    Width = _windowWidth,
                    Height = _windowHeight,
                    WindowStartupLocation = _windowStartupLocation,
                }.ShowDialog();
            });

            return tcs.Task;
        }
    }
}
