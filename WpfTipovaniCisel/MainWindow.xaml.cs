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

namespace WpfTipovaniCisel {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        Hra h;
        public MainWindow() {
            
            InitializeComponent();
            
        }

        private void rozsahComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void startButton_Click(object sender, RoutedEventArgs e) {
            h = new Hra(Int32.Parse(rozsahComboBox.Text));
            infoTextBox.Text = "ok, myslim si cislo "+h.hadaneCislo;
        }

        private void tipButton_Click(object sender, RoutedEventArgs e) {
            int tip = 0;
            if (h == null) {
                infoTextBox.Text = "Nejdriv zacni hru!";
                return;
            }
            try {                
                tip = Int32.Parse(tipTextBox.Text);
                if (h.Hadej(tip)) {
                    infoTextBox.Text = "Spravne!";
                    MessageBox.Show(h.VypisInfo(true));
                    
                }
                else {
                    infoTextBox.Text = h.VypisInfo(false);
                }
            }
            catch (Exception ex) {
                infoTextBox.Text = "Spatne, to neni cislo!";                
            }
            
        }
    }
}
