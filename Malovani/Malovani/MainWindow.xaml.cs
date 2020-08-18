using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Malovani
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
                
        int pocetPixeluSirka;
        int pocetPixeluVyska;
        double velikostPixelu;
        bool jeStisknuto;
        Brush vybranaBarva = Brushes.Black;

        public bool ZobrazovatMrizku
        {
            set
            {
                if (value)
                    ZmenMrizku(0.25);
                else
                    ZmenMrizku(0);
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            VytvorPixely();
        }


        private void NovyProjektButton_Click(object sender, RoutedEventArgs e)
        {
            NovyProjektWindow novyProjekt = new NovyProjektWindow();
            novyProjekt.ShowDialog();

            if (novyProjekt.PocetPixeluSirka == 0)
                return;

            pocetPixeluSirka = novyProjekt.PocetPixeluSirka;
            pocetPixeluVyska = novyProjekt.PocetPixeluVyska;            

            VytvorPixely();
            NastavVelikostPixelu();
        }

        private void VymazatVseButton_Click(object sender, RoutedEventArgs e)
        {            
            VytvorPixely();
            NastavVelikostPixelu();
        }

        private void UlozitButton_Click(object sender, RoutedEventArgs e)
        {
            // Když není vytvořený projekt, tak se nepokračuje.
            if (canvas.Width == 0)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "PNG obrázky (*.png)|*.png|All files (*.*)|*.*" };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    WriteableBitmap wbitmap = new WriteableBitmap(pocetPixeluVyska, pocetPixeluSirka, 96, 96, PixelFormats.Bgra32, null);

                    // Aby šel obrázek uložit, pixely v bitmapě musí být uloženy jako pole 4 hodnot pro každý pixel (modrá složka, zelená složka, červená složka, průhlednost)
                    byte[] pixely = new byte[pocetPixeluVyska * pocetPixeluSirka * 4];
                    int indexPixelu = 0; // index pixelu na plátnu
                    int indexHodnoty = 0; // index hodnoty v poli bitmapy
                    for (int i = 0; i < pocetPixeluVyska * pocetPixeluSirka; i++)
                    {
                        // Vezme pixel z canvasu
                        Rectangle pixel = (Rectangle)canvas.Children[indexPixelu++];
                        Color barva = ((SolidColorBrush)pixel.Fill).Color;
                        // Pro každý pixel na canvasu zapíšeme 4 hodnoty do bitmapy
                        pixely[indexHodnoty++] = barva.B; // modrá složka barvy
                        pixely[indexHodnoty++] = barva.G; // zelená složka barvy
                        pixely[indexHodnoty++] = barva.R; // červená složka barvy
                        pixely[indexHodnoty++] = 255; // neprůhledné
                    }
                    // Nakreslíme pixely na bitmapu
                    Int32Rect rect = new Int32Rect(0, 0, pocetPixeluVyska, pocetPixeluSirka);
                    int sirkaRadky = 4 * pocetPixeluVyska; // 4 hodnoty na jedné řádce
                    wbitmap.WritePixels(rect, pixely, sirkaRadky, 0);
                    // Uložíme bitmapu jako PNG soubor
                    using (FileStream stream = File.Create(saveFileDialog.FileName))
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        TransformedBitmap orotovana = new TransformedBitmap(wbitmap, new RotateTransform(90)); // Bitmapu musíme orotovat, protože náš editor ukládá řádky * sloupce, ale bitmapa sloupce * řádky
                        TransformedBitmap zrcadlena = new TransformedBitmap(orotovana, new ScaleTransform(-1, 1, 0, 0)); // Ze stejného důvodu ji i zrcadlíme
                        encoder.Frames.Add(BitmapFrame.Create(zrcadlena));
                        encoder.Save(stream);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void pixel_MouseEnter(object sender, MouseEventArgs e)
        {
            if (jeStisknuto)
                ((Rectangle)sender).Fill = vybranaBarva;
        }

        private void pixel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Při odkliknutí tlačítka myši se zruší stisknutí.
            jeStisknuto = false;
        }

        private void pixel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            jeStisknuto = true;

            ((Rectangle)sender).Fill = vybranaBarva;
        }

        private void canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            // Když myš opustí canvas, tak se zruší stisknutí.
            jeStisknuto = false;
        }

        /// <summary>
        /// Výběr barvy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Barva_MouseDown(object sender, MouseButtonEventArgs e)
        {
            vybranaBarva = ((Rectangle)sender).Fill;
        }

        /// <summary>
        /// Vykreslí jednotlivé pixely na canvas a přiřadí jejich událostem dané metody.
        /// </summary>
        private void VytvorPixely()
        {
            canvas.Children.Clear();

            for (int j = 0; j < pocetPixeluSirka; j++)
            {
                for (int i = 0; i < pocetPixeluVyska; i++)
                {
                    Rectangle pixel = new Rectangle();
                    pixel.Fill = Brushes.White;
                    pixel.StrokeThickness = zobrazovatMrizkuCheckBox.IsChecked == true ? 0.25 : 0;
                    pixel.Stroke = Brushes.Black;
                    pixel.MouseEnter += pixel_MouseEnter;
                    pixel.MouseUp += pixel_MouseUp;
                    pixel.MouseDown += pixel_MouseDown;
                    canvas.Children.Add(pixel);
                }
            }
        }

        private void NastavVelikostPixelu()
        {
            // Spočítá velikost pixelů jako obdélníků tak, aby při současném rozměru plátna a projektu pokryly celé plátno (např. plátno 1000x500px bude mít pixel projektu 10x10 bodů velký 100x50px)
            double sirkaPixelu = canvas.ActualWidth / pocetPixeluSirka;
            double vyskaPixelu = canvas.ActualHeight / pocetPixeluVyska;
            // protože pixely chceme čtvercové, vybereme tu menší hranu
            velikostPixelu = Math.Min(sirkaPixelu, vyskaPixelu);

            int index = 0;

            for (int j = 0; j < pocetPixeluSirka; j++)
            {
                for (int i = 0; i < pocetPixeluVyska; i++)
                {
                    Rectangle pixel = (Rectangle)canvas.Children[index];

                    index++;

                    pixel.Width = velikostPixelu + 1;
                    pixel.Height = velikostPixelu + 1;

                    Canvas.SetLeft(pixel, j * velikostPixelu);
                    Canvas.SetTop(pixel, i * velikostPixelu);
                }
            }
        }

        /// <summary>
        /// Nastaví požadovanou tloušťku mřížky.
        /// </summary>
        /// <param name="tloustka"></param>
        private void ZmenMrizku(double tloustka)
        {
            foreach (var pixel in canvas.Children)
                ((Rectangle)pixel).StrokeThickness = tloustka;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NastavVelikostPixelu();
        }
    }
}
