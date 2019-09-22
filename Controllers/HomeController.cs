using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TechTest.Models;

namespace TechTest.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View(new AverageWordsInSongSearchViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(AverageWordsInSongSearchViewModel viewModel)
        {
            TechTest.BusinessLogic.Services.SearchServices service = new BusinessLogic.Services.SearchServices();

            List<string> artistsSongs = new List<string>();

            bool artistHasMoreResulsts = true;
            int offset = 0;
            int limit = 100;

            // as there is a limit on the web-api to 100 below is the logic to get all the song names for the artist.
            while (artistHasMoreResulsts)
            {
                List<string> artistsSongsLimited = service.SearchArtistAllSongsByName(viewModel.ArtistNameToSearch,offset, limit);
                artistsSongs.AddRange(artistsSongsLimited);
                offset += limit;

                if (artistsSongsLimited.Count == 0)
                    artistHasMoreResulsts = false;
            }

            // as there might be duplicate records due to data on the web-api it is good to get rid off them (as per search term artist:tarkan there were 358 results before running the distinct method, now results are reduced to 203, will be more resource efficient when we use this song names to get the count of words)
            artistsSongs = (from d in artistsSongs select d).Distinct().ToList();
            

            return View();
        }
    }
}