using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace WpfPrvocisla {
    class MainViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        BackgroundWorker backgroundWorker;

        public List<int> prvocislaList { get; set; } = new List<int>();
        public int progressPct { get; set; }
        public string maxText { get; set; }
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
            
            ButtonCommand = new RelayCommand(o => MainButtonClick("hledatButton"));
        }
        
        
        

        public void PocitejPrvocisla(int max) {
            bool[] cisla = new bool[max];
            int i;
            cisla[0] = true;
            cisla[1] = true;
            for (i = 2; i <= Math.Sqrt(max); i++) {
                if (cisla[i]) continue;
                backgroundWorker.ReportProgress((int)(i/(double)max*100));
                for (int j = 2 * i; j < max; j += i) cisla[j] = true;
                if (backgroundWorker.CancellationPending)
                    break;
            }
            for (i=2;i<max;i++) {
                if (!cisla[i]) prvocislaList.Add(i);
            }
            MikPropertyChanged(nameof(prvocislaList));
        }
        
        public void MikPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
        }
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            int max= (int)e.Argument;
            PocitejPrvocisla(max);
        }
        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            progressPct = e.ProgressPercentage;
            MikPropertyChanged(nameof(progressPct));
        }
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            MessageBox.Show("Hotovo!");
        }


        public ICommand ButtonCommand { get; set; }

        private void MainButtonClick(object sender) {
            //MessageBox.Show(sender.ToString() + " " +maxText);
            if (sender.ToString()=="hledatButton") PocitejPrvocisla(int.Parse(maxText));
        }
    }
}
