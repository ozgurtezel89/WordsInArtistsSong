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

            TechTest.BusinessLogic.Models.ArtistAndSongs artistsAllSongs = service.SearchArtistAllSongsByName(viewModel.ArtistNameToSearch, 0, 5);

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