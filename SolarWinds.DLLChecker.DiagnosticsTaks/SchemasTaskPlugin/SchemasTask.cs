namespace SolarWinds.DLLChecker.DiagnosticsTasks.SchemasTaskPlugin
{
    using SolarWinds.DLLChecker.Backend;
    using SolarWinds.DLLChecker.Backend.DAL;
    using SolarWinds.DLLChecker.Backend.Helpers;
    using SolarWinds.DLLChecker.DiagnosticsTasksContract;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    [Export(typeof(IDiagnosticsTask))]
    public class SchemasTask : IDiagnosticsTask
    {
        public String GetTaskName() { return "Schemas Task"; }

        private string CalculateHash(string filePath)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(File.ReadAllBytes(filePath))).Replace("-", "");
        }

        public ReportMessage CompareWithPattern(IDiagnosticsManager manager, IPatternsRepository repository)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            IEnumerable<Module> modules = manager.GetModules();
            IDictionary<string, IEnumerable<string>> pattern = this.GetPattern(modules, repository);
            List<Schema> source = this.GetSchemas(manager).ToList<Schema>();
            ReportMessage message = new ReportMessage {
                Title = this.TaskType,
                Type = ReportMessage.ReportType.Message,
                ShortExplanation = "Differencies between schemas in diagnostics and patterns"
            };
            StringBuilder builder = new StringBuilder();
            using (IEnumerator<KeyValuePair<string, IEnumerable<string>>> enumerator = pattern.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Func<Schema, bool> predicate = null;
                    KeyValuePair<string, IEnumerable<string>> patternFile = enumerator.Current;
                    Func<string, bool> func = null;
                    if (predicate == null)
                    {
                        predicate = s => s.Path == patternFile.Key;
                    }
                    Schema schema = source.FirstOrDefault<Schema>(predicate);
                    if (schema != null)
                    {
                        if (func == null)
                        {
                            func = s => s != schema.Hash;
                        }
                        if (patternFile.Value.All<string>(func))
                        {
                            builder.AppendLine("<br><b>CORRUPTED SCHEMA</b><br/>");
                            builder.AppendLine(string.Format("<b>Path:</b> {0}<br/>", schema.Path));
                        }
                        source.Remove(schema);
                    }
                    else
                    {
                        builder.AppendLine("<br><b>MISSING SCHEMA</b><br/>");
                        builder.AppendLine(string.Format("<b>Path:</b> {0}<br/>", patternFile.Key));
                    }
                }
            }
            foreach (Schema schema in source)
            {
                builder.AppendLine("<br><b>UNKNOWN SCHEMA</b><br/>");
                builder.AppendLine(string.Format("<b>Path:</b> {0}<br/>", schema.Path));
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
            List<Schema> objectGraph = this.GetSchemas(manager).ToList<Schema>();
            repository.Save(module, this.TaskType, SerializeHelper.Serialize<List<Schema>>(objectGraph));
            message.ShortExplanation = string.Format("Schemas associated with module {0} {1}", module.Name, module.Version);
            StringBuilder builder = new StringBuilder();
            foreach (Schema schema in objectGraph)
            {
                builder.AppendLine(string.Format("<br/><b>Path:</b> {0}<br/>", schema.Path));
            }
            message.Message = builder.ToString();
            message.Type = ReportMessage.ReportType.Success;
            return message;
        }

        private IDictionary<string, IEnumerable<string>> GetPattern(IEnumerable<Module> modules, IPatternsRepository repository)
        {
            Dictionary<string, IEnumerable<string>> dictionary = new Dictionary<string, IEnumerable<string>>();
            foreach (Module module in modules)
            {
                string serializedObject = repository.Load(module, this.TaskType);
                if (serializedObject != string.Empty)
                {
                    List<Schema> list = SerializeHelper.Deserialize<List<Schema>>(serializedObject);
                    foreach (Schema schema in list)
                    {
                        IEnumerable<string> enumerable;
                        List<string> list2;
                        if (dictionary.TryGetValue(schema.Path, out enumerable))
                        {
                            list2 = enumerable.ToList<string>();
                            list2.Add(schema.Hash);
                            dictionary[schema.Path] = list2;
                        }
                        else
                        {
                            list2 = new List<string> {
                                schema.Hash
                            };
                            dictionary[schema.Path] = list2;
                        }
                    }
                }
            }
            return dictionary;
        }

        private IEnumerable<Schema> GetSchemas(IDiagnosticsManager manager)
        {
            string path = manager.UnpackFolder("Information Service");
            string[] strArray = Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories);
            List<Schema> list = new List<Schema>();
            Schema item = null;
            foreach (string str2 in strArray)
            {
                item = new Schema {
                    Path = str2.Replace(path, "Information Servise"),
                    Hash = this.CalculateHash(str2)
                };
                list.Add(item);
            }
            return list;
        }

        public string TaskType
        {
            get
            {
                return "Schemas";
            }
        }
    }
}

