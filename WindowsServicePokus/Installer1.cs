using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.ServiceProcess.Design;

namespace WindowsServicePokus {
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer {


        //Konstanty
        private const string DESKRIPTION = "Stručný komentář popis a účel služby ";
        private const string DISPLAYNAME = "Moje_Service_1";
        private const string SERVICENAME = "MojeService1";

        private ServiceInstallerDialog sid = new ServiceInstallerDialog();   //1
        private ServiceInstaller serviceInstaller = new ServiceInstaller();
        private ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();

        public Installer1() {            
            InstallServiceProcessInstaller();
            InstallServiceInstaller();
            InitializeComponent();
        }

        //Informace o službách
        private void InstallServiceInstaller() {
            this.serviceInstaller.ServiceName = SERVICENAME;
            this.serviceInstaller.DisplayName = DISPLAYNAME;
            this.serviceInstaller.Description = DESKRIPTION;
            this.serviceInstaller.StartType = ServiceStartMode.Manual;
            this.serviceInstaller.Parent = this;
        }

        private void InstallServiceProcessInstaller() {
            this.serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            this.serviceProcessInstaller.Parent = this;
            this.serviceProcessInstaller.Password = null;    //2
            this.serviceProcessInstaller.Username = null;    //3
        }
        //Zde můžeme provádět kontrolu jména a hesla při Users módu .
    }
}
