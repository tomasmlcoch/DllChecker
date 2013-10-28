namespace SolarWinds.DLLChecker.Backend
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class ReportManagers
    {
        private const string GREEN = "#33CC33";
        private const string RED = "#FF0000";

        private static void Report(string filePath, string content)
        {
            File.WriteAllText(filePath.Substring(0, filePath.Length - 4) + "_REPORT.html", Regex.Replace(File.ReadAllText(Path.Combine("models", "Report.html")), "@body", content));
        }

        public static void ReportFailedAnalysis(string filePath, string message)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<style>.panel h2{background:#FF0000;}</style>");
            builder.AppendLine("<h1>DLL Checker report - diagnostics analysis</h1>");
            builder.AppendLine("<div class =\"panel\">");
            builder.AppendLine("<h2>Result - Analysis FAILED</h2>");
            builder.AppendLine("<div class=\"panelcontent\">");
            builder.Append(message);
            builder.AppendLine("</div>");
            Report(filePath, builder.ToString());
        }

        public static void ReportMultipleModules(string filePath, IEnumerable<Module> modules)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<b>Found multiple modules</b></br>");
            builder.AppendLine("<i>When creating new pattern, you must provide diagnostics from fresh installation of a single module.</br>If you see listed below module which should't be involved as standalone module please add him to <a href=\"" + Path.Combine(Environment.CurrentDirectory, "config", "modules.configuration") + "\"  target=\"_blank\">configuration file</a></i></br>");
            foreach (Module module in modules)
            {
                builder.AppendLine(string.Format("{0} {1}<br/>", module.Name, module.Version));
            }
            ReportPatternCreationFailure(filePath, builder.ToString());
        }

        public static void ReportPatternCreationFailure(string filePath, string message)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<style>.panel h2{background:#FF0000;}</style>");
            builder.AppendLine("<h1>DLL Checker report - importing pattern</h1>");
            builder.AppendLine("<div class =\"panel\">");
            builder.AppendLine("<h2>Result - Pattern creation FAILED</h2>");
            builder.AppendLine("<div class=\"panelcontent\">");
            builder.Append(message);
            builder.AppendLine("</div>");
            Report(filePath, builder.ToString());
        }
    }
}

