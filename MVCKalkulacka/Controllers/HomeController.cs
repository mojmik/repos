using Microsoft.AspNetCore.Mvc;
using MVCKalkulacka.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCKalkulacka.Controllers {
    public class HomeController : Controller {
        
        public IActionResult Index(string jmeno2) { //jakmile pridam parametr ?jmeno2=mik, tak ho to nacte
            Kalkulacka kalkulacka = new Kalkulacka();
            ViewBag.Jmeno = jmeno2;
            return View(kalkulacka);
        }

        [HttpPost]
        public IActionResult Index(Kalkulacka kalkulacka) {
            if (ModelState.IsValid) {
                kalkulacka.Vypocitej();
            }

            return View(kalkulacka);
        }
    }
}
