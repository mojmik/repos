using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTamagotchi {
    class Pet {
        public int Food { get; set; }
        public int Spanek { get; set; }
        public int Hygiene { get; set; }
        public int Walk { get; set; }   
        public Pet() {
            Food = 99;
            Spanek = 99;
            Hygiene = 99;
            Walk = 99;
        }
        public bool IsHappy() {
            if (Food < 40 || Spanek < 40 || Hygiene < 40 || Walk < 40) return false;
            return true;
        }
        public void Sleep() {
            Spanek = 99;
        }

    }
}
