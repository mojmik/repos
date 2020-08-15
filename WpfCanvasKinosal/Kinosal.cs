using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.IO;

namespace WpfCanvasKinosal {
    class Kinosal {
        private bool[,] sedadla = new bool[30, 15];
        private const int velikost = 16;
        private const int mezera = 2;

        public void VlozObdelniky(Canvas MyCanvas) {
            for (int j = 0; j < sedadla.GetLength(1); j++) {
                for (int i = 0; i < sedadla.GetLength(0); i++) {
                    Rectangle rectangle = new Rectangle {
                        Height = velikost,
                        Width = velikost,
                    };

                    rectangle.Fill = sedadla[i, j] ? Brushes.Red : Brushes.Green;
                    //kliknuti
                    rectangle.MouseDown += UdalostMouseDown;
                    //rectangle.MouseLeave += UdalostMouseMove;
                    rectangle.MouseMove += UdalostMouseMove;
                    MyCanvas.Children.Add(rectangle);

                    Canvas.SetLeft(rectangle, i * (velikost + mezera));
                    Canvas.SetTop(rectangle, j * (velikost + mezera));
                }
            }
        }
        private void UdalostMouseDown(object sender, MouseButtonEventArgs e) {
            Rectangle rectangle = sender as Rectangle;

            rectangle.Fill = (rectangle.Fill == Brushes.Green) ? Brushes.Red : Brushes.Green;


            int px = Convert.ToInt32(Canvas.GetLeft(rectangle)) / (velikost + mezera);
            int py = Convert.ToInt32(Canvas.GetTop(rectangle)) / (velikost + mezera);

            if (px < sedadla.GetLength(0) && py < sedadla.GetLength(1)) PrepniStav(px, py);
        }
        private void UdalostMouseMove(object sender, MouseEventArgs e) {
            Rectangle rectangle = sender as Rectangle;

            rectangle.Fill = (rectangle.Fill == Brushes.Green) ? Brushes.Red : Brushes.Green;


            int px = Convert.ToInt32(Canvas.GetLeft(rectangle)) / (velikost + mezera);
            int py = Convert.ToInt32(Canvas.GetTop(rectangle)) / (velikost + mezera);

            if (px < sedadla.GetLength(0) && py < sedadla.GetLength(1)) PrepniStav(px, py);
        }
        public void PrepniStav(int x, int y) {
            sedadla[x, y] = !sedadla[x, y];
        }
        public void Uloz(string cesta) {
            /*ulozi stav do textaku
             *  101011000100111000000010001100
                000000000000000000000100000000
                000000000000000000010010001000
                001111101011111110000000000100
                000011110100001100101110001000
                001011101001111110000100000000
                000001000100010001111111000000
                000000111001111111111111000100
                000010100001001000100000000010
                100000100100100100101100100000
                001000000000000000000000000000
                000000000000000000000000000000
                000000000000000000000000000000
                000000000000000000000000000000
                000000000000000000000000000000
                Obsazených: 96
                Volných: 354
             */
            using (StreamWriter sw = new StreamWriter(cesta)) {
                int obsazenych = 0;
                for (int j = 0; j < sedadla.GetLength(1); j++) {
                    string radek = "";
                    for (int i = 0; i < sedadla.GetLength(0); i++) {
                        if (sedadla[i, j]) {
                            radek += "1";
                            obsazenych++;
                        }
                        else
                            radek += "0";
                    }
                    sw.WriteLine(radek);
                }
            }
        }
    }
}
