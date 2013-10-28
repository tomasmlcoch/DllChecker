namespace SolarWinds.DLLChecker.Backend.DAL
{
    using SolarWinds.DLLChecker.Backend;
    using System;

    public interface IPatternsRepository
    {
        bool CreateModuleRecord(Module module);
        void DeleteAllModuleRecords(Module module);
        string Load(Module module, string taskTypeId);
        bool ModuleRecordExists(Module module);
        bool Save(Module module, string taskTypeId, string pattern);
    }
}

