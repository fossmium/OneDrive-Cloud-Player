using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using OneDrive_Cloud_Player.Models.GraphData;
using OneDrive_Cloud_Player.Views;
using Windows.UI.Xaml;

namespace OneDrive_Cloud_Player.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary> 
    public class ViewModelLocator
    {
        // Declaring the navigation keys
        public const string MainPageKey = "MainPage";
        public const string VideoPlayerPageKey = "VideoPlayerPage";
        public const string LoginPageKey = "LoginPage";

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            var nav = new NavigationService();

            // Configure navigation keys
            nav.Configure(MainPageKey, typeof(MainPage));
            nav.Configure(VideoPlayerPageKey, typeof(VideoPlayerPage));
            nav.Configure(LoginPageKey, typeof(LoginPage));

            if (ViewModelBase.IsInDesignModeStatic)
            {
                // Create design time view services and models
            }
            else
            {
                // Create run time view services and models
            }

            //Register your services used here
            //SimpleIoc.Default.Register<INavigationService, NavigationService>();
            SimpleIoc.Default.Register<INavigationService>(() => nav);
            SimpleIoc.Default.Register<VideoPlayerPageViewModel>();
            SimpleIoc.Default.Register<LoginPageViewModel>();
            SimpleIoc.Default.Register<MainPageViewModel>();

        }

        /// <summary>
        /// Unregister and register the <see cref="MainPageViewModel"/> so the view model resets.
        /// </summary>
        public static void ResetMainPageViewModel()
        {
            SimpleIoc.Default.Unregister<MainPageViewModel>();
            SimpleIoc.Default.Register<MainPageViewModel>();
        }

        // <summary>
        // Gets the VideoPlayer view model.
        // </summary>
        // <value>
        // The VideoPlayerPage view model.
        // </value>
        public VideoPlayerPageViewModel VideoPlayerPageInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<VideoPlayerPageViewModel>();
            }
        }

        // <summary>
        // Gets the LoginPage view model.
        // </summary>
        // <value>
        // The LoginPage view model.
        // </value>
        public LoginPageViewModel LoginPageInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<LoginPageViewModel>();
            }
        }

        // <summary>
        // Gets the MainPage view model.
        // </summary>
        // <value>
        // The MainPage view model.
        // </value>
        public MainPageViewModel MainPageInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainPageViewModel>();
            }
        }

        // <summary>
        // The cleanup.
        // </summary>
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }

}