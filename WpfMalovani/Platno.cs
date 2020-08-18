using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfMalovani {
    public class Platno {
        Brush[,] platnoPole;
        public int PixeluRadek { get; set; }
        public int PixeluSloupcu { get; set; }
        int vyskaPixelu;
        int sirkaPixelu;
        bool drziTlacitko;

        public int rozestupSirka = 1;
        public int rozestupVyska = 1;
        int okrajOknoSirka = 1;
        int okrajOknoVyska = 1;
        Canvas canvas;
        public Platno(int sirka, int vyska) {
            PixeluRadek = vyska;
            PixeluSloupcu = sirka;
            platnoPole = new Brush[vyska,sirka];
        }
        public Brush VybranaBarva { get; set; }
        
        public void PrekresliPole(int y, int x) {
            sirkaPixelu = (int)((canvas.Width - rozestupSirka * PixeluSloupcu - okrajOknoSirka) / (PixeluSloupcu));
            vyskaPixelu = (int)((canvas.Height - rozestupVyska * PixeluRadek - okrajOknoVyska) / (PixeluRadek));
            platnoPole[y, x] = VybranaBarva;
            VykresliSe(canvas);
        }
        
        protected void UdalostMouseDown(object sender, MouseButtonEventArgs e) {
            drziTlacitko = true;
            int x = Convert.ToInt32(Canvas.GetLeft((Rectangle)sender)) / (sirkaPixelu + rozestupSirka);
            int y = Convert.ToInt32(Canvas.GetTop((Rectangle)sender)) / (vyskaPixelu + rozestupVyska);
            PrekresliPole(y, x);
        }
        protected void UdalostMouseUp(object sender, MouseButtonEventArgs e) {
            drziTlacitko = false;        
        }
        protected void UdalostMouseEnter(object sender, MouseEventArgs e) {
            if (drziTlacitko) {
                int x = Convert.ToInt32(Canvas.GetLeft((Rectangle)sender)) / (sirkaPixelu + rozestupSirka);
                int y = Convert.ToInt32(Canvas.GetTop((Rectangle)sender)) / (vyskaPixelu + rozestupVyska);
                PrekresliPole(y, x);
            }
        }
        public void PrintInfo() {
            MessageBox.Show($"sirkaPixelu:{sirkaPixelu} vyskaPixelu:{vyskaPixelu} rozestup:{rozestupSirka} {rozestupVyska} ");
        }
        public void VykresliSe(Canvas c) {
            Rectangle r;
            this.canvas = c;

            sirkaPixelu = (int) ((c.Width-rozestupSirka*PixeluSloupcu- okrajOknoSirka) / (PixeluSloupcu));
            vyskaPixelu = (int) ((c.Height- rozestupVyska*PixeluRadek- okrajOknoVyska) / (PixeluRadek));
            int velikostPixelu = Math.Min(sirkaPixelu, vyskaPixelu);
            sirkaPixelu = velikostPixelu;
            vyskaPixelu = velikostPixelu;

            c.Children.Clear();
            for (int y=0;y<PixeluRadek;y++) {
                for (int x=0;x<PixeluSloupcu;x++) {
                    r = new Rectangle {
                        Height=vyskaPixelu,
                        Width=sirkaPixelu
                    };
                    r.MouseDown += UdalostMouseDown;
                    r.MouseUp += UdalostMouseUp;
                    r.MouseEnter += UdalostMouseEnter;
                    r.Fill = platnoPole[y, x] != null ? platnoPole[y, x] : Brushes.Green;                    
                    c.Children.Add(r);
                    Canvas.SetLeft(r, (sirkaPixelu + rozestupSirka -1) * x);                    
                    Canvas.SetTop(r, (vyskaPixelu + rozestupVyska - 1) * y);
                }
            }
        }
    }
}
