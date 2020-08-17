using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Threading;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrvocisla {
    class MainViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        BackgroundWorker backgroundWorker;

        public List<int> prvocislaList { get; set; } = new List<int>();
        public int progressPct { get; set; }
        public MainViewModel() {
            backgroundWorker = new BackgroundWorker() {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            prvocislaList.Add(1);
            prvocislaList.Add(2);
            prvocislaList.Add(3);
            MikPropertyChanged(nameof(prvocislaList));
            backgroundWorker.RunWorkerAsync(10);
        }
        
        public void MikPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
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
            progressPct = e.ProgressPercentage;
            MikPropertyChanged(nameof(progressPct));
        }
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            MessageBox.Show("Hotovo!");
        }
    }
}
