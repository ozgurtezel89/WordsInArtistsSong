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

            try
            {
                TechTest.BusinessLogic.Models.ArtistAndSongs artistsAllSongs = service.SearchArtistAllSongsByName(viewModel.ArtistNameToSearch, 0, 100);

                // lets calculate the average words now
                // I am using decimal as its the largest numeric dataType in c#
                decimal totalWordsInAllSongs = 0;
                foreach (var song in artistsAllSongs.Songs)
                {
                    totalWordsInAllSongs += service.GetCountOfWordsOfSong(viewModel.ArtistNameToSearch, song.Title);
                }

                decimal average = totalWordsInAllSongs / artistsAllSongs.Songs.Count;

                average = Math.Ceiling(average);

                ViewBag.AverageWords = average;
            }
            catch (Exception e)
            {
                if(e.Message.Contains("503"))
                {
                    return Content(e.Message + " (This is due to musicbrainz rate Limiting! I provided userAgent in my request but I assume this is due to too many requests from my IP address, I hope you will never see this message :) further information available on their website)" + @"https://musicbrainz.org/doc/XML_Web_Service/Rate_Limiting");
                }

                return Content("There is an error: " + e.Message);
                throw;
            }
           
            return View();
        }

        public ActionResult RefreshCache()
        {
            HttpContext.Cache.Remove("artistNamesAndSongs");

            TempData["cacheRefreshed"] = true;
            return View("Index");
        }
    }
}