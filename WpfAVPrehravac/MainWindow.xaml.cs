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

namespace WpfAVPrehravac {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Boolean hraje = false;

        private BitmapImage obrPauza = new BitmapImage(new Uri("pack://application:,,,/WpfAVPrehravac;component/Obrazky/pause.png"));
        private BitmapImage obrHraj = new BitmapImage(new Uri("pack://application:,,,/WpfAVPrehravac;component/Obrazky/prehraj.png"));
        //private BitmapImage obrPauza = new BitmapImage();
        //private BitmapImage obrHraj = new BitmapImage();

        private ImageBrush imgPauza = new ImageBrush();
        private ImageBrush imgHraj = new ImageBrush();

        /// <summary>
        /// Konstruktor třídy
        /// </summary>
        public MainWindow() {
            InitializeCom­ponent();
            NastavTlacitka(0);

            imgPauza.Image­Source = obrPauza;
            imgHraj.ImageSource = obrHraj;
        }

        #region Procedury okna =======================================
        /// <summary>
        /// Nastavení přístupu na tlačítka
        /// </summary>
        /// <param name="status"></param>
        private void NastavTlacitka(int status) {
            switch (status) {
                case 0:
                    btnHraj.IsEnabled = false;
                    btnHraj.Opacity = 0.5;
                    btnZavri.IsEnabled = false;
                    btnZavri.Opacity = 0.5;
                    break;
                case 1:
                    btnHraj.IsEnabled = true;
                    btnHraj.Opacity = 1;
                    btnZavri.IsEnabled = false;
                    btnZavri.Opacity = 0.5;
                    break;
                case 2:
                    btnHraj.IsEnabled = true;
                    btnHraj.Opacity = 1;
                    btnZavri.IsEnabled = true;
                    btnZavri.Opacity = 1;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Zavření okna - tlačítkem Konec
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZavriOkno(object sender, RoutedEventArgs e) {
            ZrusPrehravani();
            wdwPrehravac.Close();
        }


        /// <summary>
        /// Zavření okna - křížkem u okna
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Zavri(object sender, System.ComponentModel.CancelEventArgs e) {
            ZrusPrehravani();
        }
        #endregion

    
        #region Procedury videa ======================================
        /// <summary>
        /// Výběr videa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOtevri(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video soubory (*.mpg; *.mpeg; *.avi; *.mp4)| *.mpg; *.mpeg; *.avi; *.mp4";
            if (openFileDialog.ShowDialog() == true) {
                ZrusPrehravani();
                avPrehravac.Source = new Uri(openFileDialog.FileName);
                NastavTlacitka(1);
            }
        }


        /// <summary>
        /// Spuštění nebo pozastavení videa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrehraj(object sender, RoutedEventArgs e) {
            if (hraje == false) {
                Prehraj();
                NastavTlacitka(3);
            }
            else {
                Pozastav();
                NastavTlacitka(1);
            }
        }


        /// <summary>
        /// Zavření videa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnZavri(object sender, RoutedEventArgs e) {
            ZrusPrehravani();
        }


        /// <summary>
        /// Přehraje video
        /// </summary>
        private void Prehraj() {
            avPrehravac.Play();
            hraje = true;
            btnHraj.Background = imgPauza;
            btnHraj.ToolTip = "Pozastavit video";
            NastavTlacitka(2);
        }

        private void Pozastav() {
            avPrehravac.Pause();
            hraje = false;
            btnHraj.Background = imgHraj;
            btnHraj.ToolTip = "Přehrát video";
            NastavTlacitka(1);
        }

        private void NastavKonec(object sender, RoutedEventArgs e) {
            avPrehravac.Stop();
            hraje = false;
            btnHraj.Background = imgHraj;
            btnHraj.ToolTip = "Přehrát video";
            NastavTlacitka(1);
        }

        private void ZrusPrehravani() {
            avPrehravac.Stop();
            avPrehravac.Close();
            hraje = false;
            btnHraj.Background = imgHraj;
            btnHraj.ToolTip = "Přehrát video";
            NastavTlacitka(0);
        }
        #endregion
    }
}
