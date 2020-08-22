using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace SlovickaZkouseni {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        ZkouseniSlovicek z;
        public MainWindow() {
            InitializeComponent();
            z = new ZkouseniSlovicek();
            z.Losuj();            
            oriTextBox.Text = z.VypisOri();            
            infoTextBlock.Content = "";
        }

        private void OK_Click(object sender, RoutedEventArgs e) {
            if (z.CheckPreklad(prekladTextBox.Text)) {
                infoTextBlock.Content += $"Spravne! {z.VypisOri()} - {z.VypisPreklad()} \n";                
            }    
            else {

                infoTextBlock.Content += $"Spatne! Spravne slovo bylo {z.VypisPreklad()}\n";
            }
            z.Losuj();
            scoreTextBlock.Text = "";
            oriTextBox.Text = z.VypisOri();
            scoreTextBlock.Text = z.VypisSkore();
            prekladTextBox.Text = "";
            if (z.isKonec()) {
                z.zapisVysledky();                                
                this.Close();
            }
        }

        private void prekladTextBox_TextChanged(object sender, TextChangedEventArgs e) {

        }

        private void prekladTextBox_GotFocus(object sender, RoutedEventArgs e) {
            prekladTextBox.Text = "";
            prekladTextBox.Foreground = Brushes.Black;
        }

        private void prekladTextBox_LostFocus(object sender, RoutedEventArgs e) {
            prekladTextBox.Text = "preklad..";
            prekladTextBox.Foreground = Brushes.Gray;
        }
    }
}
