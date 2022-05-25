using Microsoft.Extensions.DependencyInjection;
using OneDrive_Cloud_Player.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace OneDrive_Cloud_Player.Views
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            var container = App.Current.Container;
            DataContext = ActivatorUtilities.GetServiceOrCreateInstance(container, typeof(MainPageViewModel));
        }

        /// <summary>
        /// Gets called when navigating from this page.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (e.SourcePageType == typeof(LoginPage))
            {
                ResetPageCache();
            }

        }

        /// <summary>
        /// Clears the cache of the pages that have "NavigationCacheMode" set to "enabled".
        /// </summary>
        private void ResetPageCache()
        {
            var cacheSize = ((Frame)Parent).CacheSize;
            ((Frame)Parent).CacheSize = 0;
            ((Frame)Parent).CacheSize = cacheSize;
        }
    }
}
