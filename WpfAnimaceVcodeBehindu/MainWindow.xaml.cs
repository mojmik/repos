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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfAnimaceVcodeBehindu {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        double mikLeft,mikTop,mikRight,mikBottom;
        Thickness aktPoloha;
        public MainWindow() {
            InitializeComponent();
            //DispatcherTimer dt = new DispatcherTimer();
            DispatcherTimer dt = new DispatcherTimer(DispatcherPriority.Render); //- tohle je plynulejsi
            dt.Interval = TimeSpan.FromMilliseconds(50);
            dt.Tick += TimerTick;
            
            aktPoloha = rctMik.Margin;
            mikLeft = aktPoloha.Left;
            mikRight = aktPoloha.Right;
            mikTop = aktPoloha.Top;
            mikBottom = aktPoloha.Bottom;
            
            dt.Start();
        }

        private void TimerTick(object sender, EventArgs e) {
            aktPoloha.Left += 1;
            rctMik.Margin = aktPoloha;
        }

        private void ZmenPruhlednost(object sender, MouseButtonEventArgs e) {
            DoubleAnimation da = new DoubleAnimation();
            da.From = 1;
            da.To = 0;
            da.Duration = new Duration(TimeSpan.FromSeconds(1));
            da.AutoReverse = true;
            da.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard sb = new Storyboard();
            sb.Children.Add(da);
            Storyboard.SetTargetName(da, "rctRamecek");
            Storyboard.SetTargetProperty(da, new PropertyPath(Rectangle.OpacityProperty));
            sb.Begin(this);
        }
        private void MenVelikost(object sender, MouseEventArgs e) {
            DoubleAnimation da = new DoubleAnimation();
            da.From = 300;
            da.To = 10;
            da.Duration = new Duration(TimeSpan.FromSeconds(1));
            da.AutoReverse = true;
            da.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard sb = new Storyboard();
            sb.Children.Add(da);
            Storyboard.SetTargetName(da, "rctRamecek");
            Storyboard.SetTargetProperty(da, new PropertyPath(Rectangle.WidthProperty));
            sb.Begin(this);
        }       

    }
}
