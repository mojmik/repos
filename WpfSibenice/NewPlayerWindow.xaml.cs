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
    /// Interaction logic for NameWindow.xaml
    /// </summary>
    public partial class NewPlayerWindow : Window {
        SpravceHracu spravceHracu;
        private int Skore { get; set; }
        public NewPlayerWindow(int skore) {
            InitializeComponent();
            Skore = skore;
            spravceHracu = new SpravceHracu();
            //DataContext = spravceHracu;
        }

        private void nameOkButton_Click(object sender, RoutedEventArgs e) {
            spravceHracu.Pridej(nameTextBox.Text,"skore: " + Skore);
            SeznamHracuOkno seznamHracuOkno=new SeznamHracuOkno(spravceHracu);
            seznamHracuOkno.Show();
        }
    }
}
