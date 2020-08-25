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

namespace SlovnicekWpfaDB {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    /// viz. tohle: https://www.itnetwork.cz/csharp/databaze/c-sharp-tutorial-linq-to-sql-classes
    /// ale uz se to prej moc nepouziva
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataClasses1DataContext kontext = new DataClasses1DataContext();
            mDataGrid.ItemsSource = kontext.Words;
        }
    }
}
