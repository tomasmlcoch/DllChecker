using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarWinds.Logging;

namespace SolarWinds.DLLChecker.Backend
{
    public class PatternsManager
    {
        private readonly Log Log = new Log();
        private PatternsDbManager dbManager;

        public PatternsManager()
        {

            dbManager = new PatternsDbManager();
        }

        public void CreatePattern(string filePath, Module module)
        {
            IEnumerable<AssemblyFile> files = null;
            IEnumerable<Module> modules = null;
            using (var analyser = new DiagnosticsAnalyser(filePath))
            {
                {
                    analyser.UnpackFiles();
                    files = analyser.GetFiles();
                    dbManager.CreatePattern(module, files);
                }

            }
        }

        public void CreatePattern(string filePath)
        {
            IEnumerable<AssemblyFile> files = null;
            IEnumerable<Module> modules = null;
            using (var analyser = new DiagnosticsAnalyser(filePath))
            {
                {
                    analyser.UnpackFiles();
                    files = analyser.GetFiles();
                    modules = analyser.GetModules();
                    if (modules.Count() != 1)
                    {
                        Log.Error("Can't create pattern: Multiple modules found in diagnostics");
                        ReportManager.ReportMultipleModules(filePath,modules);
                    }
                    else
                    {
                        try
                        {
                            dbManager.CreatePattern(modules.First(), files);
                            ReportManager.ReportPatternCreationSuccess(filePath, modules.First(), files);
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format("<b><i>{0}</i></b></br>{1}", ex.Message, ex);
                            ReportManager.ReportPatternCreationFailure(filePath,message);
                        }
                        
                    }
                    
                }
            }
        }

        public void DeletePattern(Module module)
        {
            dbManager.DeletePattern(module);
        }

        public IDictionary<string, AssemblyFile> GetPatternDictionary(IEnumerable<Module> modules)
        {
            var result = new Dictionary<string, AssemblyFile>();
            AssemblyFile resultFile;
            foreach (var module in modules)
            {
                var files = dbManager.GetDllFiles(module).ToArray();

                if (files.Length == 0)
                {
                    module.HasPattern = false;
                }

                foreach (var file in files)
                {
                    if (result.ContainsKey(file.Path))
                    {
                        result.TryGetValue(file.Path, out resultFile);
                        if (CompareVersions(resultFile.Version, file.Version) < 0)
                        {
                            result[file.Path] = file;
                        }
                        result.Add(file.Path, file);
                    }
                    else
                    {
                        result.Add(file.Path, file);
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version1"></param>
        /// <param name="version2"></param>
        /// <returns>Returns positive integer if version1 is greater than version2, 0 is versions are equal and negative integer if version1 is less than version2</returns>
        public int CompareVersions(string version1, string version2)
        {
            Version ver1, ver2;
            if (ParseVersions(version1,version2,out ver1,out ver2))
            {
                    return ver1.CompareTo(ver2);
            }
                
            string[] split1 = version1.Split(' ');
            string[] split2 = version1.Split(' ');

            if (ParseVersions(split1[0], split2[0], out ver1, out ver2))
            {
                return ver1.CompareTo(ver2);
            }

            return System.String.Compare(version1, version2, System.StringComparison.Ordinal);

        }

        private bool ParseVersions(string version1,string version2, out Version ver1, out Version ver2)
        {
            if (Version.TryParse(version1, out ver1))
            {
                if (Version.TryParse(version2, out ver2))
                {
                    return true;
                }
            }
            ver2 = null;
            return false;
        }

        public void DeleteAllPatterns()
        {
            dbManager.DeleteAllPatterns();
        }

        public IEnumerable<Module> GetAvailableModules()
        {
            return dbManager.GetAllModules();
            
        }

        public IEnumerable<AssemblyFile> GetPatternDetails(Module module)
        {
           return dbManager.GetDllFiles(module);
            
        }
        
    }
}
