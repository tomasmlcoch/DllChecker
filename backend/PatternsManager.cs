using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using SolarWinds.Logging;

namespace SolarWinds.DLLChecker.Service
{
    class PatternsManager
    {
        private readonly string connectionString;
        private readonly Log Log = new Log();

        public PatternsManager()
        {
            connectionString = @"Data Source=PatternsDatabase.sdf";
        }


        public void CreatePattern(Module module, List<DllFile> dllFiles)
        {

           Log.Info(string.Format("Creating pattern for module: {0}, version: {1}",module.Name,module.Version));
            try
            {
                GetModuleId(module);
            }
            catch (InvalidDataException)
            {
                Log.Error(string.Format("Module pattern {0} {1} already exists", module.Name, module.Version));
                return;
            }

            InsertModuleRecord(module);

            var moduleId = GetModuleId(module);

            foreach (var dllFile in dllFiles)
            {
                InsertFileRecord(moduleId,dllFile);
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
            command.Parameters.AddWithValue("@dName", module.Name);
            command.Parameters.AddWithValue("@Version", module.Version);

            var dataTable = GetRecord(command);

            if (dataTable.Rows.Count == 0)
            {
                
                throw new InvalidDataException("Module record wasn't found");
            }
            return long.Parse(dataTable.Rows[0][0].ToString());
        }

        public List<DllFile> GetDllFiles(Module module)
        {
            long moduleId = GetModuleId(module);
            var command = new SqlCeCommand("Select * from [Files] Where ModuleID=@ModuleID");
            command.Parameters.AddWithValue("@ModuleID", moduleId);
            

            var dataTable = GetRecord(command);

            var result = (from DataRow row in dataTable.Rows let dllFile = new DllFile() select new DllFile() {Path = row[1].ToString(), Version = row[2].ToString()}).ToList();

            return result;
        }

        private void InsertRecord(string commandString)
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

        private void InsertModuleRecord(Module module)
        {
            var commandString = string.Format("Insert) into [Modules] (Name,Version) values ('{0}','{1}')", module.Name, module.Version);
            InsertRecord(commandString);
        }

        private void InsertFileRecord(long moduleId, DllFile dllFile)
        {
            var commandString = string.Format("Insert into [Files] (ModuleID,Path,Version) values ('{0}','{1}','{2}')",
                                              moduleId, dllFile.Path, dllFile.Version);
            InsertRecord(commandString);
        }

        public void Run(Module module, List<DllFile> dllFiles)
        {
            

            /*
            
                con.Open();

                //command = new SqlCeCommand("SET IDENTITY_INSERT IdentityTable ON", con);
                //command.ExecuteNonQuery();
                command = new SqlCeCommand(string.Format("Insert into [Modules] (Name,Version) values ('{0}','{1}')", module.Name, module.Version), con);
                command.ExecuteNonQuery();
                command = new SqlCeCommand(string.Format("select ID from [Modules] Where Name='{0}' and Version='{1}'", module.Name,module.Version), con);
                
                adapter = new SqlCeDataAdapter(command);
                dataTable = new DataTable();
                adapter.Fill(dataTable);
                var moduleId = dataTable.Rows[0][0];


                
                foreach (var dllFile in dllFiles)
                {
                    command = new SqlCeCommand(string.Format("Insert into [Files] (ModuleID,Path,Version) values ('{0}','{1}','{2}')",moduleId,dllFile.Path, dllFile.Version), con);
                    command.ExecuteNonQuery();
                }

                command = new SqlCeCommand(string.Format("select ID from [Files] Where ModuleID='{0}'", moduleId), con);

                adapter = new SqlCeDataAdapter(command);
                dataTable = new DataTable();
                adapter.Fill(dataTable);
                Console.WriteLine(dataTable.Rows.Count);
                Console.ReadKey();


                //con.Close();

                
                
            }


            
            using (var conn = new SqlCeConnection(@"Data Source=PatternsDatabase.sdf"))
            {
                conn.Open();

                 
                var command = new SqlCeCommand(string.Format("Delete From [Modules] ", module.Name, module.Version), conn);
                command.ExecuteNonQuery();
                conn.Close();
            }*/
            
        }

        
    }
}
