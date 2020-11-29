using Microsoft.AspNetCore.Mvc;
using MVCNahodneCislo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCNahodneCislo.Controllers {
    public class HomeController : Controller {
        public IActionResult Index() {
            Generator generator = new Generator();
            ViewBag.Neco = "ahoj";
            ViewBag.Cislo = generator.VratCislo();
            return View();
        }
    }
}
