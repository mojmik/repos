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
using System.Windows.Threading;

namespace WpfDispatcherTimerHodiny {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        DispatcherTimer timer;
        int predchoziSekundy = 0;
        enum TypHodin {
            Analogove, Digitalni, Kombinovane
        }
        public MainWindow() {
            InitializeComponent();
            timer = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(10)
            };

            timer.Tick += Timer_Tick;
            timer.Start();
        }
        public void Timer_Tick(object sender, EventArgs e) {
            if (DateTime.Now.Second != predchoziSekundy) {
                Sekundy.Angle = DateTime.Now.Second * 6;
                Minuty.Angle = (DateTime.Now.Minute * 6) + (DateTime.Now.Second * 0.1);
                Hodiny.Angle = (DateTime.Now.Hour * 30) + (DateTime.Now.Minute * 0.5);

                if (Format24.IsChecked.Value)
                    DigitalHodiny.Content = DateTime.Now.ToLongTimeString();
                else
                    DigitalHodiny.Content = DateTime.Now.ToString("hh:mm:ss");
            }

            predchoziSekundy = DateTime.Now.Second;
        }

        private void Analogove_Checked(object sender, RoutedEventArgs e) {
            Visibility viditelnost = Analogove.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
            ZobrazPozadovaneHodiny(TypHodin.Analogove, viditelnost);
        }

        private void Digitalni_Checked(object sender, RoutedEventArgs e) {
            Visibility viditelnost = Digitalni.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
            ZobrazPozadovaneHodiny(TypHodin.Digitalni, viditelnost);
        }

        private void Kombinovane_Checked(object sender, RoutedEventArgs e) {
            Visibility viditelnost = Kombinovane.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
            ZobrazPozadovaneHodiny(TypHodin.Kombinovane, viditelnost);
        }

        private void Format24_Checked(object sender, RoutedEventArgs e) {

        }
        private void ZobrazPozadovaneHodiny(TypHodin typ, Visibility viditelnost) {
            switch (typ) {
                case TypHodin.Analogove:
                    DigitalHodiny.Visibility = viditelnost == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;

                case TypHodin.Digitalni:
                    DigitalHodiny.Visibility = viditelnost;
                    viditelnost = viditelnost == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;

                case TypHodin.Kombinovane:
                    DigitalHodiny.Visibility = viditelnost;
                    break;
            }

            Pozadi.Visibility = viditelnost;
            OHodiny.Visibility = viditelnost;
            OMinuty.Visibility = viditelnost;
            OSekundy.Visibility = viditelnost;
        }
    }
}
