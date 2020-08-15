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
using Microsoft.Win32;

namespace WpfCanvasKinosal {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Kinosal kinosal = new Kinosal();
        public MainWindow() {
            InitializeComponent();
            kinosal.VlozObdelniky(MikCanvas);

        }

        private void Ulozit_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog dialog = new SaveFileDialog {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true) {
                try {
                    kinosal.Uloz(dialog.FileName);
                }
                catch (Exception E) {
                    MessageBox.Show("Soubor se nepodařilo uložit.", E.Message, MessageBoxButton.OK);
                }
            }
        }
    }

}
