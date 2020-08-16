using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfBackgroundWorker {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        BackgroundWorker backgroundWorker;
   

    public MainWindow() {
            InitializeComponent();
            backgroundWorker= new BackgroundWorker() {                 
                     WorkerSupportsCancellation = true,
                     WorkerReportsProgress = true
                 };
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }
        

        private void Spustit_Click(object sender, RoutedEventArgs e) {
            if (int.TryParse(Vteriny.Text, out int hodnota))
                backgroundWorker.RunWorkerAsync(hodnota);
            else
                MessageBox.Show("Zadané vteřiny nejsou ve správném formátu!");
        }

        private void Zastavit_Click(object sender, RoutedEventArgs e) {
            backgroundWorker.CancelAsync();
        }
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            int pocet = (int)e.Argument;
            for (int i = 1; i <= pocet; i++) {
                Thread.Sleep(1000);
                int procent = (int)Math.Round((i / (double)pocet) * 100);
                backgroundWorker.ReportProgress(procent);
                if (backgroundWorker.CancellationPending)
                    break;
            }
        }
        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            vterinyProgressBar.Value = e.ProgressPercentage;
        }
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            MessageBox.Show("Hotovo!");
        }

    }
}
