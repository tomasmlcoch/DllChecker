using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using SolarWinds.Logging;

namespace SolarWinds.DLLChecker.Backend
{
    internal class PatternsDbManager
    {
        private readonly string connectionString;
        private readonly Log Log = new Log();

        public PatternsDbManager()
        {
            connectionString = backend.Properties.Settings.Default.ConnectionString;
        }


        internal void CreatePattern(Module module, IEnumerable<AssemblyFile> dllFiles)
        {

            Log.Info(string.Format("Creating pattern for module: {0}, version: {1}", module.Name, module.Version));

            if (GetModuleId(module) != -1)
            {
                Log.Error(string.Format("Module pattern {0} {1} already exists", module.Name, module.Version));
                throw new Exception(string.Format("Module pattern {0} {1} already exists", module.Name, module.Version));
            }

            InsertModuleRecord(module);

            var moduleId = GetModuleId(module);

            foreach (var dllFile in dllFiles)
            {
                InsertFileRecord(moduleId, dllFile);
            }


            Log.Info("Pattern created");

        }

        private DataTable GetRecord(SqlCeCommand command)
        {
            var dataTable = new DataTable();
            using (var connection = new SqlCeConnection(connectionString))
            {
                command.Connection = connection;
                var adapter = new SqlCeDataAdapter(command);
                adapter.Fill(dataTable);
            }
            return dataTable;
        }

        private long GetModuleId(Module module)
        {
            var command = new SqlCeCommand("Select ID from [Modules] Where Name=@Name and Version=@Version");
            command.Parameters.AddWithValue("@Name", module.Name);
            command.Parameters.AddWithValue("@Version", module.Version);

            var dataTable = GetRecord(command);

            if (dataTable.Rows.Count == 0)
            {
                return -1;
            }
            return long.Parse(dataTable.Rows[0][0].ToString());
        }

        public IEnumerable<Module> GetAllModules()
        {
            var result = new List<Module>();
            var command = new SqlCeCommand("Select * from [Modules]");
            var dataTable = GetRecord(command);
            Module module;

            foreach (DataRow row in dataTable.Rows)
            {
                module = new Module();
                module.Name = row[1].ToString();
                module.Version = row[2].ToString();

                result.Add(module);
            }

            return result;
        }

        public IEnumerable<AssemblyFile> GetDllFiles(Module module)
        {
            long moduleId = 0;

            moduleId = GetModuleId(module);
            if (moduleId == -1)
            {
                Log.Warn(string.Format("No pattern for module {0} {1}", module.Name, module.Version));
                return new List<AssemblyFile>();

            }


            var command = new SqlCeCommand("Select * from [Files] Where ModuleID=@ModuleID");
            command.Parameters.AddWithValue("@ModuleID", moduleId);


            var dataTable = GetRecord(command);

            var result = (from DataRow row in dataTable.Rows
                          let dllFile = new AssemblyFile()
                          select new AssemblyFile() {Path = row[2].ToString(), Version = row[3].ToString(), Module = module})
                .ToList();

            return result;
        }

        private void ExecuteCommand(string commandString)
        {
            var connection = new SqlCeConnection(connectionString);
            var command = new SqlCeCommand(commandString, connection);
            connection.Open();
            try
            {
                command.ExecuteNonQuery();

            }
            finally
            {
                connection.Close();
            }

        }

        internal void DeletePattern(Module module)
        {
            var moduleId = GetModuleId(module);
            if (moduleId == -1)
            {
                return;

            }
            var command = string.Format("Delete from [Files] Where ModuleID='{0}'", moduleId);
            ExecuteCommand(command);
            command = string.Format("Delete from [Modules] Where Name='{0}' and Version='{1}'", module.Name,
                                    module.Version);
            ExecuteCommand(command);
        }

        internal void DeleteAllPatterns()
        {
            var command = "Delete from [Files]";
            ExecuteCommand(command);
            command = "Delete from [Modules]";
            ExecuteCommand(command);
        }
        
        private void InsertModuleRecord(Module module)
        {
            var commandString = string.Format("Insert into [Modules] (Name,Version) values ('{0}','{1}')", module.Name, module.Version);
            ExecuteCommand(commandString);
        }

        private void InsertFileRecord(long moduleId, AssemblyFile assemblyFile)
        {
            var commandString = string.Format("Insert into [Files] (ModuleID,Path,Version) values ('{0}','{1}','{2}')",
                                              moduleId, assemblyFile.Path, assemblyFile.Version);
            ExecuteCommand(commandString);
        }
       
    }
}
