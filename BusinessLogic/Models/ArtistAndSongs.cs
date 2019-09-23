using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TechTest.BusinessLogic.Models
{
    public class ArtistAndSongs
    {
        public string ArtistName { get; set; }
        public List<Song> Songs { get; set; }
    }

    public class Song
    {
        public string Title { get; set; }
    }
}