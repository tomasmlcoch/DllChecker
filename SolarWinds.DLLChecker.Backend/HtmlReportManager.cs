namespace SolarWinds.DLLChecker.Backend
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    public class HtmlReportManager : IReportManager
    {
        private const string GREEN = "#33CC33";
        private const string RED = "#FF0000";

        private void PrintReport(string diagnosticsFilePath, string body, ReportFileIndication indication)
        {
            string str = null;
            switch (indication)
            {
                case ReportFileIndication.NoIssue:
                    str = "NoIssue";
                    break;

                case ReportFileIndication.IssueFound:
                    str = "IssueFound";
                    break;

                case ReportFileIndication.Error:
                    str = "Error";
                    break;
            }
            File.WriteAllText(diagnosticsFilePath.Substring(0, diagnosticsFilePath.Length - 4) + "_" + str + ".html", Regex.Replace(File.ReadAllText(Path.Combine("models", "Report.html")), "@body", body));
        }

        public void Report(string diagnosticsFilePath, string reportTitle, IEnumerable<ReportMessage> messages)
        {
            if (string.IsNullOrEmpty(diagnosticsFilePath))
            {
                throw new ArgumentNullException("diagnosticsFilePath");
            }
            if (string.IsNullOrEmpty(reportTitle))
            {
                throw new ArgumentNullException("reportTitle");
            }
            if (messages == null)
            {
                throw new ArgumentNullException("messages");
            }
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("<h1>DLL Checker report - {0}</h1>", reportTitle));
            ReportFileIndication noIssue = ReportFileIndication.NoIssue;
            foreach (ReportMessage message in messages)
            {
                string str = null;
                if (message.Message == string.Empty)
                {
                    message.Message = "-none-";
                    str = "noIssue";
                }
                if (message.Type == ReportMessage.ReportType.Failure)
                {
                    str = "issue";
                    noIssue = ReportFileIndication.IssueFound;
                }
                else
                {
                    str = "noIssue";
                }
                builder.AppendLine(string.Format("<div class =\"panel {0}\">", str));
                builder.AppendLine(string.Format("<h2  onclick=\"collapse('{0}')\" >{0}</h2>", message.Title));
                builder.AppendLine(string.Format("<div class=\"panelcontent\" id=\"{0}\">", message.Title));
                builder.AppendLine(string.Format("<b>{0}</b><br/>", message.ShortExplanation));
                builder.Append(message.Message);
                builder.AppendLine("</div></div>");
            }
            this.PrintReport(diagnosticsFilePath, builder.ToString(), noIssue);
        }

        public void ReportMultipleModules(string diagnosticsFilePath, IEnumerable<Module> modules)
        {
            if (string.IsNullOrEmpty(diagnosticsFilePath))
            {
                throw new ArgumentNullException("diagnosticsFilePath");
            }
            if (modules == null)
            {
                throw new ArgumentNullException("modules");
            }
            string str = string.Format("<h1>DLL Checker report - Pattern creation</h1>\r\n                                      <div class =\"panelissue\">\r\n                                      <h2 class=\"issue\">Modules</h2>\r\n                                      <div class=\\\"panelcontent\\\">\r\n                                      <b>Found multiple modules</b><br/>\r\n                                      <i>When creating new pattern, you must provide diagnostics from fresh installation of a single module.</br>\r\n                                      If you see listed below module which should't be involved as standalone module please add him to <a href=\"{1}\" \r\n                                      target=\"_blank\">configuration file</a></i></br>", "#FF0000", Path.Combine(Environment.CurrentDirectory, "config", "modules.configuration"));
            StringBuilder builder = new StringBuilder();
            foreach (Module module in modules)
            {
                builder.AppendLine(string.Format("{0} {1}<br/>", module.Name, module.Version));
            }
            builder.AppendLine("</div></div>");
            this.PrintReport(diagnosticsFilePath, str + builder.ToString(), ReportFileIndication.IssueFound);
        }

        private enum ReportFileIndication
        {
            NoIssue,
            IssueFound,
            Error
        }
    }
}

