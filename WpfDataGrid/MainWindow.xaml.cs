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

namespace WpfDataGrid {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private SpravceOsob spravceOsob = new SpravceOsob();
        public MainWindow() {
            InitializeComponent();
            OsobyDataGrid.ItemsSource = spravceOsob.osoby;
        }

        private void Vypis_Click(object sender, RoutedEventArgs e) {
            if (OsobyDataGrid.SelectedItem != null) {

                /*                
                object o1 = OsobyDataGrid.SelectedItem; //vrati Osoboa
                object o=OsobyDataGrid.SelectedCells[0].Column.GetCellContent(o1); //vrati TextBlock
                TextBlock t = (TextBlock)(o);
                string s = t.Text;
                int id = Convert.ToInt32(s);
                a tohle na jeden radek:
                */                
                int id = Convert.ToInt32((OsobyDataGrid.SelectedCells[0].Column.GetCellContent(OsobyDataGrid.SelectedItem) as TextBlock).Text);

                Osoba osoba = (Osoba)OsobyDataGrid.SelectedItem;
                MessageBox.Show("Jméno osoby získané z objektu je " + osoba.Jmeno + " a Id získané z tabulky je " + id);
            }
        }
    }
}
