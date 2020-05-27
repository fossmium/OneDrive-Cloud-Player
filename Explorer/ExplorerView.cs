using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Explorer
{
    class ExplorerView : MainWindow, INotifyPropertyChanged
    {
        List<Show> showList { get; set; }
        
        public ExplorerView()
        {
            showList = new List<Show>();
            CreateShowList();
            BindData(showList);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<Show> CreateShowList()
        {
            showList.Add(new Show("Attack on Titan", "Action"));
            showList.Add(new Show("World End", "Anime"));
            showList.Add(new Show("Twilight", "?????"));
            showList.Add(new Show("Spiderman", "Spooder"));

            return showList;
        }

        public void BindData(List<Show> showList)
        {
            
        }
    }
}
