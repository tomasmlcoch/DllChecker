namespace SolarWinds.DLLChecker.Service
{
    using SolarWinds.DLLChecker.Backend;
    using SolarWinds.DLLChecker.Backend.DAL;
    using SolarWinds.DLLChecker.DiagnosticsTasksContract;
    using SolarWinds.Logging;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using System.Text;

    public class DiagnosticsTasksManager
    {
        private CompositionContainer container = new CompositionContainer();
        private static readonly SolarWinds.Logging.Log Log = new SolarWinds.Logging.Log();
        [ImportMany(typeof(IDiagnosticsTask))]
        private IEnumerable<IDiagnosticsTask> tasks;

        public DiagnosticsTasksManager()
        {
            DiagnosticsManager.Configure();
        }

        private void Comparison(IDiagnosticsManager manager, IPatternsRepository repository, List<ReportMessage> reportMessages)
        {
            Module[] moduleArray = manager.GetModules().ToArray<Module>();
            ReportMessage item = new ReportMessage {
                Type = ReportMessage.ReportType.Message,
                Title = "Modules",
                ShortExplanation = "Modules found in diagnostics"
            };
            StringBuilder builder = new StringBuilder();
            foreach (Module module in moduleArray)
            {
                string str;
                if (!repository.ModuleRecordExists(module))
                {
                    str = "(NO PATTERN FOUND)";
                    item.Type = ReportMessage.ReportType.Failure;
                }
                else
                {
                    str = "";
                }
                builder.AppendLine(string.Format("<br/><b>{0} {1} <font color=\"red\">{2}</font></b>", module.Name, module.Version, str));
            }
            item.Message = builder.ToString();
            reportMessages.Add(item);
            foreach (IDiagnosticsTask task in this.tasks)
            {
                try
                {
                    Log.Info(task.GetTaskName());
                    reportMessages.Add(task.CompareWithPattern(manager, repository));
                }
                catch (Exception exception)
                {
                    ReportMessage message2 = new ReportMessage {
                        Title = task.TaskType,
                        Type = ReportMessage.ReportType.Failure,
                        ShortExplanation = exception.Message,
                        Message = exception.ToString()
                    };
                    reportMessages.Add(message2);
                    Log.Error("Error in task: " + exception);
                }
            }
        }

        private void CreatePatterns(IDiagnosticsManager manager, IPatternsRepository repository, Module module, List<ReportMessage> reportMessages)
        {
            if (!repository.CreateModuleRecord(module))
            {
                repository.DeleteAllModuleRecords(module);
                if (!repository.CreateModuleRecord(module))
                {
                    throw new InvalidOperationException(string.Format("Can't create module record for {0} {1}", module.Name, module.Version));
                }
            }
            ReportMessage item = new ReportMessage {
                Type = ReportMessage.ReportType.Message,
                Title = "Module",
                ShortExplanation = "Module imported from diagnostics",
                Message = string.Format("<b>Name: </b> {0}<br/><b>Version: </b> {1}", module.Name, module.Version)
            };
            reportMessages.Add(item);
            foreach (IDiagnosticsTask task in this.tasks)
            {
                try
                {
                    reportMessages.Add(task.CreatePattern(manager, repository));
                }
                catch (Exception exception)
                {
                    ReportMessage message2 = new ReportMessage {
                        Title = task.TaskType,
                        Type = ReportMessage.ReportType.Failure,
                        ShortExplanation = exception.Message,
                        Message = exception.ToString()
                    };
                    reportMessages.Add(message2);
                    Log.Error("Error in task: " + exception);
                }
            }
        }

        public void ExecuteTasks(string zipPath, DiagnosticsTaskType type)
        {
            Log.Info("Processing " + zipPath);
            IPatternsRepository repository = new SqlCePatternsRepository();
            IReportManager manager = new HtmlReportManager();
            string reportTitle = null;
            using (DiagnosticsManager manager2 = new DiagnosticsManager(zipPath))
            {
                List<ReportMessage> reportMessages = new List<ReportMessage>();
                try
                {
                    Module[] moduleArray;
                    switch (type)
                    {
                        case DiagnosticsTaskType.Comparison:
                            reportTitle = "Comparison";
                            this.Comparison(manager2, repository, reportMessages);
                            goto Label_0105;

                        case DiagnosticsTaskType.PatternCreation:
                            moduleArray = manager2.GetModules().ToArray<Module>();
                            reportTitle = "Pattern creation";
                            if (moduleArray.Length == 1)
                            {
                                break;
                            }
                            manager.ReportMultipleModules(zipPath, moduleArray);
                            return;

                        default:
                            goto Label_0105;
                    }
                    this.CreatePatterns(manager2, repository, moduleArray.First<Module>(), reportMessages);
                }
                catch (Exception exception)
                {
                    Log.Error("Can't process: " + exception);
                    ReportMessage item = new ReportMessage {
                        Type = ReportMessage.ReportType.Failure,
                        Title = "Error",
                        ShortExplanation = exception.Message,
                        Message = exception.ToString()
                    };
                    reportMessages.Add(item);
                }
            Label_0105:
                manager.Report(zipPath, reportTitle, reportMessages);
            }
        }

        public void LoadTasks()
        {
            DirectoryCatalog catalog = new DirectoryCatalog(".", "*");
            this.container = new CompositionContainer(catalog, new ExportProvider[0]);
            try
            {
                this.container.ComposeParts(new object[] { this });
            }
            catch (CompositionException exception)
            {
                Log.Fatal("Error while loading tasks: " + exception);
                Environment.Exit(-1);
            }
        }
    }
}

