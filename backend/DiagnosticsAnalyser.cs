using System.Collections.Generic;
using System.IO;
using System.Linq;
using SolarWinds.Logging;

namespace SolarWinds.DLLChecker.Service
{
    public class DiagnosticsAnalyser
    {
        private readonly Log Log = new Log();

        public void Analyse(string zipPath)
        {
            Log.Info("Analysing " + zipPath);
        }

        private List<DllFile> GetDlls(string assemblyInfoFile)
        {
            string installPath = null;
            const string installPathNormalized = @"INSTALLPATH\";
            string normalizedPath = null;
            var result = new List<DllFile>();
            var lines = File.ReadLines(assemblyInfoFile).ToList();
            string[] linePieces;

            bool firstLine = true;

            foreach (var line in lines)
            {
                if (firstLine)
                {
                    firstLine = false;
                    continue;
                }

                linePieces = line.Split(',');
                if (line.StartsWith("\"Root Folder\""))
                {

                    normalizedPath = linePieces[1].Replace("\"", "");


                    if (installPath == null)
                    {
                        installPath = normalizedPath;
                    }

                    // first path in AssemblyInfo file is installPath
                    normalizedPath = normalizedPath.Replace(installPath, installPathNormalized);
                }
                else
                {
                    var dllFile = new DllFile {Path = normalizedPath + linePieces[0], Version = linePieces[1]};
                    result.Add(dllFile);
                }



            }
            return result;
        }
    }
}
