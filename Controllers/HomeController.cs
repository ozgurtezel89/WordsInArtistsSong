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

            service.SearchArtistAllSongsByName(viewModel.ArtistNameToSearch);

            return View();
        }
    }
}