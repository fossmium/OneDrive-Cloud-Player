using System;
using System.Collections.Generic;
using System.Text;

namespace Explorer
{
    class Show
    {
        public string Title
        {
            get
            {
                return Title;
            }
            set
            {
                Title = value;
            }
        }

        public string Genre
        {
            get
            {
                return Genre;
            }
            set
            {
                Genre = value;
            }
        }

        public Show(string title, string genre)
        {
            this.Title = title;
            this.Genre = genre;
        }

    }
}
