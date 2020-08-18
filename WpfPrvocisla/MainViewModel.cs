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
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace WpfPrvocisla {
    class MainViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        BackgroundWorker backgroundWorker;
        object itemsLock = new object();
        public string RetezecPrvocisel { get; private set; }
        public ObservableCollection<string> prvocislaList { get; set; }
        public List<int> cislaList { get; set; }
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
            prvocislaList = new ObservableCollection<string>();
            cislaList = new List<int>();                      
        }


        public void ZacniPocitat(int max) {
            backgroundWorker.RunWorkerAsync(max);            
        }

        public void PocitejPrvocisla(int max) {
            bool[] cisla = new bool[max];
            int i;
            
            int hotovoProcent=0;
            int aktualneHotovoProcent=0;
            cisla[0] = true;
            cisla[1] = true;
            double maxPocet = Math.Sqrt(max);

            for (i = 2; i <= maxPocet; i++) {
                if (cisla[i]) continue;
                aktualneHotovoProcent = (int) ((i / maxPocet) * 100 / 2);
                if (aktualneHotovoProcent != hotovoProcent) {
                    backgroundWorker.ReportProgress(aktualneHotovoProcent);
                    hotovoProcent = aktualneHotovoProcent;
                }
                
                for (int j = 2 * i; j < max; j += i) cisla[j] = true;
                if (backgroundWorker.CancellationPending)
                    break;
            }

            for (i = 2; i < max; i++) {                
                if (!cisla[i]) {            
                        cislaList.Add(i);                                            
                }
                aktualneHotovoProcent = 50 + (int) ((i / (double)max) * 100 / 2);
                if (aktualneHotovoProcent != hotovoProcent) {
                    backgroundWorker.ReportProgress(aktualneHotovoProcent);
                    hotovoProcent = aktualneHotovoProcent;
                }                
                if (backgroundWorker.CancellationPending)
                    break;
            }

    
        }

        public void MikPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            /*
             * PropertyChanged(this,new PropertyChangedEventArgs(propertyName));
             */
        }
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            int max = (int)e.Argument;
            PocitejPrvocisla(max);

            backgroundWorker.ReportProgress(100);
        }
        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            progressPct = e.ProgressPercentage;
            MikPropertyChanged(nameof(progressPct));
        }
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            foreach (int cislo in cislaList) {
                prvocislaList.Add("" + cislo);
                MikPropertyChanged(nameof(prvocislaList));
                RetezecPrvocisel += cislo + ",";
                MikPropertyChanged(nameof(RetezecPrvocisel));
            }            
            MessageBox.Show("Hotovo!");
        }
    }
}
