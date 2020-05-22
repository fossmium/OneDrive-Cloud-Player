using System;
using System.Collections.Generic;
using System.Text;

namespace Explorer
{
    class Shows
    {
        public static List<Show> CreateShowList()
        {
            List<Show> ShowList = new List<Show>();
            ShowList.Add(new Show("Akame ga Kill", "Anime", new Uri("https://image.tmdb.org/t/p/w220_and_h330_face/3c5BIEfAhTCWrIi4C8WMuwOl6bX.jpg")));
            ShowList.Add(new Show("Attack on Titan", "Anime", new Uri("https://image.tmdb.org/t/p/w220_and_h330_face/1DTny2XW9vvMMpc5fwLRvei7W3L.jpg")));
            ShowList.Add(new Show("WorldEnd", "Heartbreaking", new Uri("https://image.tmdb.org/t/p/w220_and_h330_face/mwOPwKKqEoYDLBSnyOmpm0H2QZJ.jpg")));
            ShowList.Add(new Show("WorldEnd", "Heartbreaking", new Uri("https://image.tmdb.org/t/p/w220_and_h330_face/mwOPwKKqEoYDLBSnyOmpm0H2QZJ.jpg")));
            return ShowList;
        }
    }
}
