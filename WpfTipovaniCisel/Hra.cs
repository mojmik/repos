using System;
using System.Collections.Generic;
using System.Text;

namespace WpfTipovaniCisel {
    class Hra {
        public int hadaneCislo = 0;
        public int range = 0;
        public int pokus = 0;
        string tipy = "";
        Random rand = new Random();
        public Hra (int range) {
            this.range = range;
            hadaneCislo = rand.Next(range);
        }
        public bool Hadej(int guess) {
            if (guess == hadaneCislo) return true;
            else {
                this.pokus++;
                this.tipy=(this.tipy == "") ? ""+guess : this.tipy + ", "+guess;
                return false;
            }
        }
        public string VypisInfo(bool success) {
            if (success) return $"Uhodl jsi! Pocet pokusu: {pokus} \n Tve tipy: {tipy} ";
            else return $"Spatne! Pocet pokusu: {pokus} \n Tve tipy: {tipy} ";
        }
    }
}
