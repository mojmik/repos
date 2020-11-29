using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCNahodneCislo.Models {
    public class Generator {
        private Random random = new Random();

        public int VratCislo() {
            return random.Next(100);
        }
    }
}
