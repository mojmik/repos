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

namespace WpfNakupniSeznam {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        SpravcePolozek s;
        public MainWindow() {
            InitializeComponent();
            s = new SpravcePolozek();
            DataContext = s;
        }

        private void pridejButton_Click(object sender, RoutedEventArgs e) {
            s.Pridat(polozkaTextBox.Text);
        }

        private void odebratButton_Click(object sender, RoutedEventArgs e) {
            if (polozkyListBox.SelectedItem != null) {
                s.Odebrat((Polozka)polozkyListBox.SelectedItem);
            }
            
        }
    }
}
