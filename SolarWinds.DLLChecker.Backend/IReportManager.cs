namespace SolarWinds.DLLChecker.Backend
{
    using System;
    using System.Collections.Generic;

    public interface IReportManager
    {
        void Report(string diagnosticsFilePath, string reportTitle, IEnumerable<ReportMessage> messages);
        void ReportMultipleModules(string diagnosticsFilePath, IEnumerable<Module> modules);
    }
}

