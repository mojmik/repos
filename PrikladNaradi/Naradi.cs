using System;
using System.Collections.Generic;
using System.Text;

namespace PrikladNaradi
{
    abstract class Naradi
    {
        public int Vaha { get; set; }
        public string Nazev { get; set; }
        public abstract string Pracuj();
        
    }
}
