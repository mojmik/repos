using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;

namespace mCompWarden2 {
    class NetworkTools {
        public long IsHostAvailable(string nameOrAddress) {
            bool pingable = false;
            long pingMs;
            Ping pinger = new Ping();
            try {
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
                pingMs = reply.RoundtripTime;
            }
            catch (PingException) {
                pingMs = -1;
                // Discard PingExceptions and return false;
            }
            return pingMs;
        }
        public static string GetIPs(bool includingName = false, bool ip4only = true, string delimiter = "|") {
            string ips = "";
            string thisips = "";
            string ip;
            int n;
            int y=0;
            foreach (System.Net.NetworkInformation.NetworkInterface netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()) {
              
                thisips = "";
                //Console.WriteLine("Description: " + netInterface.Description);
                //Console.WriteLine("Addresses: ");
                System.Net.NetworkInformation.IPInterfaceProperties ipProps = netInterface.GetIPProperties();                
                foreach (System.Net.NetworkInformation.UnicastIPAddressInformation addr in ipProps.UnicastAddresses) {
                    n = 0;  
                    if (ip4only && (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)) {
                        //ipv4
                        ip = addr.Address.ToString();
                        if (ip != "127.0.0.1" && !ip.Contains("169.254.")) {
                            if (n > 0) thisips += ", " + ip;
                            else thisips += ip;
                        }
                        n++;   
                    }                    
                }
                if (thisips!="") {                    
                    if (y > 0) ips += delimiter;
                    if (includingName) ips += "Name: " + netInterface.Name + " ";
                    ips += thisips;
                    y++;
                }                
            }            
            return ips;
        }
    }
}
