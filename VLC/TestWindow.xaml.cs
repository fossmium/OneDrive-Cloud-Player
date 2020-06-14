using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OneDrive_Cloud_Player.VLC
{

    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        private bool VolumeSliderActive;

        public TestWindow()
        {
            InitializeComponent();
            VolumeSliderActive = false;

        }

        private void Slider_MouseEnter(object sender, MouseEventArgs e)
        {
            VolumeSliderActive = true;

            Console.WriteLine("Entered!");
        }
    }
}
