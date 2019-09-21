using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TechTest.Models
{
    public class AverageWordsInSongSearchViewModel
    {
        [Display(Name = "Artist Name:")]
        [Required]
        [StringLength(100)]
        public string ArtistNameToSearch { get; set; }
    }
}