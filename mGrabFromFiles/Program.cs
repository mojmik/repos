using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mGrabFromFiles {
    class Program {
        
        static void Main(string[] args) {
            FileWalker fw = new FileWalker();
            fw.ListFiles(@"\\rentex.intra\company\data\Rezervace\backup","*.txt");
        }
    }
}
