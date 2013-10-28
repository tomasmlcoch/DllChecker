namespace SolarWinds.DLLChecker.Backend.DAL
{
    using System;
    using System.Collections.Generic;

    public interface IDiagnosticsManager
    {
        IEnumerable<Module> GetModules();
        string UnpackFile(string relativePath);
        string UnpackFolder(string relativePath);

        string InstallPath { get; set; }

        string WebsitePath { get; set; }
    }
}

