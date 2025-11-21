
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Prog6212.Models;


namespace Prog6212.Controllers

{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}