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
using System.Windows.Shapes;

namespace WpfSibenice {
    /// <summary>
    /// Interaction logic for ProhraWindow.xaml
    /// </summary>
    public partial class ProhraWindow : Window {
        public ProhraWindow(string hledaneSlovo) {
            InitializeComponent();
            hledaneSlovoTextBlock.Text = hledaneSlovo;
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
