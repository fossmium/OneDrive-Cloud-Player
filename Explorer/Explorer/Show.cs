using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Explorer
{
    class Show
    {
        public string Title { get; private set; }
        public string Genre { get; private set; }

        public Uri ImageLocation { get; private set; }
        private int rating = 0;
        public int Rating
        {
            get { return rating; }
            set
            {
                if (value > 0 && value < 11)
                {
                    rating = value;
                }
            }
        }

        public Show(string Title, string Genre)
        {
            this.Title = Title;
            this.Genre = Genre;
        }

        public Show(string Title, string Genre, Uri ImageLocation)
        {
            this.Title = Title;
            this.Genre = Genre;
            this.ImageLocation = ImageLocation;
        }
    }
}
