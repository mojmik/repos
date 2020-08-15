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

namespace WpfSibenice {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        SpravceHry spravceHry;
        public MainWindow() {
            InitializeComponent();
            spravceHry = new SpravceHry();
            DataContext = spravceHry;
            UkazatObrazky();
            
        }

        private void tipButton_Click(object sender, RoutedEventArgs e) {
            try {
                spravceHry.Hadej(tipTextBox.Text);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UkazatObrazky();
            if (spravceHry.Prohra()) {
                new ProhraWindow(spravceHry.HadaneSlovo.TextSlova).ShowDialog();

            }
            if (spravceHry.Vyhra()) {                
                PridatSkore();
            }
        }
        private void PridatSkore() {
            NewPlayerWindow nw = new NewPlayerWindow(spravceHry.AktualniSkore());
            nw.ShowDialog();
        }
        
        public void UkazatObrazky() {
            int pokusy = spravceHry.SpatnePokusy;
            par1.Visibility = pokusy >= 1 ? Visibility.Visible : Visibility.Hidden;
            par2.Visibility = pokusy >= 2 ? Visibility.Visible : Visibility.Hidden;
            par3.Visibility = pokusy >= 3 ? Visibility.Visible : Visibility.Hidden;
            par4.Visibility = pokusy >= 4 ? Visibility.Visible : Visibility.Hidden;
            par5.Visibility = pokusy >= 5 ? Visibility.Visible : Visibility.Hidden;
            par6.Visibility = pokusy >= 6 ? Visibility.Visible : Visibility.Hidden;
            par7.Visibility = pokusy >= 7 ? Visibility.Visible : Visibility.Hidden;
            par8.Visibility = pokusy >= 8 ? Visibility.Visible : Visibility.Hidden;
            par9.Visibility = pokusy >= 9 ? Visibility.Visible : Visibility.Hidden;
            par10.Visibility = pokusy >= 10 ? Visibility.Visible : Visibility.Hidden;
            par11.Visibility = pokusy >= 11 ? Visibility.Visible : Visibility.Hidden;
            

        }

        private void restartButton_Click(object sender, RoutedEventArgs e) {
            spravceHry.Restart();
            UkazatObrazky();
        }

        private void topButton_Click(object sender, RoutedEventArgs e) {
            SpravceHracu sh = new SpravceHracu();
            SeznamHracuOkno okno = new SeznamHracuOkno(sh);
            okno.ShowDialog();
        }
    }
}
