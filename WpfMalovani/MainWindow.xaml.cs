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

namespace WpfMalovani {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        SpravceMalovani sm;
        public MainWindow() {
            InitializeComponent();
            sm = new SpravceMalovani();
        }

        private void novyButton_Click(object sender, RoutedEventArgs e) {
            if (sm.VytvorPlatno(vyskaTextBox.Text, sirkaTextBox.Text)) {
                sm.JdemeMalovat();
            }
        }
    }
}
