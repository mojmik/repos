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

        string url;
        string user;
        string pass;
        public bool IsConnected { get; private set; }
        public void Connect(string uri,string user, string password) {
            this.url = uri;
            this.user = user;
            this.pass = password;
            if (IsConnected) return;
            Uri publicUri = new Uri(@""+uri);
            publicFtp = (FtpWebRequest)FtpWebRequest.Create(publicUri);
            publicFtp.Credentials = new NetworkCredential(user, password);
            IsConnected = true;
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
            this.url = uri;
            this.user = user;
            this.pass = password;
            Connect(uri, user, password);
            DoMethod("list");
            ReadResponse();
            return responseStrings;
        }
        public void DownloadFile(string file) {
            if (!IsConnected) throw new ApplicationException();
            WebClient webClient = new WebClient();
            webClient.Credentials = new NetworkCredential(user, pass);
            Uri uri = new Uri(@""+url+@"/"+file);            
            string fn = System.IO.Path.GetFileName(uri.LocalPath);
            webClient.DownloadFile(uri, fn);
        }
    }
}
