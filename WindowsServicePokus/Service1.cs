using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServicePokus {
    public partial class Service1 : ServiceBase {
        // private ServiceA_1 service11; //reference na službu
        PerformanceCounter counter;
        System.Timers.Timer timer = new System.Timers.Timer();
        string fileName;
        bool isStopping;

        protected override void OnStart(string[] args) {
            // vytvořit performance counter pro zjištění zatížení procesoru
            counter = new PerformanceCounter();
            counter.CategoryName = "Processor";
            counter.CounterName = "% Processor Time";
            counter.InstanceName = "_Total";
            // cesta, kam se budou data ukládat libovolný adresář musí existovat !!!
            // zápis musí být v tomto tvaru nesmí být ....\\temp\\....
            fileName = @"c:\temp\cpu.txt";
            // vytvořit timer, který bude každou vteřinu data zapisovat
            timer.Interval = 1000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.AutoReset = false;
            timer.Start();
        }

        protected override void OnStop() {
            isStopping = true;
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            // zastavuje se služba? pak neprovádět další krok
            if (isStopping)
                return;
            try {
                // naformátovat hlášku, která bude do logu zapsána
                string message =
                    string.Format("{0:HH:mm:ss} - CPU usage {1}%",
                    DateTime.Now,
                    counter.NextValue());
                // zapsat hlášku
                System.IO.File.AppendAllText(fileName, message + Environment.NewLine);
            }
            finally {
                // po provedení kroku znovu spustit časovač
                timer.Start();
            }
        }
    }
}
