using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokusFileSystemUdalosti
{
    public class Watcher
    {
        FileSystemWatcher fsw;
 
        public Watcher()
        {
            fsw = new FileSystemWatcher();
            fsw.Path = "c:\\temp\\";
            fsw.EnableRaisingEvents = true;            
            fsw.Created += new FileSystemEventHandler(OnCreated);
            fsw.Changed += new FileSystemEventHandler(OnModified);          

        }
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Byl vytvořen soubor: {0}", e.Name);
        }
        private void OnModified(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("zmenen soubor: {0}", e.Name);
        }
    }
}
