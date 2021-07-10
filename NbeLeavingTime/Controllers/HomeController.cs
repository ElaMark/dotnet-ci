using NbeLeavingTime.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace NbeLeavingTime.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.SiteHeading = ConfigurationManager.AppSettings["SiteHeading"].ToString();

            NbeModel m = new NbeModel();
            return View();
        }

      
        public string GetAllPosts()
        {
            NbeModel m = new NbeModel();
            return Newtonsoft.Json.JsonConvert.SerializeObject(m.GetAllPosts()); 
        }

        

        

    }
}