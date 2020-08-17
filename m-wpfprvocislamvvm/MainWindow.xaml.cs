using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace WpfPrvocisla {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        //MainViewModel mv = new MainViewModel();
        public MainWindow() {
            InitializeComponent();
        }
        public void UpdateProgress(int pct) {
            mProgressBar.Value = pct;
        }

        private void hledatButton_Click(object sender, RoutedEventArgs e) {
            //mv.PocitejPrvocisla(int.Parse(maxTextBox.Text));
        }
    }
}
