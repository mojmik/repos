using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MVCKalkulacka.Models {
    public class Kalkulacka {
        [Display(Name = "1. číslo")]
        [Range(1, 100, ErrorMessage = "Zadejte prosím číslo od 1 do 100.")]
        public int Cislo1 { get; set; }
        [Display(Name = "2. číslo")]
        public int Cislo2 { get; set; }
        public double Vysledek { get; set; }
        public string Operace { get; set; }
        public List<SelectListItem> MozneOperace { get; set; }

        public Kalkulacka() {
            MozneOperace = new List<SelectListItem>();
            //tady me prekvapila konstrukce s "objektovym inicializatorem"
            MozneOperace.Add(new SelectListItem { Text = "Sečti", Value = "+", Selected = true });
            MozneOperace.Add(new SelectListItem { Text = "Odečti", Value = "-" });
            MozneOperace.Add(new SelectListItem { Text = "Vynásob", Value = "*" });
            MozneOperace.Add(new SelectListItem { Text = "Vyděl", Value = "/" });
        }

        public void Vypocitej() {
            switch (Operace) {
                case "+":
                    Vysledek = Cislo1 + Cislo2;
                    break;
                case "-":
                    Vysledek = Cislo1 - Cislo2;
                    break;
                case "*":
                    Vysledek = Cislo1 * Cislo2;
                    break;
                case "/":
                    Vysledek = Cislo1 / Cislo2;
                    break;
            }
        }



    }

}
