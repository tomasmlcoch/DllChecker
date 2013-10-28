namespace SolarWinds.DLLChecker.Backend.DAL
{
    using SolarWinds.DLLChecker.Backend;
    using SolarWinds.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class DiagnosticsManager : IDiagnosticsManager, IDisposable
    {        
        private bool disposed = false;
        internal static List<string> IGNORED_MODULES;
        private static readonly SolarWinds.Logging.Log Log = new SolarWinds.Logging.Log();
        private IEnumerable<Module> modules;
        internal static Dictionary<string, string> MODULES_ALIASES;
        private string registryFile;
        private readonly string tempDir = Path.Combine(Path.GetTempPath(), "Solarwinds", Path.GetRandomFileName());
        private readonly string zipPath;

        public DiagnosticsManager(string zipPath)
        {
            Log.Info("Created tempdir " + this.tempDir);
            Directory.CreateDirectory(this.tempDir);
            this.zipPath = zipPath;
        }

        public static void Configure()
        {
            ConfigModes nONE = ConfigModes.NONE;
            string[] strArray = File.ReadAllLines(Path.Combine("config", "modules.configuration"));
            foreach (string str in strArray)
            {
                string str2 = str.Trim();
                string str3 = str2;
                if (str3 == null)
                {
                    goto Label_0079;
                }
                if (!(str3 == "Ignored modules"))
                {
                    if (str3 == "Aliases")
                    {
                        goto Label_0068;
                    }
                    goto Label_0079;
                }
                nONE = ConfigModes.IGNORE;
                IGNORED_MODULES = new List<string>();
                continue;
            Label_0068:
                MODULES_ALIASES = new Dictionary<string, string>();
                nONE = ConfigModes.ALIAS;
                continue;
            Label_0079:
                if (!str2.StartsWith("#") && !(str2 == string.Empty))
                {
                    switch (nONE)
                    {
                        case ConfigModes.IGNORE:
                            IGNORED_MODULES.Add(str);
                            break;

                        case ConfigModes.ALIAS:
                        {
                            string[] strArray2 = str2.Split(new char[] { '=' });
                            MODULES_ALIASES.Add(strArray2[0], strArray2[1]);
                            break;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                Directory.Delete(this.tempDir, true);
                this.disposed = true;
            }
        }

        public IEnumerable<Module> GetModules()
        {
            if (this.modules == null)
            {
                var foundModules = new List<Module>();
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    var selection = (from e in archive.Entries
                                     where (e.FullName).EndsWith(".xml")
                                     && (e.FullName).Contains("Modules")
                                     && !(e.FullName).Contains("UpgradeInfo")
                                     select e);

                    Module module;

                    foreach (var entry in selection)
                    {
                        if (IGNORED_MODULES.Any(ignoredModule => entry.FullName.EndsWith(ignoredModule + ".xml")))
                        {
                            continue;

                        }

                        module = new Module();
                        module.Name = Path.GetFileNameWithoutExtension(entry.Name);
                        module.Version = GetModuleVersion(module.Name);
                        foundModules.Add(module);
                    }
                }
                modules = foundModules;
            }
            return this.modules;
        }

        private string GetModuleVersion(string moduleName)
        {
            string str;
            if (this.registryFile == null)
            {
                this.registryFile = this.UnpackFile("Registry/SolarWinds.csv");
            }
            if (!MODULES_ALIASES.TryGetValue(moduleName, out str))
            {
                str = moduleName;
            }
            string[] strArray = File.ReadAllLines(this.registryFile);
            foreach (string str3 in strArray)
            {
                if (str3.Contains(string.Format("{0},Version,", str)))
                {
                    return str3.Split(new char[] { ',' })[2];
                }
            }
            throw new InvalidDataException(string.Format("Module version for {0} not found!", moduleName));
        }

        public string UnpackFile(string relativePath)
        {
            Func<ZipArchiveEntry, bool> predicate = null;
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException("relativePath");
            }
            string destinationFileName = null;
            using (ZipArchive archive = ZipFile.OpenRead(this.zipPath))
            {
                if (predicate == null)
                {
                    predicate = e => e.FullName.Replace('\\', '/').Contains(relativePath);
                }
                IEnumerable<ZipArchiveEntry> source = archive.Entries.Where<ZipArchiveEntry>(predicate);
                ZipArchiveEntry[] entryArray = (source as ZipArchiveEntry[]) ?? source.ToArray<ZipArchiveEntry>();
                if (!entryArray.Any<ZipArchiveEntry>())
                {
                    throw new InvalidDataException(relativePath + " not found");
                }
                destinationFileName = Path.Combine(this.tempDir, entryArray.First<ZipArchiveEntry>().Name);
                entryArray.First<ZipArchiveEntry>().ExtractToFile(destinationFileName);
            }
            return destinationFileName;
        }

        public string UnpackFolder(string relativePath)
        {
            Func<ZipArchiveEntry, bool> predicate = null;
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException("relativePath");
            }
            using (ZipArchive archive = ZipFile.OpenRead(this.zipPath))
            {
                if (predicate == null)
                {
                    predicate = e => e.FullName.Replace('\\', '/').StartsWith(relativePath);
                }
                IEnumerable<ZipArchiveEntry> source = archive.Entries.Where<ZipArchiveEntry>(predicate);
                ZipArchiveEntry[] entryArray = (source as ZipArchiveEntry[]) ?? source.ToArray<ZipArchiveEntry>();
                if (!entryArray.Any<ZipArchiveEntry>())
                {
                    throw new InvalidDataException(relativePath + " not found");
                }
                foreach (ZipArchiveEntry entry in entryArray)
                {
                    if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith(@"\"))
                    {
                        Directory.CreateDirectory(Path.Combine(this.tempDir, entry.FullName));
                    }
                    else
                    {
                        entry.ExtractToFile(Path.Combine(this.tempDir, entry.FullName));
                    }
                }
            }
            return Path.Combine(this.tempDir, relativePath);
        }

        public string InstallPath { get; set; }

        public string WebsitePath { get; set; }

        private enum ConfigModes
        {
            NONE,
            IGNORE,
            ALIAS
        }
    }
}

