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

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.SourcePageType == typeof(LoginPage))
            {
                ResetPageCache();
            }
          
        }

        private void ResetPageCache()
        {
            var cacheSize = ((Frame)Parent).CacheSize;
            ((Frame)Parent).CacheSize = 0;
            ((Frame)Parent).CacheSize = cacheSize;
        }
    }
}
