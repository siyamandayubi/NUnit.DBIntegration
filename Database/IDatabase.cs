using System.Data.Common;

namespace NUnit.DBIntegration
{
    internal interface IDatabase
    {
        DbConnection GetConnection(string connectionString);
        DbCommand GetCommand();
        void BackupDatabase(DbConnection connection, string databaseName, string backupPath);
        void BackupDatabase(string masterConnectionString, string databaseName, string backupPath);
        void RestoreDatabase(DbConnection connection, string databaseName, string backupPath);
        void RestoreDatabase(string masterConnectionString, string databaseName, string backupPath);
        void RunScript(DbConnection connection, string sql);
        void RunScript(string connectionString, string filename);
    }
}