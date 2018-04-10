using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NUnit.DBIntegration
{
    internal class SqlServerDatabase : IDatabase
    {
        public DbConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public DbCommand GetCommand()
        {
            return new SqlCommand();
        }

        public void BackupDatabase(string masterConnectionString, string databaseName, string backupPath)
        {
            SqlConnection connection = new SqlConnection(masterConnectionString);
            connection.Open();
            BackupDatabase(connection, databaseName, backupPath);
        }

        public void BackupDatabase(DbConnection connection, string databaseName, string backupPath)
        {
            if (!(connection is SqlConnection))
            {
                throw new ArgumentException("connection must be an instance of SqlConnection");
            }

            SqlCommand command = new SqlCommand();
            command.Connection = (SqlConnection)connection;
            command.CommandType = CommandType.Text;
            string backupSQL = $"BACKUP DATABASE {databaseName} TO DISK = '{backupPath}'";
            command.CommandText = backupSQL;
            command.ExecuteNonQuery();
        }

        public void RestoreDatabase(string masterConnectionString, string databaseName, string backupPath)
        {
            SqlConnection connection = new SqlConnection(masterConnectionString);
            connection.Open();
            RestoreDatabase(connection, databaseName, backupPath);
        }

        public void RestoreDatabase(DbConnection connection, string databaseName, string backupPath)
        {
            if (!(connection is SqlConnection))
            {
                throw new ArgumentException("connection must be an instance of SqlConnection");
            }

            SqlCommand command = new SqlCommand();
            command.Connection = (SqlConnection)connection;
            command.CommandTimeout = 300;
            command.CommandType = CommandType.Text;

            string makeDbSingleUserModelSql = @"
                IF (EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = '{0}' OR name = '{0}')))
                begin
                    ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE
                end";
            makeDbSingleUserModelSql = string.Format(makeDbSingleUserModelSql, databaseName);
            command.CommandText = makeDbSingleUserModelSql;
            command.ExecuteNonQuery();

            string backupSQL = $"RESTORE DATABASE {databaseName} FROM DISK = '{backupPath}'";
            command.CommandText = backupSQL;
            command.ExecuteNonQuery();
        }

        public void RunScript(string connectionString, string filename)
        {
            if (!Path.IsPathRooted(filename))
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                filename = Path.Combine(Path.GetDirectoryName(codeBase), filename);
                filename = filename.StartsWith("file:\\", StringComparison.OrdinalIgnoreCase) ? filename.Substring(6) : filename;
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            // Calls the sqlcmd
            string parameters = string.Empty;
            if (builder.IntegratedSecurity)
            {
                parameters = $"-S {builder.DataSource}";
            }
            else
            {
                parameters = $" -S {builder.DataSource} -U {builder.UserID} -P {builder.Password} -i \"{filename}\"";
            }

            ProcessStartInfo info = new ProcessStartInfo("sqlcmd", parameters);

            //  Indicades if the Operative System shell is used, in this case it is not
            info.UseShellExecute = false;

            //No new window is required
            info.CreateNoWindow = true;

            //The windows style will be hidden
            info.WindowStyle = ProcessWindowStyle.Hidden;

            //The output will be read by the starndar output process
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            Process proc = new Process();

            proc.StartInfo = info;

            //Start the process
            proc.Start();
            proc.WaitForExit();
            string error = proc.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }

            string output = proc.StandardOutput.ReadToEnd();
            if (Regex.IsMatch(output, "Msg\\s"))
            {
                throw new Exception(output);
            }
        }

        public void RunScript(DbConnection connection, string sql)
        {
            if (!(connection is SqlConnection))
            {
                throw new ArgumentException("connection must be an instance of SqlConnection");
            }

            SqlCommand command = new SqlCommand();
            command.Connection = (SqlConnection)connection;
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }
}
