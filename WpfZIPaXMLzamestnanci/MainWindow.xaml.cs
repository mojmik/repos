using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfZIPaXMLzamestnanci {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// mik: ty obrazky delat pres bitMapImage je asi blbost, jednodussi by to bylo pres System.Drawing.Image
    public partial class MainWindow : Window {
        Zamestnanec z;
        public MainWindow() {
            InitializeComponent();
            z = new Zamestnanec();
            DataContext = z;
            string[] parametrySpusteni = Environment.GetCommandLineArgs();
            if (parametrySpusteni.Length > 1) {
                MessageBox.Show("nahravam z parametru");
                z.Load(parametrySpusteni[1]);                
            }            
        }


        private void selectImage_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Fotka JPG (*.jpg)|*.jpg|Fotka PNG (*.png)|*.png|Fotka BMP (*.bmp)|*.bmp";
            if (dialog.ShowDialog()==true) {                
                z.bitMapImage= new BitmapImage(new Uri(@"" + dialog.FileName)); //vytvoreni novy bitmapy
                z.Foto = new System.Windows.Controls.Image();                
                z.Foto.Source = z.bitMapImage;                                
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = Zamestnanec.filter;
            dialog.FileName = z.Jmeno + " " + z.Prijmeni;
            if (dialog.ShowDialog() == true) {
                z.Save(dialog.FileName);
            }            
        }

        private void loadButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = Zamestnanec.filter;
            if (dialog.ShowDialog()==true) {
                z.Load(dialog.FileName);
            }
            
        }
    }
}
