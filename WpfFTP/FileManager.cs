using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfFTP {
    class FileManager : INotifyPropertyChanged {
        public List<string> Files { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void ReadFiles() {
            FTPtools ftpTools = new FTPtools();
            Files=ftpTools.GetFiles("ftp://test.rebex.net/pub/example","demo","password");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Files)));
        }

    }
}
