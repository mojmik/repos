using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mGrabFromFiles {
    class FileWalker {
        List<string> output;
        Dictionary<string,int> outputDetails=new Dictionary<string, int>();
        public void GetLines(string file, string lineStartsWith="") {
            string line="";
            string seznamKodu = "";
            string[] kody;
            if (!File.Exists(file)) return;
            output = new List<string>();            
            using (System.IO.StreamReader reader = new System.IO.StreamReader(file)) {
                while ((line = reader.ReadLine()) != null) {
                    if (lineStartsWith!="" && line.StartsWith(lineStartsWith)) {
                        seznamKodu = line.Substring(lineStartsWith.Length+1);
                        output.Add(seznamKodu);
                        kody = seznamKodu.Split(' ');
                        foreach (string kod in kody) {
                            if (kod != "") {
                                if (outputDetails.ContainsKey(kod)) outputDetails[kod]++;
                                else outputDetails.Add(kod, 1);                                
                            }
                        }
                    }
                }
            }
        }
        public void WriteOutput() {
            using (StreamWriter writetext = new StreamWriter("write.txt")) {                
                foreach (var o in outputDetails) {
                    string kod = o.Key;
                    int pocet = o.Value;
                    writetext.WriteLine(kod + " - " + pocet);
                }
            }

            
        }
        public void ListFiles(string path, string fileMask) {
            int n = 0;
            DirectoryInfo d = new DirectoryInfo(path);
            try {
                FileInfo[] Files = d.GetFiles(fileMask);
                foreach (FileInfo file in Files) {
                    if (n % 50 == 0) Console.WriteLine(n);
                    GetLines(file.FullName,"41");
                    n++;
                }
                WriteOutput();
            }
            catch (Exception e) {
                //logger.WriteLog($"Fileserver problably not reachable, exception: {e.Message} {e.InnerException}", Logger.TypeLog.both);
            }
        }
    }
}
