using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBE.Models;

namespace NBE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppSettings _appSettings;

        public HomeController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            GlobalAppSettings.Settings = _appSettings;
            
        }

     
        public IActionResult Index()
        {
            ViewBag.SiteHeading = _appSettings.SiteTitle;

            NbeModel m = new NbeModel(_appSettings);
            if (_appSettings.Hardcodednames=="true"){
                AzureServices azureServices = new AzureServices(_appSettings);
                var personID = RouteData.Values["id"]??""; 
                var person = azureServices.GetPersonFromAzure(personID.ToString());
                ViewBag.Person=person;
            }

            return View();
        }

        public string GetAllPosts()
        {
            NbeModel m = new NbeModel(_appSettings);
            return Newtonsoft.Json.JsonConvert.SerializeObject(m.GetAllPosts()); 
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
