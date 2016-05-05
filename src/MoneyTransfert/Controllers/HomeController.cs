﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;

namespace MoneyTransfert.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Historique()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Rapport()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Help()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
