using OneDrive_Cloud_Player.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OneDrive_Cloud_Player.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlayerPage : Page
    {
        public VideoPlayerPage()
        {
            this.InitializeComponent();
            SeekBar.AddHandler(PointerPressedEvent,
            new PointerEventHandler(SeekBar_PointerPressed), true);
        }

        //TODO: When clicked 3 times, the seekbar wont update anymore. I think it has to do with the boolean of IsSeeking turning to true after the Tapped event.
        /// <summary>
        /// When user clicks it needs to set the IsSeeking boolean to true.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeekBar_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ((VideoPlayerPageViewModel)(this.DataContext)).IsSeeking = true;
        }

        /// <summary>
        /// When the user releases the click is needs to set the IsSeeking boolean to false.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeekBar_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            ((VideoPlayerPageViewModel)(this.DataContext)).IsSeeking = false;
        }
    }
}
