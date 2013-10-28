namespace SolarWinds.DLLChecker.Service
{
    using System;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.ServiceProcess;

    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            InitializeComponent();
            var process = new ServiceProcessInstaller();

            process.Account = ServiceAccount.LocalSystem;

            var serviceAdmin = new ServiceInstaller();

            serviceAdmin.StartType = ServiceStartMode.Automatic;
            serviceAdmin.AfterInstall += (sender, args) => new ServiceController(serviceAdmin.ServiceName).Start();
            serviceAdmin.ServiceName = "SWDLLCheckerService";
            serviceAdmin.DisplayName = "SolarWinds DLL Checker Service";
            serviceAdmin.Description = "Automatically processes diagnostics in chosen folders";
            Installers.Add(process);
            Installers.Add(serviceAdmin);
        }
    }
}

