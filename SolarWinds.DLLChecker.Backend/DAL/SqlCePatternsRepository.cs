namespace SolarWinds.DLLChecker.Backend.DAL
{
    using SolarWinds.DLLChecker.Backend;
    using SolarWinds.DLLChecker.Backend.Helpers;
    using SolarWinds.DLLChecker.Backend.Properties;
    using SolarWinds.Logging;
    using System;
    using System.Data.SqlServerCe;

    public class SqlCePatternsRepository : IPatternsRepository
    {
        private readonly string connectionString = Settings.Default.ConnectionString;
        private readonly SolarWinds.Logging.Log Log = new SolarWinds.Logging.Log();

        public bool CreateModuleRecord(Module module)
        {
            if (this.GetModuleId(module) != -1L)
            {
                this.Log.Warn(string.Format("Module {0} {1} is already in database", module.Name, module.Version));
                return false;
            }
            using (SqlCeCommand command = SqlCeHelper.GetTextCommand("Insert into [Modules] (Name,Version) values (@Name,@Version)"))
            {
                command.Parameters.AddWithValue("@Name", module.Name);
                command.Parameters.AddWithValue("@Version", module.Version);
                return (SqlCeHelper.ExecuteNonQuery(command) == 1);
            }
        }

        public void DeleteAllModuleRecords(Module module)
        {
            SqlCeCommand command;
            this.Log.Info(string.Format("Deleting all records associated with module {0} {1}", module.Name, module.Version));
            long moduleId = this.GetModuleId(module);
            using (command = SqlCeHelper.GetTextCommand("Delete from [Patterns] Where ModuleID=@ModuleID"))
            {
                command.Parameters.AddWithValue("@ModuleID", moduleId);
                SqlCeHelper.ExecuteNonQuery(command);
            }
            using (command = new SqlCeCommand("Delete from [Modules] Where ID=@ID"))
            {
                command.Parameters.AddWithValue("@ID", moduleId);
                SqlCeHelper.ExecuteNonQuery(command);
            }
        }

        private long GetModuleId(Module module)
        {
            long num;
            using (SqlCeCommand command = SqlCeHelper.GetTextCommand("Select ID from [Modules] Where Name=@Name and Version=@Version"))
            {
                command.Parameters.AddWithValue("@Name", module.Name);
                command.Parameters.AddWithValue("@Version", module.Version);
                using (SqlCeDataReader reader = (SqlCeDataReader) SqlCeHelper.ExecuteReader(command))
                {
                    if (!reader.Read())
                    {
                        return -1L;
                    }
                    num = long.Parse(reader[0].ToString());
                }
            }
            return num;
        }

        public string Load(Module module, string taskTypeId)
        {
            string str;
            long moduleId = this.GetModuleId(module);
            if (moduleId == -1L)
            {
                this.Log.Warn(string.Format("No record for module {0} {1} exists", module.Name, module.Version));
                return string.Empty;
            }
            using (SqlCeCommand command = SqlCeHelper.GetTextCommand("Select Object from [Patterns] Where ModuleID=@ModuleID and TaskType=@TaskType"))
            {
                command.Parameters.AddWithValue("@ModuleID", moduleId);
                command.Parameters.AddWithValue("@TaskType", taskTypeId);
                using (SqlCeDataReader reader = (SqlCeDataReader) SqlCeHelper.ExecuteReader(command))
                {
                    if (!reader.Read())
                    {
                        this.Log.Warn(string.Format("Record for module {0} {1} of type {2} not found", module.Name, module.Version, taskTypeId));
                        return string.Empty;
                    }
                    str = reader[0].ToString();
                }
            }
            return str;
        }

        public bool ModuleRecordExists(Module module)
        {
            return (this.GetModuleId(module) != -1L);
        }

        public bool Save(Module module, string taskTypeId, string pattern)
        {
            long moduleId = this.GetModuleId(module);
            using (SqlCeCommand command = SqlCeHelper.GetTextCommand("Insert into [Patterns] (ModuleID,TaskType,Object) values (@ModuleID,@TaskType,@Pattern)"))
            {
                command.Parameters.AddWithValue("@ModuleID", moduleId);
                command.Parameters.AddWithValue("@TaskType", taskTypeId);
                command.Parameters.AddWithValue("@Pattern", pattern);
                return (SqlCeHelper.ExecuteNonQuery(command) == 1);
            }
        }
    }
}

