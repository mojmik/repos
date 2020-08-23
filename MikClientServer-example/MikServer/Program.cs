using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace MikServer {
    
    class Program {
        static byte[] data;  
        static Socket socket;
        
        static void Main(string[] args) {
            bool serverEnd = false;
            int j;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6666)); 

            socket.Listen(1); //posloucham 1 klienta
            Socket accepteddata = socket.Accept();
            while (!serverEnd) {                
                data = new byte[accepteddata.SendBufferSize];
                try {
                    j = accepteddata.Receive(data);
                }
                catch (Exception e) {
                    //asi klient ukoncil spojeni
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Zmackni neco");
                    Console.ReadKey();
                    break;
                }
                byte[] adata = new byte[j];
                for (int i = 0; i < j; i++)
                    adata[i] = data[i];
                string dat = Encoding.Default.GetString(adata);
                if (dat == "end") serverEnd = true;
                Console.WriteLine(dat);
            }
        }
    }
}
