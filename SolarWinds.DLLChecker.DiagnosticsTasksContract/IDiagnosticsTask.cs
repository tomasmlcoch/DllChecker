using System;
using System.Collections.Generic;
using SolarWinds.DLLChecker.Backend;
using SolarWinds.DLLChecker.Backend.DAL;

namespace SolarWinds.DLLChecker.DiagnosticsTasksContract
{
    public interface IDiagnosticsTask
    {
        string TaskType { get; }

        ReportMessage CreatePattern(IDiagnosticsManager manager, IPatternsRepository repository);
        ReportMessage CompareWithPattern(IDiagnosticsManager manager, IPatternsRepository repository);
        
    }
}
