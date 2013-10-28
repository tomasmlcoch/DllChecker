using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SolarWinds.DLLChecker.Backend
{
    public static class ReportManagers
    {
        
        #region report header background color options
        private const string RED = "#FF0000";
        private const string GREEN = "#33CC33";
        #endregion
        
        public static void ReportPatternCreationFailure(string filePath,string message)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<style>.panel h2{background:"+RED+";}</style>");
            builder.AppendLine("<h1>DLL Checker report - importing pattern</h1>");
            builder.AppendLine("<div class =\"panel\">");
            builder.AppendLine("<h2>Result - Pattern creation FAILED</h2>");
            builder.AppendLine("<div class=\"panelcontent\">");
            builder.Append(message);
            builder.AppendLine("</div>");
            Report(filePath,builder.ToString());
        }

        public static void ReportFailedAnalysis(string filePath, string message)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<style>.panel h2{background:" + RED + ";}</style>");
            builder.AppendLine("<h1>DLL Checker report - diagnostics analysis</h1>");
            builder.AppendLine("<div class =\"panel\">");
            builder.AppendLine("<h2>Result - Analysis FAILED</h2>");
            builder.AppendLine("<div class=\"panelcontent\">");
            builder.Append(message);
            builder.AppendLine("</div>");
            Report(filePath, builder.ToString());
        }

      /*  public static void ReportSuccessfulAnalysis(string filePath,IEnumerable<Module> modules,IEnumerable<Difference> differences)
        {
            string hasPatternString;
            var builder = new StringBuilder();
           builder.AppendLine("<h1>DLL Checker report - diagnostics analysis</h1>");
            builder.AppendLine("<div class =\"panel\">");
            builder.AppendLine("<h2>Modules</h2>");
            builder.AppendLine("<div class=\"panelcontent\">");
            foreach (var module in modules)
            {
                hasPatternString = module.HasPattern != null && !(bool) module.HasPattern ? string.Format(" <font color=\"{0}\">(NO PATTERN FOUND)</font>",RED) : "";

                builder.AppendLine(string.Format("<b>{0} {1}{2}</b><br/>", module.Name, module.Version,hasPatternString));
            }
            builder.AppendLine("</div>");
            builder.AppendLine("</div>");

            builder.AppendLine("<div class =\"panel\">");
            builder.AppendLine("<h2>Defects</h2>");
            builder.AppendLine("<div class=\"panelcontent\">");
            foreach (var difference in differences)
            {
                builder.AppendLine(string.Format("<h3>{0}</h3>",difference.Title));
			    builder.AppendLine(string.Format("<b>Path:</b> {0}</br>",difference.ExpectedFile.Path));
                if (difference.Title!="UNKNOWN FILE")
                {
                    builder.AppendLine(string.Format("<b>Module: </b>{0} {1}</br>", difference.ExpectedFile.Module.Name,difference.ExpectedFile.Module.Version));
                    builder.AppendLine(string.Format("<b>Expected version:</b> {0}</br>", difference.ExpectedFile.Version));
                }
                builder.AppendLine(string.Format("<b>Found version:</b> {0}</br>", difference.FoundVersion));
			 }
            builder.AppendLine("</div>");
            builder.AppendLine("</div>");

            Report(filePath, builder.ToString());
        }*/

        public static void ReportMultipleModules(string filePath, IEnumerable<Module> modules)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<b>Found multiple modules</b></br>");
            builder.AppendLine(
                "<i>When creating new pattern, you must provide diagnostics from fresh installation of a single module.</br>" +
                "If you see listed below module which should't be involved as standalone module please add him to <a href=\"" +
                Path.Combine(Environment.CurrentDirectory, "config", "modules.configuration") +
                                                                     "\"  target=\"_blank\">configuration file</a></i></br>");
            foreach (var module in modules)
            {
                builder.AppendLine(string.Format("{0} {1}<br/>", module.Name, module.Version));
            }
            ReportPatternCreationFailure(filePath,builder.ToString());
        }

       /* public static void ReportPatternCreationSuccess(string filePath,Module module,IEnumerable<AssemblyFile> assemblyFiles)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<style>.panel h2{background:"+GREEN+";}</style>");
            builder.AppendLine("<h1>DLL Checker report - importing pattern</h1>");
            builder.AppendLine("<div class =\"panel\">");
            builder.AppendLine("<h2>Result - Pattern creation SUCCESSFUL</h2>");
            builder.AppendLine("<div class=\"panelcontent\">");
            builder.AppendLine("<b>Imported module</b></br>");
            builder.AppendLine(string.Format("{0} {1}</br></br>", module.Name, module.Version));
            builder.AppendLine("<b>Associated assembly files (Path, Version)</b></br>");
            foreach (var file in assemblyFiles)
            {
                builder.AppendLine(string.Format("{0} {1}<br/>", file.Path, file.Version));
            }
            builder.AppendLine("</div>");
            Report(filePath, builder.ToString());
        }*/

        private static void Report(string filePath, string content)
        {
            
            var file = filePath.Substring(0, filePath.Length - 4) + "_REPORT.html";
            File.WriteAllText(file, Regex.Replace(File.ReadAllText(Path.Combine("models","Report.html")), "@body", content));
        }
    }
}
