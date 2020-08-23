using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registry_UAC {
    class RegistracePriponSouboru {
        //zaregistruje priponu souboru, postara se o UAC
        private string pripona;
        private string cestaKAplikaci;
        public string ikona;
        public RegistracePriponSouboru(string pripona, string cestaKAplikaci) {
            this.pripona = pripona;
            this.cestaKAplikaci = cestaKAplikaci;
        }
        public void PripravIkonu() {
            byte[] ikonaVBytech;
            using (MemoryStream ms = new MemoryStream()) {                
                Properties.Resource1.Mattahan_Umicons_Windows.Save(ms);
                ikonaVBytech = ms.ToArray();                
            }
            string workerIconPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"zamestnanci\worker.ico");
            File.WriteAllBytes(workerIconPath, ikonaVBytech);
            ikona = workerIconPath;
        }
        public bool JeZaregistrovana() {
            String[] keys = Registry.ClassesRoot.GetSubKeyNames();
            return (keys.Contains<String>(this.pripona + ".mik"));
        }
        public void Registruj() {
            try {
                RegistryKey classes = Registry.ClassesRoot;

                // přípona
                RegistryKey extension = classes.CreateSubKey(this.pripona + ".mik");
                RegistryKey shell = extension.CreateSubKey("shell");
                RegistryKey open = shell.CreateSubKey("open");
                RegistryKey command = open.CreateSubKey("Command");
                command.SetValue("", this.cestaKAplikaci + " \"%1\"");

                RegistryKey association = classes.CreateSubKey("." + this.pripona);
                association.SetValue("", this.pripona + ".mik");
                if (ikona != "") {
                    extension.CreateSubKey("DefaultIcon").SetValue("", ikona);
                }
            }
            catch {
                UAC.RestartujSPozadavkemNaZvyseniPrav();
            }
        }
    }
}
