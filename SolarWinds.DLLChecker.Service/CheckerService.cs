namespace SolarWinds.DLLChecker.Service
{
    using SolarWinds.DLLChecker.Backend;
    using SolarWinds.DLLChecker.DiagnosticsTasksContract;
    using SolarWinds.DLLChecker.Service.Properties;
    using SolarWinds.Logging;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.ServiceProcess;
    using System.Threading;

    internal class CheckerService : ServiceBase
    {
        private DiagnosticsWatcher diagnosticsWatcher;
        private static readonly SolarWinds.Logging.Log Log = new SolarWinds.Logging.Log();
        private DiagnosticsWatcher patternsWatcher;
        private readonly object syncObject = new object();
        private DiagnosticsTasksManager tasksManager;
        private bool unhandledExceptionCaught = false;

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            lock (this.syncObject)
            {
                this.unhandledExceptionCaught = true;
            }
            Log.Fatal("Unhandled exception caught.", e.ExceptionObject as Exception);
        }

        private void InternalStart()
        {
            Action<string> action = null;
            Action<string> action2 = null;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            SolarWinds.Logging.Log.Configure(string.Empty);
            IDisposable disposable = Log.Block();
            try
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.CurrentDomain_UnhandledException);
                this.PrepareFolders();
                this.tasksManager = new DiagnosticsTasksManager();
                this.tasksManager.LoadTasks();
                this.diagnosticsWatcher = new DiagnosticsWatcher();
                if (action == null)
                {
                    action = diagnosticsFilePath => this.tasksManager.ExecuteTasks(diagnosticsFilePath, DiagnosticsTaskType.Comparison);
                }
                this.diagnosticsWatcher.OnDiagnosticsDetected = action;
                this.diagnosticsWatcher.StartWatch(Settings.Default.DiagnosticsPath);
                this.patternsWatcher = new DiagnosticsWatcher();
                if (action2 == null)
                {
                    action2 = diagnosticsFilePath => this.tasksManager.ExecuteTasks(diagnosticsFilePath, DiagnosticsTaskType.PatternCreation);
                }
                this.patternsWatcher.OnDiagnosticsDetected = action2;
                this.patternsWatcher.StartWatch(Settings.Default.PatternsPath);
            }
            catch (Exception exception)
            {
                Log.Fatal("Error starting service.", exception);
                throw;
            }
            finally
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        private void InternalStop()
        {
            try
            {
                lock (this.syncObject)
                {
                    this.diagnosticsWatcher.EndWatch();
                    if (this.unhandledExceptionCaught)
                    {
                        Log.Info("Unhandled exception previously caught, just exiting.");
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error while stopping service: " + exception.Message);
                return;
            }
            base.ExitCode = 0;
        }

        protected override void OnStart(string[] args)
        {
            new Thread(new ThreadStart(this.InternalStart)).Start();
        }

        protected override void OnStop()
        {
            using (Log.Block())
            {
                this.InternalStop();
            }
        }

        private void PrepareFolders()
        {
            if (!Directory.Exists(Settings.Default.DiagnosticsPath))
            {
                Log.Info("Diagnostics folder doesn't exist. Creating " + Settings.Default.DiagnosticsPath);
                Directory.CreateDirectory(Settings.Default.DiagnosticsPath);
            }
            if (!Directory.Exists(Settings.Default.PatternsPath))
            {
                Log.Info("Patterns folder doesn't exist. Creating " + Settings.Default.PatternsPath);
                Directory.CreateDirectory(Settings.Default.PatternsPath);
            }
        }

        internal void RunInConsole()
        {
            SafeNativeMethods.AllocConsole();
            Console.WriteLine("SolarWinds DLL Checker");
            Console.WriteLine("(running in command-line mode)");
            Console.WriteLine();
            Console.WriteLine("Press any key to terminate the application");
            Console.WriteLine();
            this.InternalStart();
            Console.ReadKey();
            this.InternalStop();
            Log.Info("Console mode shutting down");
        }

        private static class SafeNativeMethods
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("kernel32.dll")]
            public static extern bool AllocConsole();
        }
    }
}

