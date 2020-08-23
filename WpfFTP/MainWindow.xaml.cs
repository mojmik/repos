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

namespace WpfFTP {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        FileManager fileMan;

        public MainWindow() {
            
            InitializeComponent();
            fileMan = new FileManager();
            DataContext = fileMan;
        }

        private void goButton_Click(object sender, RoutedEventArgs e) {
            fileMan.ReadFiles(); 
        }

        private void goDownload_Click(object sender, RoutedEventArgs e) {
            try {
                if (outListBox.SelectedIndex >= 0) fileMan.Download(fileMan.Files[outListBox.SelectedIndex]);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
