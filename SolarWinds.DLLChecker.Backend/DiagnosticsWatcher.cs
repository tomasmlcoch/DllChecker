namespace SolarWinds.DLLChecker.Backend
{
    using SolarWinds.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public class DiagnosticsWatcher
    {
        private static SolarWinds.Logging.Log Log = new SolarWinds.Logging.Log();
        private FileSystemWatcher watcher;

        private void CheckAccessibility(string zipPath)
        {
            Log.Info(string.Format("Checking {0} accessibility", zipPath));
            int num = 0x3e8;
            while (true)
            {
                if (!this.IsFileLocked(zipPath))
                {
                    Log.Info(string.Format("{0} is accessible", zipPath));
                    return;
                }
                if (num >= 0xdbba0)
                {
                    throw new FieldAccessException(string.Format("AssemblyFile {0} is inaccessible for long time", zipPath));
                }
                Log.Warn(string.Format("{0} is inaccessible, retrying in {1} ms", zipPath, num));
                Thread.Sleep(num);
                num *= 2;
            }
        }

        public void CheckExistingFiles(string directory)
        {
            using (IEnumerator<string> enumerator = Directory.EnumerateFiles(directory, "*.zip", SearchOption.AllDirectories).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Action action = null;
                    string zipFile = enumerator.Current;
                    string path = Path.Combine(Path.GetDirectoryName(zipFile), Path.GetFileNameWithoutExtension(zipFile) + "_NoIssue.html");
                    string str2 = Path.Combine(Path.GetDirectoryName(zipFile), Path.GetFileNameWithoutExtension(zipFile) + "_IssueFound.html");
                    string str3 = Path.Combine(Path.GetDirectoryName(zipFile), Path.GetFileNameWithoutExtension(zipFile) + "_REPORT.html");
                    if ((!File.Exists(path) && !File.Exists(str2)) && !File.Exists(str3))
                    {
                        if (action == null)
                        {
                            action = () => this.RaiseOnDiagnosticsDetected(zipFile);
                        }
                        Task.Factory.StartNew(action);
                    }
                }
            }
        }

        public void EndWatch()
        {
            this.watcher.EnableRaisingEvents = false;
            Log.Info("Diagnostics watcher stopped");
        }

        private void InspectChanges(object sender, FileSystemEventArgs e)
        {
            Action action = null;
            if (Directory.Exists(e.FullPath))
            {
                string[] strArray = Directory.GetFiles(e.FullPath, "*.zip", SearchOption.AllDirectories);
                foreach (string str in strArray)
                {
                    Log.Info("New zip file detected: " + str);
                    if (action == null)
                    {
                        action = () => this.RaiseOnDiagnosticsDetected(e.FullPath);
                    }
                    Task.Factory.StartNew(action);
                }
            }
            else if (e.Name.EndsWith(".zip") || e.Name.EndsWith(".ZIP"))
            {
                Log.Info("New zip file detected: " + e.FullPath);
                Task.Factory.StartNew(() => this.RaiseOnDiagnosticsDetected(e.FullPath));
            }
        }

        private bool IsFileLocked(string file)
        {
            FileStream stream = null;
            try
            {
                stream = File.OpenRead(file);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return false;
        }

        private void RaiseOnDiagnosticsDetected(string filePath)
        {
            this.CheckAccessibility(filePath);
            if (this.OnDiagnosticsDetected != null)
            {
                this.OnDiagnosticsDetected(filePath);
            }
        }

        public void StartWatch(string directory)
        {
            Log.Info("Starting inspector");
            Task.Factory.StartNew(() => this.CheckExistingFiles(directory));
            this.watcher = new FileSystemWatcher();
            this.watcher.Path = directory;
            this.watcher.IncludeSubdirectories = true;
            this.watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.DirectoryName | NotifyFilters.FileName;
            this.watcher.Created += new FileSystemEventHandler(this.InspectChanges);
            this.watcher.Renamed += new RenamedEventHandler(this.InspectChanges);
            this.watcher.EnableRaisingEvents = true;
        }

        public Action<string> OnDiagnosticsDetected { get; set; }
    }
}

