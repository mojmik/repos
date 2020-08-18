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

namespace WpfTamagotchi {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        GameManager gm;
        public MainWindow() {
            InitializeComponent();
            gm = new GameManager();
            DataContext = gm;

        }

        private async void foodButton_Click(object sender, RoutedEventArgs e) {
            foodButton.IsEnabled = false;
            foodButton.Content = "xxx";
            gm.Food();
            foodButton.IsEnabled = true;
            foodButton.Content = "Krmeni";
        }

        private async void sleepButton_Click(object sender, RoutedEventArgs e) {
            gm.Sleep();
        }

        private async void hygieneButton_Click(object sender, RoutedEventArgs e) {
            gm.Hygiene();
        }

        private async void walkButton_Click(object sender, RoutedEventArgs e) {
            gm.Walk();
        }
    }
}
