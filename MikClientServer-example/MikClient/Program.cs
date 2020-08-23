using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace MikClient {
    class Program {
        static void Main(string[] args) {
            bool endClient = false;
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try 
            {
                s.Connect(IPAddress.Parse("127.0.0.1"), 6666);                
                while (!endClient) {
                    Console.Write("Zadej nejakej text : ");
                    string q = Console.ReadLine();
                    if (q=="end") {
                        endClient = true;
                    }
                    else {                        
                        byte[] data = Encoding.Default.GetBytes(q);
                        s.Send(data);
                    }                    
                }
            }
            catch (Exception e)
            {
                // nepripojeno
                Console.Write("Asi se nepodarilo spojit se serverem.. "+e.Message);
            }
        }
    }
}
