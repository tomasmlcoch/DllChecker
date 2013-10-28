using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SolarWinds.Logging;
using ZipFile = System.IO.Compression.ZipFile;


namespace SolarWinds.DLLChecker.Backend
{
    public class DiagnosticsAnalyser: IDisposable
    {
        #region modules names 

        internal static Dictionary<string, string> MODULES_ALIASES; 
        internal static List<string> IGNORED_MODULES;
        #endregion

        Log Log = new Log();

        internal string InstallPath { get; set; }

        private enum ConfigModes
        {
            NONE,IGNORE,ALIAS
        }

        private bool configured = false;

        private string tempDir;

        private string zipPath;

        private bool disposed = false;

        public DiagnosticsAnalyser(string _zipPath)
        {
            if (!configured)
            {
                Configure();
            }
            zipPath = _zipPath;
        }

        private static void Configure()
        {
            var mode = ConfigModes.NONE;
            var lines = File.ReadAllLines(Path.Combine("config", "modules.configuration"));
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                switch (trimmedLine)
                {
                    case "Ignored modules": 
                        mode = ConfigModes.IGNORE;
                        IGNORED_MODULES = new List<string>();
                        break;
                    case "Aliases":
                        MODULES_ALIASES = new Dictionary<string, string>();
                        mode = ConfigModes.ALIAS;
                        break;
                    default:
                        if (trimmedLine.StartsWith("#") || trimmedLine == string.Empty)
                        {
                            break;
                        }
                        if (mode == ConfigModes.IGNORE)
                        {
                            IGNORED_MODULES.Add(line);
                        }
                        else if (mode == ConfigModes.ALIAS)
                        {
                            var parts = trimmedLine.Split('=');
                            MODULES_ALIASES.Add(parts[0], parts[1]);
                        }
                        
                        break;
                }
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Directory.Delete(tempDir, true);
                disposed = true;
            }
        }

        internal void UnpackFiles()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "Solarwinds", Path.GetRandomFileName());

            Directory.CreateDirectory(tempDir);

            UnpackFile("SystemInformation/AssemblyInfo.csv");
            UnpackModulesDirectory();
            UnpackFile("Registry/SolarWindsNet.csv");
            UnpackFile("Registry/SolarWinds.csv");

        }

        private void UnpackFile(string name)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {

                var selection = (archive.Entries.Where(e => (e.FullName.Replace('\\','/')).Contains(name)));


                var zipArchiveEntries = selection as ZipArchiveEntry[] ?? selection.ToArray();
                if (!zipArchiveEntries.Any())
                {
                    throw new InvalidDataException(name+" not found");
                }

                zipArchiveEntries.First().ExtractToFile(Path.Combine(tempDir, zipArchiveEntries.First().Name));
            }
        }

       internal IEnumerable<AssemblyFile> GetFiles()
        {
            string assemblyInfoFile = Path.Combine(tempDir, "AssemblyInfo.csv");
            string websitePath = GetWebsitePath();
            const string installPathNormalized = @"INSTALLPATH\";
            const string websitePathNormalized = @"WEBSITEPATH\";
            string normalizedPath = null;
            var result = new List<AssemblyFile>();
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

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                linePieces = line.Split(',');
                if (line.StartsWith("\"Root Folder\""))
                {

                    normalizedPath = linePieces[1].Replace("\"", "");

                    // first path in AssemblyInfo file is installPath
                    if (InstallPath == null)
                    {
                        InstallPath = normalizedPath;
                    }

                    
                    normalizedPath = normalizedPath.Replace(InstallPath, installPathNormalized);
                    normalizedPath = normalizedPath.Replace(websitePath, websitePathNormalized);
                    if (normalizedPath.Contains("Program Files (x86)\\Common Files"))
                    {
                        normalizedPath =  "WinDisc"+normalizedPath.Substring(1);
                    
                    }
                    
                }
                else
                {
                    var dllFile = new AssemblyFile { Path = normalizedPath + linePieces[0], Version = linePieces[1] };

                    result.Add(dllFile);
                }
            }
            return result;
        }

        internal string GetWebsitePath()
        {
            string filePath = Path.Combine(tempDir, "SolarWindsNet.csv");
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.Contains("Web Root Dir"))
                {
                    return line.Split(',')[2];
                }
            }
            throw new Exception("Web Root Dir information not found");
        }

        internal IEnumerable<Module> GetModules()
        {
            string directoryPath = Path.Combine(tempDir, "Modules");

            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException("Modules directory not found");
            }

            var result = new List<Module>();
            var files = Directory.GetFiles(directoryPath, "*.xml");
            Module module;
            foreach (var file in files)
            {
                if (IGNORED_MODULES.Any(ignoredModule => file.EndsWith(ignoredModule + ".xml")))
                {
                    continue;

                }
                module = new Module();
                module.Name = ((Path.GetFileName(file)).Split('.'))[0];
                module.Version = GetModuleVersionFromRegistry(module.Name);
                result.Add(module);
            }
            return result;
        }
        
        private string GetModuleVersionFromRegistry(string moduleName)
        {
            string alias;
            string version = null;
            if (!MODULES_ALIASES.TryGetValue(moduleName,out alias))
            {
                alias = moduleName;
            }
            var lines = File.ReadAllLines(Path.Combine(tempDir,"SolarWinds.csv"));
            foreach (var line in lines)
            {
                if (line.Contains(string.Format("{0},Version,",alias)))
                {
                    version = line.Split(',')[2];
                    return version;
                }
            }

            throw new InvalidDataException(string.Format("Module version for {0} not found!",moduleName));
        }

        private void UnpackModulesDirectory()
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "Modules"));
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                var selection = (from e in archive.Entries
                                 where (e.FullName).EndsWith(".xml")
                                 && (e.FullName).Contains("Modules")
                                 && !(e.FullName).Contains("UpgradeInfo")
                                 select e);
                foreach (var entry in selection)
                {
                    entry.ExtractToFile(Path.Combine(tempDir, "Modules", entry.Name));
                }
            }

        }
    }
}
