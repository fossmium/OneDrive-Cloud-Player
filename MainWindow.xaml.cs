using System.Windows;
using LibVLCSharp.Shared;

namespace OneDrive_Cloud_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaPlayer _mediaPlayer;
        LibVLC _libVLC;
        public MainWindow()
        {
            InitializeComponent();

            videoView.Loaded += VideoView_Loaded;
        }

        void VideoView_Loaded(object sender, RoutedEventArgs e)
        {
            Core.Initialize();
            _libVLC = new LibVLC(); 
            _mediaPlayer = new MediaPlayer(_libVLC);
            videoView.MediaPlayer = _mediaPlayer;
            _mediaPlayer.Play(new Media(_libVLC, "https://newuniversity-my.sharepoint.com/personal/tim_gels_student_nhlstenden_com/_layouts/15/download.aspx?UniqueId=cecae3ba-111e-4097-9a4a-70fad01ac410&Translate=false&tempauth=eyJ0eXAiOiJKV1QiLCJhbGciOiJub25lIn0.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTBmZjEtY2UwMC0wMDAwMDAwMDAwMDAvbmV3dW5pdmVyc2l0eS1teS5zaGFyZXBvaW50LmNvbUAwMTZhOWU0OC1iYTBiLTQ5ZjQtOTdmOC1hODgzNTIxNjRlNTgiLCJpc3MiOiIwMDAwMDAwMy0wMDAwLTBmZjEtY2UwMC0wMDAwMDAwMDAwMDAiLCJuYmYiOiIxNTkwNTAxMjYxIiwiZXhwIjoiMTU5MDUwNDg2MSIsImVuZHBvaW50dXJsIjoiREpOYm5OT1FrY1N5TkxrTlMrNHAxeDJrZy9SSnJ1KzBXdlF2eG1JWmc4az0iLCJlbmRwb2ludHVybExlbmd0aCI6IjE2OCIsImlzbG9vcGJhY2siOiJUcnVlIiwiY2lkIjoiT1RsaFpUazFNamN0Wmpka01DMWtOR000TFRNME1EZ3RNR0psTTJVMlpUQmpOR0l3IiwidmVyIjoiaGFzaGVkcHJvb2Z0b2tlbiIsInNpdGVpZCI6Ik5qRTFZelUzTXpBdE1XSXpOQzAwTW1VM0xUa3hZakF0TWpGaVlqbG1Zak0xTXpZMCIsImFwcF9kaXNwbGF5bmFtZSI6IkdyYXBoIGV4cGxvcmVyIiwiZ2l2ZW5fbmFtZSI6IlRpbSIsImZhbWlseV9uYW1lIjoiR2VscyIsInNpZ25pbl9zdGF0ZSI6IltcImttc2lcIl0iLCJhcHBpZCI6ImRlOGJjOGI1LWQ5ZjktNDhiMS1hOGFkLWI3NDhkYTcyNTA2NCIsInRpZCI6IjAxNmE5ZTQ4LWJhMGItNDlmNC05N2Y4LWE4ODM1MjE2NGU1OCIsInVwbiI6InRpbS5nZWxzQHN0dWRlbnQubmhsc3RlbmRlbi5jb20iLCJwdWlkIjoiMTAwMzNGRkZBOTU1NzU4MSIsImNhY2hla2V5IjoiMGguZnxtZW1iZXJzaGlwfDEwMDMzZmZmYTk1NTc1ODFAbGl2ZS5jb20iLCJzY3AiOiJhbGxmaWxlcy53cml0ZSBhbGxzaXRlcy53cml0ZSBhbGxwcm9maWxlcy5yZWFkIGFsbHByb2ZpbGVzLnJlYWQgYWxscHJvZmlsZXMud3JpdGUiLCJ0dCI6IjIiLCJ1c2VQZXJzaXN0ZW50Q29va2llIjpudWxsfQ.d3c2K0kveFpWSnNEWktJbzZEeVU0cGM4K0RKRmtSdGFRU1NZZ2hvZkFVZz0&ApiVersion=2.0", FromType.FromLocation));
        }
    }
}
