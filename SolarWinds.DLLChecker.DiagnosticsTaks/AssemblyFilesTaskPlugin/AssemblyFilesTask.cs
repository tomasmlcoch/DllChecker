namespace SolarWinds.DLLChecker.DiagnosticsTasks.AssemblyFilesTaskPlugin
{
    using SolarWinds.DLLChecker.Backend;
    using SolarWinds.DLLChecker.Backend.DAL;
    using SolarWinds.DLLChecker.Backend.Helpers;
    using SolarWinds.DLLChecker.DiagnosticsTasksContract;
    using SolarWinds.Logging;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    [Export(typeof(IDiagnosticsTask))]
    public class AssemblyFilesTask : IDiagnosticsTask
    {
        private static readonly SolarWinds.Logging.Log Log = new SolarWinds.Logging.Log();

        public String GetTaskName() {return "AssemblyFiles Task"; }

        public int CompareVersions(string version1, string version2)
        {
            Version version;
            Version version3;
            if (this.ParseVersions(version1, version2, out version, out version3))
            {
                return version.CompareTo(version3);
            }
            string[] strArray = version1.Split(new char[] { ' ' });
            string[] strArray2 = version1.Split(new char[] { ' ' });
            if (this.ParseVersions(strArray[0], strArray2[0], out version, out version3))
            {
                return version.CompareTo(version3);
            }
            return string.Compare(version1, version2, StringComparison.Ordinal);
        }

        public ReportMessage CompareWithPattern(IDiagnosticsManager manager, IPatternsRepository repository)
        {
            string str;
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            ReportMessage message = new ReportMessage {
                Title = this.TaskType,
                Type = ReportMessage.ReportType.Message,
                ShortExplanation = "Differencies between assembly files in diagnostics and patterns"
            };
            Module[] modules = manager.GetModules().ToArray<Module>();
            Dictionary<string, AssemblyFile> dictionary = this.GetAssemblyFiles(manager).ToDictionary<AssemblyFile, string>(f => f.Path);
            IDictionary<string, AssemblyFile> pattern = this.GetPattern(modules, repository);
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, AssemblyFile> pair in pattern)
            {
                AssemblyFile file;
                if (dictionary.TryGetValue(pair.Value.Path, out file))
                {
                    if (this.CompareVersions(pair.Value.Version, file.Version) > 0)
                    {
                        str = pair.Value.Path.Replace(@"INSTALLPATH\", manager.InstallPath).Replace(@"WEBSITEPATH\", this.GetWebsitePath(manager));
                        builder.AppendLine("<br><b>WRONG VERSION</b><br/>");
                        builder.AppendLine(string.Format("<b>Path:</b> {0}<br/>", str));
                        builder.AppendLine(string.Format("<b>Expected version:</b> {0}<br/>", pair.Value.Version));
                        builder.AppendLine(string.Format("<b>Found version:</b> {0}<br/>", file.Version));
                    }
                    dictionary.Remove(file.Path);
                }
                else
                {
                    str = pair.Value.Path.Replace(@"INSTALLPATH\", manager.InstallPath).Replace(@"WEBSITEPATH\", this.GetWebsitePath(manager));
                    builder.AppendLine("<br><b>MISSING FILE</b><br/>");
                    builder.AppendLine(string.Format("<b>Path:</b> {0}<br/>", str));
                    builder.AppendLine(string.Format("<b>Version:</b> {0}<br/>", pair.Value.Version));
                }
            }
            if (dictionary.Count != 0)
            {
                builder.AppendLine("<style> .uf{display: none } </style>");
                builder.AppendLine("<script>function showUnknownFiles(){ var display = document.getElementById('unknownFiles').style.display;\r\n                if (display != \"block\")\r\n                    document.getElementById('unknownFiles').style.display = \"block\";\r\n                else\r\n                    document.getElementById('unknownFiles').style.display = \"none\";\r\n                }\r\n                </script>");
                builder.AppendLine(string.Format("<a href=\"#\" onclick=\"showUnknownFiles()\">{0} unknown files</a>", dictionary.Count));
                builder.AppendLine(string.Format("<div class=\"uf\" id=\"unknownFiles\">", new object[0]));
                foreach (KeyValuePair<string, AssemblyFile> pair2 in dictionary)
                {
                    str = pair2.Value.Path.Replace(@"INSTALLPATH\", manager.InstallPath).Replace(@"WEBSITEPATH\", this.GetWebsitePath(manager));
                    builder.AppendLine("<br><b>UNKNOWN FILE</b><br/>");
                    builder.AppendLine(string.Format("<b>Path:</b> {0}<br/>", str));
                    builder.AppendLine(string.Format("<b>Version:</b> {0}<br/>", pair2.Value.Version));
                }
                builder.AppendLine(string.Format("</div>", new object[0]));
            }
            message.Message = builder.ToString();
            return message;
        }

        public ReportMessage CreatePattern(IDiagnosticsManager manager, IPatternsRepository repository)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            ReportMessage message = new ReportMessage {
                Title = this.TaskType
            };
            Module module = manager.GetModules().First<Module>();
            List<AssemblyFile> objectGraph = this.GetAssemblyFiles(manager).ToList<AssemblyFile>();
            if (!repository.Save(module, this.TaskType, SerializeHelper.Serialize<List<AssemblyFile>>(objectGraph)))
            {
                throw new Exception("Pattern creation failed. Cannot save pattern.");
            }
            message.ShortExplanation = string.Format("Assembly files associated with module {0} {1}", module.Name, module.Version);
            StringBuilder builder = new StringBuilder();
            foreach (AssemblyFile file in objectGraph)
            {
                builder.AppendLine(string.Format("<br/><b>Path:</b> {0}<br/>", file.Path));
                builder.AppendLine(string.Format("<b>Version:</b> {0}<br/>", file.Version));
            }
            message.Message = builder.ToString();
            message.Type = ReportMessage.ReportType.Success;
            return message;
        }

        private IEnumerable<AssemblyFile> GetAssemblyFiles(IDiagnosticsManager manager)
        {
            bool flag = true;
            string str = null;
            List<AssemblyFile> list = new List<AssemblyFile>();
            string websitePath = this.GetWebsitePath(manager);
            List<string> list2 = File.ReadLines(manager.UnpackFile("SystemInformation/AssemblyInfo.csv")).ToList<string>();
            foreach (string str4 in list2)
            {
                try
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else if (!string.IsNullOrEmpty(str4))
                    {
                        string[] strArray = str4.Split(new char[] { ',' });
                        if (str4.StartsWith("\"Root Folder\""))
                        {
                            str = strArray[1].Replace("\"", "");
                            if (manager.InstallPath == null)
                            {
                                manager.InstallPath = str;
                            }
                            str = str.Replace(manager.InstallPath, @"INSTALLPATH\");
                            str = str.Replace(websitePath, @"WEBSITEPATH\");
                            if (str.Contains(@"Program Files (x86)\Common Files"))
                            {
                                str = "WinDisc" + str.Substring(1);
                            }
                        }
                        else
                        {
                            AssemblyFile item = new AssemblyFile {
                                Path = str + strArray[0],
                                Version = strArray[1]
                            };
                            list.Add(item);
                        }
                    }
                }
                catch
                {
                    Log.Warn("'" + str4 + "' cannot be parsed!");
                }
            }
            return list;
        }

        private IDictionary<string, AssemblyFile> GetPattern(IEnumerable<Module> modules, IPatternsRepository repository)
        {
            Dictionary<string, AssemblyFile> dictionary = new Dictionary<string, AssemblyFile>();
            foreach (Module module in modules)
            {
                string serializedObject = repository.Load(module, this.TaskType);
                if (serializedObject != string.Empty)
                {
                    List<AssemblyFile> list = SerializeHelper.Deserialize<List<AssemblyFile>>(serializedObject);
                    foreach (AssemblyFile file in list)
                    {
                        if (dictionary.ContainsKey(file.Path))
                        {
                            if (this.CompareVersions(file.Version, dictionary[file.Path].Version) > 0)
                            {
                                dictionary[file.Path].Version = file.Version;
                                dictionary[file.Path].Module = module;
                            }
                        }
                        else
                        {
                            file.Module = module;
                            dictionary.Add(file.Path, file);
                        }
                    }
                }
            }
            return dictionary;
        }

        private string GetWebsitePath(IDiagnosticsManager manager)
        {
            if (manager.WebsitePath == null)
            {
                string[] strArray = File.ReadAllLines(manager.UnpackFile("Registry/SolarWindsNet.csv"));
                foreach (string str2 in strArray)
                {
                    if (str2.Contains("Web Root Dir"))
                    {
                        manager.WebsitePath = str2.Split(new char[] { ',' })[2];
                        return manager.WebsitePath;
                    }
                }
                throw new Exception("Web Root Dir information not found");
            }
            return manager.WebsitePath;
        }

        private bool ParseVersions(string version1, string version2, out Version ver1, out Version ver2)
        {
            if (Version.TryParse(version1, out ver1) && Version.TryParse(version2, out ver2))
            {
                return true;
            }
            ver2 = null;
            return false;
        }

        public string TaskType
        {
            get
            {
                return "Assembly files";
            }
        }
    }
}

