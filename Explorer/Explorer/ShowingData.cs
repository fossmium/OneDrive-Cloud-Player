using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Explorer
{
    class ShowingData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<Show> showList = new List<Show>();

        private Show selectedShow;

        public List<Show> ShowList
        {
            get { return showList; }
            set { showList = value;
                NotifyPropertyChanged();
            }
        }

        public Show SelectedShow
        {
            get { return selectedShow; }
            set
            {
                selectedShow = value;
                NotifyPropertyChanged();
            }
        }

        public ShowingData()
        {
            this.showList = Shows.CreateShowList();
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
