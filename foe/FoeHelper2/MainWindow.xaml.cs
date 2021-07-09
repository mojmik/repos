using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;

namespace FoeHelper2
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;        
        private Task workerTask;
        CancellationTokenSource ts;
        CancellationToken ct;
        FoeTools foeTools;

        private string mOut;
        public string Mout
        {
            get { return mOut; }
            set
            {
                mOut = value;
                RaisePropertyChanged("Mout");
            }
        }
        private int mainState;
        public MainWindow()
        {
            InitializeComponent();
            foeTools = new FoeTools(this);
            this.DataContext = this;
        }



        private void GoPixel(object sender, RoutedEventArgs e)
        {
            
            ts = new CancellationTokenSource();
            ct = ts.Token;
            if (mainState > 0)
            {
                ts.Cancel();
                Mout = "Cancelled";
                return;
            }            
            workerTask=Task.Factory.StartNew(() =>
            {
                mainState = 1;
                foeTools.initScreen();
                /*
                for (int x = 0; x < 100; x++)
                {
                    //System.Threading.Thread.Sleep(2);
                    Mout = px.GetColorAt(100 + x, 100);
                    //MouseTools.MoveMouse(1636 + x, 627,0,0);
                    //MouseTools.DoMouseClick();
                }                                
                */                
                mainState = 0;
            },ct);
            
        }
        private void GoKnajpa(object sender, RoutedEventArgs e)
        {
            ts = new CancellationTokenSource();
            ct = ts.Token;
            if (mainState > 0)
            {
                ts.Cancel();
                mainStatusTextBlock.Text = "Cancelled";
                return;
            }
            mainStatusTextBlock.Text = "Running";
            workerTask = Task.Factory.StartNew(() =>
            {
                mainState = 1;
                foeTools.goHelp();
                /*
                for (int x = 0; x < 100; x++)
                {
                    //System.Threading.Thread.Sleep(2);
                    Mout = px.GetColorAt(100 + x, 100);
                    //MouseTools.MoveMouse(1636 + x, 627,0,0);
                    //MouseTools.DoMouseClick();
                }                
                Mout = Mout + " and done";
                */
                mainState = 0;
            }, ct);

        }

        private void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
