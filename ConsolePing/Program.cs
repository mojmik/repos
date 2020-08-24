using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsolePing {
    class Program {
        public static bool DoPing(string url) {
            Ping ping = new Ping();

            PingReply reply = ping.Send(url, 3000);

            if (reply.Status == IPStatus.Success)
                return true;
            return false;
        }
        static void Main(string[] args) {
            string adresa = "www.google.com";
            int n = 0;
            int count = -1;
            string cntArg;
            foreach (string arg in args) {
                if (n==0) adresa = arg;
                if (n == 1) {
                    if (arg == "t") count = -1;
                    else {
                        count = int.Parse(arg);
                    }
                }
                n++;
            }            
            
            while (count!=0) {
                try {
                    if (DoPing(adresa)) {
                        Console.WriteLine($"Hej funguje to {adresa}, funguje, popláááách!!!!");                        
                    }
                    else {
                        Console.WriteLine($"Zatím nic {adresa} :'(");
                    }
                }
                catch (System.Net.NetworkInformation.PingException e) {
                    Console.WriteLine(" Adresa neexistuje! " + e.Message);
                    return;
                }
                
                System.Threading.Thread.Sleep(5000);
                if (count>0) count--;
            }
        }
    }
}
