using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;

namespace WpfDispatcherTimer  {
    class MainViewModel : INotifyPropertyChanged {
        //DispatcherTimer neběží v jiném vlákně, kdyz se ma neco delat na pozadi, tak bud novy vlakno nebo BackgroundWorker
        private DispatcherTimer dispatcherTimer;
        public event PropertyChangedEventHandler PropertyChanged;
        int zbyva = 10;
        public MainViewModel() {
            dispatcherTimer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(1)
            };
            /*
             * tohle byl zjednoduseny zapis pro:         
             *  dispatcherTimer = new DispatcherTimer();
             *   dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
             */
            dispatcherTimer.Tick += Timer_Tick;
            dispatcherTimer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e) {
            Zbyva--;
            if (Zbyva == 0)
                dispatcherTimer.Stop();
        }
        public int Zbyva {
            get {
                return zbyva;
            }
            set {
                zbyva = value;
                MikOnPropertyChanged(nameof(Zbyva));
            }
        }
        public void MikOnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
