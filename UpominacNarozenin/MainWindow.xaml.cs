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

namespace UpominacNarozenin {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private SpravceOsob spravceOsob = new SpravceOsob();

        public MainWindow() {
            //string scriptFile = "WinScript1.wsf";
            //System.Diagnostics.Process.Start("WScript.exe", " " + scriptFile);            
            InitializeComponent();
            try {
                spravceOsob.Nacti();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //nastavime kontext na bindovani; to je vlastne instance, ktera se bude pro vsechna bindovani v okne pouzivat
            DataContext = spravceOsob;
        }

        private void pridatButton_Click(object sender, RoutedEventArgs e) {
            OsobaWindow osobaWindow = new OsobaWindow(spravceOsob);
            osobaWindow.ShowDialog();
        }

        private void odebratButton_Click(object sender, RoutedEventArgs e) {
            if (osobyListBox.SelectedItem != null) {
                try {
                    spravceOsob.Odeber((Osoba)osobyListBox.SelectedItem);
                    spravceOsob.Uloz();
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
