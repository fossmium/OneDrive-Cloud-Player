using System.Windows;
using LibVLCSharp.Shared;
using OneDrive_Cloud_Player.VLC;

namespace OneDrive_Cloud_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenVideoWindow_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayerWindow win2 = new VideoPlayerWindow("https://newuniversity-my.sharepoint.com/personal/tim_gels_student_nhlstenden_com/_layouts/15/download.aspx?UniqueId=987a9233-79d2-4aa4-a01b-29f0ba103b03&Translate=false&tempauth=eyJ0eXAiOiJKV1QiLCJhbGciOiJub25lIn0.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTBmZjEtY2UwMC0wMDAwMDAwMDAwMDAvbmV3dW5pdmVyc2l0eS1teS5zaGFyZXBvaW50LmNvbUAwMTZhOWU0OC1iYTBiLTQ5ZjQtOTdmOC1hODgzNTIxNjRlNTgiLCJpc3MiOiIwMDAwMDAwMy0wMDAwLTBmZjEtY2UwMC0wMDAwMDAwMDAwMDAiLCJuYmYiOiIxNTkwNjY0NTc3IiwiZXhwIjoiMTU5MDY2ODE3NyIsImVuZHBvaW50dXJsIjoib2pKR1BHMmVUS1hZR0xCRlJRbjFPTk5rUUNweVpzd1pvbWJEdnIwa05RYz0iLCJlbmRwb2ludHVybExlbmd0aCI6IjE2OCIsImlzbG9vcGJhY2siOiJUcnVlIiwiY2lkIjoiT0RnMk5URmhZMlF0TUROaE5TMDFZbU5sTFdFM1pEQXRNek15WVdVeFpEa3pNbVl5IiwidmVyIjoiaGFzaGVkcHJvb2Z0b2tlbiIsInNpdGVpZCI6Ik5qRTFZelUzTXpBdE1XSXpOQzAwTW1VM0xUa3hZakF0TWpGaVlqbG1Zak0xTXpZMCIsImFwcF9kaXNwbGF5bmFtZSI6IkdyYXBoIGV4cGxvcmVyIiwiZ2l2ZW5fbmFtZSI6IlRpbSIsImZhbWlseV9uYW1lIjoiR2VscyIsInNpZ25pbl9zdGF0ZSI6IltcImttc2lcIl0iLCJhcHBpZCI6ImRlOGJjOGI1LWQ5ZjktNDhiMS1hOGFkLWI3NDhkYTcyNTA2NCIsInRpZCI6IjAxNmE5ZTQ4LWJhMGItNDlmNC05N2Y4LWE4ODM1MjE2NGU1OCIsInVwbiI6InRpbS5nZWxzQHN0dWRlbnQubmhsc3RlbmRlbi5jb20iLCJwdWlkIjoiMTAwMzNGRkZBOTU1NzU4MSIsImNhY2hla2V5IjoiMGguZnxtZW1iZXJzaGlwfDEwMDMzZmZmYTk1NTc1ODFAbGl2ZS5jb20iLCJzY3AiOiJhbGxmaWxlcy53cml0ZSBhbGxzaXRlcy53cml0ZSBhbGxwcm9maWxlcy5yZWFkIGFsbHByb2ZpbGVzLnJlYWQgYWxscHJvZmlsZXMud3JpdGUiLCJ0dCI6IjIiLCJ1c2VQZXJzaXN0ZW50Q29va2llIjpudWxsfQ.cTNFSzZucmtrNy90V3VBa0kxcGs1QXQ5dW82ZmxqYzh4ZnZVKzluM0kyaz0&ApiVersion=2.0");
            win2.Show();
        }
    }
}
