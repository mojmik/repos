using System.Windows;

namespace Malovani
{
    /// <summary>
    /// Interaction logic for NovyProjektWindow.xaml
    /// </summary>
    public partial class NovyProjektWindow : Window
    {
        /*  _____ _______         _                      _
         * |_   _|__   __|       | |                    | |
         *   | |    | |_ __   ___| |___      _____  _ __| | __  ___ ____
         *   | |    | | '_ \ / _ \ __\ \ /\ / / _ \| '__| |/ / / __|_  /
         *  _| |_   | | | | |  __/ |_ \ V  V / (_) | |  |   < | (__ / /
         * |_____|  |_|_| |_|\___|\__| \_/\_/ \___/|_|  |_|\_(_)___/___|
         *                                _
         *              ___ ___ ___ _____|_|_ _ _____
         *             | . |  _| -_|     | | | |     |  LICENCE
         *             |  _|_| |___|_|_|_|_|___|_|_|_|
         *             |_|
         *
         * IT ZPRAVODAJSTVÍ  <>  PROGRAMOVÁNÍ  <>  HW A SW  <>  KOMUNITA
         *
         * Tento zdrojový kód je součástí výukových seriálů na
         * IT sociální síti WWW.ITNETWORK.CZ
         *
         * Kód spadá pod licenci prémiového obsahu a vznikl díky podpoře
         * našich členů. Je určen pouze pro osobní užití a nesmí být šířen.
         * Více informací na http://www.itnetwork.cz/licence
        */

        int sirka;
        int vyska;

        public int PocetPixeluSirka => sirka;
        public int PocetPixeluVyska => vyska;

        public NovyProjektWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {            
            if (!int.TryParse(sirkaTextBox.Text, out sirka) || sirka < 1 || sirka > 100)
            {
                MessageBox.Show("Šířka musí mít hodnotu 1-100!");
                return;
            }            
            if (!int.TryParse(vyskaTextBox.Text, out vyska) || vyska < 1 || vyska > 100)
            {
                MessageBox.Show("Výška musí mít hodnotu 1-100!");
                return;
            }
            Close();
        }
    }
}
