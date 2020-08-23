using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WpfFTP {
    class FTPtools {
        List<string> responseStrings = new List<string>();
        FtpWebRequest publicFtp;
        FtpWebResponse publicResponse;
        public void Connect(string uri,string user, string password) {
            Uri publicUri = new Uri(@""+uri);
            publicFtp = (FtpWebRequest)FtpWebRequest.Create(publicUri);
            publicFtp.Credentials = new NetworkCredential(user, password);
            
        }
        public void DoMethod(string method) {
            if (method=="list")  publicFtp.Method = WebRequestMethods.Ftp.ListDirectory;
            publicResponse = (FtpWebResponse)publicFtp.GetResponse();
        }
        public void ReadResponse() {
            Stream publicStream = publicResponse.GetResponseStream();
            StreamReader publicStreamReader = new StreamReader(publicStream);

            while (!publicStreamReader.EndOfStream) {
                responseStrings.Add(publicStreamReader.ReadLine());
            }
        }
        public List<string> GetFiles(string uri, string user, string password) {
            Connect(uri, user, password);
            DoMethod("list");
            ReadResponse();
            return responseStrings;
        }
    }
}
