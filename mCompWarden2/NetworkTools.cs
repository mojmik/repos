using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;

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
        public string GetIPs(string outWhat = "full") {
            string ips = "";
            string ip;
            string strout = "";
            int n;
            foreach (System.Net.NetworkInformation.NetworkInterface netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()) {
                strout += "Name: " + netInterface.Name;
                //Console.WriteLine("Description: " + netInterface.Description);
                //Console.WriteLine("Addresses: ");
                System.Net.NetworkInformation.IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                n = 0;
                foreach (System.Net.NetworkInformation.UnicastIPAddressInformation addr in ipProps.UnicastAddresses) {
                    ip = addr.Address.ToString();
                    if (n > 0) ip += ", ";
                    else ip = " " + ip;
                    strout += ip;
                    ips += ip;
                    n++;
                }
                strout += "\n";
            }
            if (outWhat == "full") return strout;
            else return ips;
        }
    }
}
