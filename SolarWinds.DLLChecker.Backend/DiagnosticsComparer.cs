using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using SolarWinds.Logging;


namespace SolarWinds.DLLChecker.Backend
{
    public class DiagnosticsComparer
    {
        Log Log = new Log();
        private readonly PatternsManager patternsManager;
        private Exception exception;
        public DiagnosticsComparer()
        {
       
            patternsManager = new PatternsManager();
        }

        public void CheckDiagnostics(string zipPath)
        {
            Log.Info("Processing " + zipPath);

            Dictionary<string, AssemblyFile> files = null;
            IEnumerable<Module> modules = null;
            using (var analyser = new DiagnosticsAnalyser(zipPath))
            {
                try
                {
                    analyser.UnpackFiles();
                    files = analyser.GetFiles().ToDictionary(file => file.Path);
                    modules = analyser.GetModules().ToArray();

                    IDictionary<string, AssemblyFile> dllFilesPattern = patternsManager.GetPatternDictionary(modules);
                    IEnumerable<Difference> diff = Compare(dllFilesPattern, files, analyser.InstallPath, analyser.GetWebsitePath());
                    
                    ReportManager.ReportSuccessfulAnalysis(zipPath,modules,diff);
                    Log.Info(string.Format("{0} successfully analysed", zipPath));
                    return;
                }
                catch (FileNotFoundException ex)
                {
                    exception = ex;
                    Log.Error(string.Format("Diagnostics in file {0} appears corrupted. {1}", zipPath, ex.Message));
                    
                }
                catch (DirectoryNotFoundException ex)
                 {
                    exception = ex;
                   Log.Error(string.Format("Diagnostics in file {0} appears corrupted. {1}", zipPath, ex.Message));
                    
                 }
                 catch (Exception ex)
                  {
                     exception = ex;
                     Log.Error("Can't process: " + ex);
                       
                  }
                string message = string.Format("<b><i>{0}</i></b></br>{1}", exception.Message, exception);
                ReportManager.ReportFailedAnalysis(zipPath, message);
            }
            
        }

        /// <summary>
        /// Compares Assembly files found in diagnostics to patterns in database
        /// </summary>
        /// <param name="patterns">Assembly files taken from patterns database</param>
        /// <param name="files">Assembly files found in diagnostics</param>
        /// <param name="installPath"></param>
        /// <param name="webSitePath"></param>
        /// <returns></returns>
        internal IEnumerable<Difference> Compare(IDictionary<string, AssemblyFile> patterns, IDictionary<string, AssemblyFile> files, string installPath, string webSitePath)
        {
            AssemblyFile assemblyFile;
            string foundVersion = null;
            var result = new List<Difference>();
            var print = false;
            Difference difference = null;

            foreach (var pattern in patterns)
            {
                print = false;
                if (files.TryGetValue(pattern.Key, out assemblyFile))
                {

                    if (patternsManager.CompareVersions(assemblyFile.Version, pattern.Value.Version) < 0)
                    {
                        print = true;
                        difference = new Difference();
                        difference.Title = "WRONG VERSION";
                        foundVersion = assemblyFile.Version;
                    }
                    files.Remove(pattern.Key);
                }
                else
                {
                    difference = new Difference();
                    print = true;
                    difference.Title = "MISSING";
                    foundVersion = "-";

                }

                if (print)
                {
                    difference.ExpectedFile = pattern.Value;
                    difference.ExpectedFile.Path = difference.ExpectedFile.Path.Replace(@"INSTALLPATH\", installPath);
                    difference.ExpectedFile.Path = difference.ExpectedFile.Path.Replace(@"WEBSITEPATH\", webSitePath);
                    difference.FoundVersion = foundVersion;
                    result.Add(difference);
                    
                }
                

            }

            foreach (var file in files)
            {
                difference = new Difference();
                difference.Title = "UNKNOWN FILE";
                difference.ExpectedFile = file.Value;
                difference.ExpectedFile.Path = difference.ExpectedFile.Path.Replace(@"INSTALLPATH\", installPath);
                difference.ExpectedFile.Path = difference.ExpectedFile.Path.Replace(@"WEBSITEPATH\", webSitePath);
                difference.FoundVersion = file.Value.Version;
                result.Add(difference);
            }
            return result;
        }

        private void Report(string diagnosticsFileName,IEnumerable<Module> modules,IEnumerable<Difference> differencies)
        {
            var builder = new StringBuilder();
            builder.AppendLine("############# DLL Checker report  #############");
            builder.AppendLine();
            builder.AppendLine(" -Modules found- ");
            builder.AppendLine();
            foreach (var module in modules)
            {
                builder.Append(string.Format("{0} {1}", module.Name, module.Version));

                if (module.HasPattern != null || module.HasPattern == false)
                {
                    builder.AppendLine(" (NO PATTERN FOUND)");
                }
                else
                {
                    builder.AppendLine();
                }
                
            }
            builder.AppendLine();
            builder.AppendLine(" -Defects- ");
            builder.AppendLine();
            foreach (var difference in differencies)
            {
                builder.AppendLine(difference.Title);
                builder.AppendLine("-------------");
                builder.AppendLine(string.Format("Path: {0}", difference.ExpectedFile.Path));
                builder.AppendLine(string.Format("Module: {0} {1}", difference.ExpectedFile.Module.Name, difference.ExpectedFile.Module.Version));
                builder.AppendLine(string.Format("Expected version: {0}", difference.ExpectedFile.Version));
                builder.AppendLine(string.Format("Found version: {0}", difference.FoundVersion));
                builder.AppendLine(" ");  
                
                
            }


            using (var stream = new FileStream(string.Format("{0}_REPORT.txt", diagnosticsFileName), FileMode.Create))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(builder.ToString());
                    
                }    
            }

            builder.Clear();
        }

       
    }
}
