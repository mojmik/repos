using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;

namespace WpfZIPaXMLzamestnanci {
    class Zamestnanec : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public static string filter = "Soubor zaměstnance (*.zamestnanec)|*.zamestnanec";
        string jmeno;
        public string Jmeno {
            get => jmeno;
            
            set {
                jmeno = value;
                ZmenaProperty(nameof(Jmeno));
            }
        }
        
        string prijmeni;
        public string Prijmeni {
            get => prijmeni;
            
            set {
                prijmeni = value;
                ZmenaProperty(nameof(prijmeni));
            } 
        }
        
        string email;
        public string Email { 
            get =>email;
            set {
                email = value;
                ZmenaProperty(nameof(Email));
            } 
        }
        string telefon;
        public string Telefon {
            get => telefon;
            set {
                telefon = value;
                ZmenaProperty(nameof(Telefon));
            }
        }
        public DateTime DatumNarozeni { get; set; }
        Image foto;
        public void ZmenaProperty(string propName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public Image Foto {
            get { return foto; }
            set {   if (value != null) {
                        foto = value; 
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Foto)));
                    }
                }
        }
        public BitmapImage bitMapImage;
        public Zamestnanec() {
            Jmeno = "";
            Prijmeni = "";
            Email = "";
            Telefon = "";
            Foto = null;
            DatumNarozeni = DateTime.Now;
        }
        public void Load(string soubor) {
            using (FileStream fs = new FileStream(soubor, FileMode.Open)) {
                using (System.IO.Compression.ZipArchive zip = new System.IO.Compression.ZipArchive(fs)) {
                    System.IO.Compression.ZipArchiveEntry info = zip.GetEntry("info.xml");
                    LoadInfo(info.Open());
                    System.IO.Compression.ZipArchiveEntry foto = zip.GetEntry("foto.jpg");
                    if (foto != null) {
                        bitMapImage = new BitmapImage();
                        using (var memoryStream = new MemoryStream()) {
                            /// mik: ty obrazky delat pres bitMapImage je asi blbost, jednodussi by to bylo pres System.Drawing.Image
                            /// bez toho memorystreamu to blbne (natahne to obrazek 1x1, asi protoze je ten filestream moc velkej, viz: https://stackoverflow.com/questions/45011973/bitmapimage-from-stream-returns-1x1px-instead-of-the-whole-image/45044968#45044968
                            foto.Open().CopyTo(memoryStream);
                            memoryStream.Position = 0;
                            bitMapImage.BeginInit();
                            bitMapImage.StreamSource = memoryStream;
                            bitMapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitMapImage.EndInit();
                        }
                    }
                    Foto = new System.Windows.Controls.Image();
                    Foto.Source = bitMapImage;                  
                }
            }

        }
        private void LoadInfo(Stream proud) {
            XmlDocument doc = new XmlDocument();
            using (XmlReader r = XmlReader.Create(proud)) {
                doc.Load(r);
                this.Jmeno = doc.GetElementsByTagName("Jmeno")[0].FirstChild.Value;
                this.Prijmeni = doc.GetElementsByTagName("Prijmeni")[0].FirstChild.Value;
                this.Email = doc.GetElementsByTagName("Email")[0].FirstChild.Value;
                this.Telefon = doc.GetElementsByTagName("Telefon")[0].FirstChild.Value;
                this.DatumNarozeni = DateTime.Parse(doc.GetElementsByTagName("DatumNarozeni")[0].FirstChild.Value);
            }
        }
        public void UpdateProperty(string property) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(property)));
        }
        public void Save(string mFileName) {
            //vytvoreni temp dir
            string cestaTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),@"zamestnanci\",Path.GetRandomFileName());
            Directory.CreateDirectory(cestaTemp);
            using (FileStream fs = new FileStream(Path.Combine(cestaTemp, "info.xml"), FileMode.Create)) {
                UlozInfo(fs);
            }
            if (Foto != null) {
                using (FileStream fs = new FileStream(Path.Combine(cestaTemp, "foto.jpg"), FileMode.Create)) {
                    var encoder = new JpegBitmapEncoder();
                    /// mik: ty obrazky delat pres bitMapImage je asi blbost, jednodussi by to bylo pres System.Drawing.Image
                    encoder.Frames.Add(BitmapFrame.Create(bitMapImage));
                    encoder.QualityLevel = 100; // Set quality level 1-100.
                    encoder.Save(fs);
                }
            }
            MessageBox.Show("saved!");

            //nutne pridat externi referenci System.IO.Compression a System.IO.Compression.FileSystem
            System.IO.Compression.ZipFile.CreateFromDirectory(cestaTemp, mFileName);
        }
        private void UlozInfo(Stream proud) {
            XmlDocument doc = new XmlDocument();

            XmlElement zamestnanci = doc.CreateElement("Zamestnanci");
            XmlElement zamestnanec = doc.CreateElement("Zamestnanec");
            zamestnanci.AppendChild(zamestnanec);
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);
            doc.AppendChild(zamestnanci);

            XmlElement Jmeno = doc.CreateElement("Jmeno");
            Jmeno.AppendChild(doc.CreateTextNode(this.Jmeno));

            XmlElement Prijmeni = doc.CreateElement("Prijmeni");
            Prijmeni.AppendChild(doc.CreateTextNode(this.Prijmeni));

            XmlElement Email = doc.CreateElement("Email");
            Email.AppendChild(doc.CreateTextNode(this.Email));

            XmlElement Telefon = doc.CreateElement("Telefon");
            Telefon.AppendChild(doc.CreateTextNode(this.Telefon));

            XmlElement DatumNarozeni = doc.CreateElement("DatumNarozeni");
            DatumNarozeni.AppendChild(doc.CreateTextNode(this.DatumNarozeni.ToShortDateString()));

            zamestnanec.AppendChild(Jmeno);
            zamestnanec.AppendChild(Prijmeni);
            zamestnanec.AppendChild(Email);
            zamestnanec.AppendChild(Telefon);
            zamestnanec.AppendChild(DatumNarozeni);

            doc.Save(proud);
        }


    }
}
