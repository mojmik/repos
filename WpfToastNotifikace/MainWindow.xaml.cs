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
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace WpfToastNotifikace {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }
        Notifier zpravaObrazovka = new Notifier(cfg =>
        {
            cfg.PositionProvider = new PrimaryScreenPositionProvider(
                Corner.BottomRight, 10, 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(5),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });


        /// <summary>
        /// Definice zprávy pro okno
        /// </summary>
        Notifier zpravaOkno = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.BottomRight,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(5),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });
        private void ZobrazSucc(object sender, RoutedEventArgs e) {
            zpravaOkno.ShowSuccess("Zpracování proběhlo úspěšně.");
        }
        private void ZobrazInfo(object sender, RoutedEventArgs e) {
            zpravaObrazovka.ShowInformation("Je právě " +
                DateTime.Now.ToString("HH:mm") + " hodin.");
        }

     
        private void ZobrazWar(object sender, RoutedEventArgs e) {
            zpravaObrazovka.ShowWarning("Nejsou vyplněny všechny povinné údaje.");
        }

        private void ZobrazError(object sender, RoutedEventArgs e) {
            zpravaObrazovka.ShowError("Ve zpracování se vyskytly chyby.");
        }

        private void Konec(object sender, RoutedEventArgs e) {
            zpravaObrazovka.Dispose();
            zpravaOkno.Dispose();
            wdwTN.Close();
        }
        /// <summary>
        /// Vyčištění fronty při ukončení aplikace křížkem (událost Closed)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VycistiFrontu(object sender, EventArgs e) {
            zpravaObrazovka.Dispose();
            zpravaOkno.Dispose();
        }

    }
    /// <summary>
    /// Definice zprávy pro obrazovku
    /// </summary>
    
}
