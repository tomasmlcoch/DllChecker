using System.Threading.Tasks;
using SolarWinds.DLLChecker.Service;
using SolarWinds.Logging;
using System.IO;
using backend.Properties;

namespace SolarWinds.DLLChecker.Backend
{
    public class DiagnosticsWatcher
    {
        
        private static Log Log = new Log(); 
        
        private FileSystemWatcher watcher;
        
        public void StartWatch()
        {
            Log.Info("Starting inspector");

            watcher = new FileSystemWatcher();
            
            watcher.Path = Settings.Default.DiagnosticsPath;
            watcher.IncludeSubdirectories = true;
           
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
           
            watcher.Created += new FileSystemEventHandler(InspectChanges);
            watcher.Renamed += new RenamedEventHandler(InspectChanges);
        
            // Begin watching.
            watcher.EnableRaisingEvents = true;
            
            
        }
        
        private void InspectChanges(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
                var files = Directory.GetFiles(e.FullPath, "*.zip", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    Log.Info("New zip file detected: " + file);
                    Task.Factory.StartNew(() => StartAnalysis(e.FullPath));

                }
                return;
            }
            else if (!e.Name.EndsWith(".zip") && !e.Name.EndsWith(".ZIP"))
            {
                return;
            }
            Log.Info("New zip file detected: " + e.FullPath);
            Task.Factory.StartNew(() => StartAnalysis(e.FullPath));
         


        }

        private void StartAnalysis(string filePath)
        {
            
            var analyser = new DiagnosticsAnalyser();
            analyser.Analyse(filePath);
        }



        public void EndWatch()
        {
            watcher.EnableRaisingEvents = false;
            Log.Info("Diagnostics watcher stopped");
        }
    }
}
