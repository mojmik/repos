using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfMalovani {
    /// <summary>
    /// Interaction logic for MalovaniWindow.xaml
    /// </summary>
    public partial class MalovaniWindow : Window {
        public Platno Platno { get; set; }
        
        public MalovaniWindow(Platno platno) {
            InitializeComponent();
            Platno = platno;
            this.SizeChanged += ZmenaVelikostiUdalost;
        }
        private void ZmenaVelikostiUdalost(object sender, SizeChangedEventArgs e) {
            RoztahniCanvas();
        }

        private void Barva_MouseDown(object sender, MouseButtonEventArgs e) {
            Platno.VybranaBarva = ((Rectangle)sender).Fill;
        }
       

        public void RoztahniCanvas() {
            int okrajX = 20;
            int okrajY = 30;
            
            int maxSirka = (this.mainGrid.ActualWidth > 0) ? (int)mainGrid.ActualWidth : (int)(Width - okrajX);
            int maxVyska = this.mainGrid.ActualHeight > 0 ? (int)this.mainGridRow.ActualHeight : (int)Height - okrajY;
            malovaniCanvas.Width = maxSirka;
            malovaniCanvas.Height = maxVyska;
            this.Platno.VykresliSe(malovaniCanvas);
        }

        private void zobrazitMrizku_Checked(object sender, RoutedEventArgs e) {
            if (Platno == null) return;
            if (    ((CheckBox)sender).IsChecked == true) {
                Platno.rozestupSirka = 1;
                Platno.rozestupVyska = 1;
            }
            else {
                Platno.rozestupSirka = 0;
                Platno.rozestupVyska = 0;
            }
            Platno.VykresliSe(malovaniCanvas);
        }

        private void vymazatButton_Click(object sender, RoutedEventArgs e) {
            Platno.PrintInfo();
            
        }
    }
}
