namespace SolarWinds.DLLChecker.Backend.Helpers
{
    using SolarWinds.DLLChecker.Backend.Properties;
    using SolarWinds.Logging;
    using System;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.Text;

    public static class SqlCeHelper
    {
        private static object classLock = new object();
        private static SqlCeConnection connection;
        private static string connectionString = string.Empty;
        private static readonly Log log = new Log();
        private static TimeSpan longRunningQueryTime = TimeSpan.FromSeconds(30.0);
        public static bool nUnits = false;

        public static SqlCeConnection CreateNewDatabaseConnection()
        {
            if (nUnits)
            {
                return OpenNewDatabaseConnection();
            }
            lock (classLock)
            {
                if (connectionString.Length == 0)
                {
                    connectionString = Settings.Default.ConnectionString;
                }
                SqlCeConnection connection = new SqlCeConnection(connectionString);
                connection.Open();
                return connection;
            }
        }

        public static int ExecuteNonQuery(SqlCeCommand command)
        {
            return ExecuteNonQuery(command, null, null);
        }

        public static int ExecuteNonQuery(SqlCeCommand command, SqlCeConnection connection, SqlCeTransaction transaction)
        {
            int num2;
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (connection == null)
            {
                connection = OpenNewDatabaseConnection();
            }
            lock (classLock)
            {
                using (command)
                {
                    command.Connection = connection;
                    if (transaction != null)
                    {
                        command.Transaction = transaction;
                    }
                    LogSql(command);
                    try
                    {
                        DateTime now = DateTime.Now;
                        int num = command.ExecuteNonQuery();
                        LogLongRunningQuery(now, command);
                        num2 = num;
                    }
                    catch (Exception)
                    {
                        log.Error(GetQueryLogString(command));
                        throw;
                    }
                }
            }
            return num2;
        }

        public static IDataReader ExecuteReader(SqlCeCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            SqlCeConnection connection = CreateNewDatabaseConnection();
            command.Connection = connection;
            LogSql(command);
            DateTime now = DateTime.Now;
            IDataReader reader = command.ExecuteReader(nUnits ? CommandBehavior.Default : CommandBehavior.CloseConnection);
            LogLongRunningQuery(now, command);
            return reader;
        }

        internal static SqlCeResultSet ExecuteResultSet(SqlCeCommand command, ResultSetOptions resultSetOptions)
        {
            return ExecuteResultSet(command, resultSetOptions, null);
        }

        internal static SqlCeResultSet ExecuteResultSet(SqlCeCommand command, ResultSetOptions resultSetOptions, SqlCeConnection connection)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (connection == null)
            {
                connection = OpenNewDatabaseConnection();
            }
            lock (classLock)
            {
                command.Connection = connection;
                LogSql(command);
                DateTime now = DateTime.Now;
                SqlCeResultSet set = command.ExecuteResultSet(resultSetOptions);
                LogLongRunningQuery(now, command);
                return set;
            }
        }

        public static object ExecuteScalar(SqlCeCommand command)
        {
            return ExecuteScalar(command, null, null);
        }

        public static object ExecuteScalar(SqlCeCommand command, SqlCeConnection connection)
        {
            return ExecuteScalar(command, connection, null);
        }

        public static object ExecuteScalar(SqlCeCommand command, SqlCeConnection connection, SqlCeTransaction transaction)
        {
            object obj3;
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (connection == null)
            {
                connection = OpenNewDatabaseConnection();
            }
            lock (classLock)
            {
                using (command)
                {
                    command.Connection = connection;
                    if (transaction != null)
                    {
                        command.Transaction = transaction;
                    }
                    LogSql(command);
                    try
                    {
                        DateTime now = DateTime.Now;
                        object obj2 = command.ExecuteScalar();
                        LogLongRunningQuery(now, command);
                        obj3 = obj2;
                    }
                    catch
                    {
                        log.Error(GetQueryLogString(command));
                        throw;
                    }
                }
            }
            return obj3;
        }

        private static string GetQueryLogString(SqlCeCommand command)
        {
            string str = "";
            if ((command.Parameters != null) && (command.Parameters.Count > 0))
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(" (");
                foreach (SqlCeParameter parameter in command.Parameters)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.AppendFormat("{0}={1}", parameter.ParameterName, parameter.Value);
                }
                builder.Append(")");
                str = builder.ToString();
            }
            return (command.CommandText + str);
        }

        public static SqlCeCommand GetTextCommand(string sqlText)
        {
            return new SqlCeCommand { CommandType = CommandType.Text, CommandText = sqlText, CommandTimeout = 0 };
        }

        private static void LogLongRunningQuery(DateTime startTime, SqlCeCommand command)
        {
            if (log.IsWarnEnabled)
            {
                TimeSpan span = (TimeSpan) (DateTime.Now - startTime);
                if (span > LongRunningQueryTime)
                {
                    string queryLogString = GetQueryLogString(command);
                    log.WarnFormat("Long Running Query took {1} seconds: {0}", queryLogString, (int) span.TotalSeconds);
                }
            }
        }

        private static void LogSql(SqlCeCommand command)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("SQL: {0}", GetQueryLogString(command));
            }
        }

        public static SqlCeConnection OpenNewDatabaseConnection()
        {
            lock (classLock)
            {
                if (connection != null)
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }
                    return connection;
                }
                if (connectionString.Length == 0)
                {
                    connectionString = Settings.Default.ConnectionString;
                }
                connection = new SqlCeConnection(connectionString);
                connection.Open();
                return connection;
            }
        }

        public static void Verify()
        {
            lock (classLock)
            {
                if (connectionString.Length == 0)
                {
                    connectionString = Settings.Default.ConnectionString;
                }
                using (new SqlCeEngine(connectionString))
                {
                }
            }
        }

        public static TimeSpan LongRunningQueryTime
        {
            get
            {
                return longRunningQueryTime;
            }
            set
            {
                longRunningQueryTime = value;
            }
        }
    }
}

