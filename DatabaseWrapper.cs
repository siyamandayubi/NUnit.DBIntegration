using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using NUnit.Framework;
using System.Configuration;
using NUnit.DBIntegration.Database;

namespace NUnit.DBIntegration
{
    public static class DatabaseWrapper
    {
        public const string MasterConnectionStringKey = "MasterConnectionString";
        public const string TargetConnectionStringKey = "TargetConnectionString";
        public const string DatabaseNameSettingKey = "DatabaseName";
        public const string AfterDeploymentScriptFilePathSettingKey = "AfterDeploymentScriptFilePath";
        public const string BackupPathSettingKey = "BackupPath";
        public const string ScriptFilesPathSettingKey = "ScriptFilesPath";
        public const string ScriptFilterSettingKey = "ScriptFilter";

        private static DatabaseInitializerOptions _options;
        private static IDatabase _database;
        public static bool Initialized { get; private set; }

        public static void Initialize(DatabaseTypes databaseType)
        {
            DatabaseInitializerOptions options = new DatabaseInitializerOptions();
            options.MasterConnectionString = ConfigurationManager.ConnectionStrings[MasterConnectionStringKey]?.ConnectionString;
            options.DatabaseConnectionString = ConfigurationManager.ConnectionStrings[TargetConnectionStringKey]?.ConnectionString;
            options.DatabaseName = ConfigurationManager.AppSettings[DatabaseNameSettingKey];
            options.BackupPath = ConfigurationManager.AppSettings[BackupPathSettingKey];
            options.ToRunSqlsPath = ConfigurationManager.AppSettings[ScriptFilesPathSettingKey];
            options.SqlFilesFilter = ConfigurationManager.AppSettings[ScriptFilterSettingKey];
            options.AfterRestoreScriptFile = ConfigurationManager.AppSettings[AfterDeploymentScriptFilePathSettingKey];

            Initialize(databaseType, options);
        }

        public static Object RunSqlScalar(string sql)
        {
            using (var connection = _database.GetConnection(_options.DatabaseConnectionString))
            {
                connection.Open();
                var command = _database.GetCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                return command.ExecuteScalar();
            }
        }

        public static void Initialize(DatabaseTypes databaseType, DatabaseInitializerOptions options)
        {
            _options = options;

            try
            {
                _database = Factory.Create(databaseType);
                using (var connection = _database.GetConnection(_options.MasterConnectionString))
                {
                    connection.Open();

                    _database.RestoreDatabase(connection, _options.DatabaseName, _options.BackupPath);

                    if (!string.IsNullOrEmpty(_options.AfterRestoreScriptFile))
                    {
                        _database.RunScript(_options.MasterConnectionString, _options.AfterRestoreScriptFile);
                    }

                    connection.Close();
                }

                using (var connection = _database.GetConnection(_options.DatabaseConnectionString))
                {
                    connection.Open();

                    var files = Directory.GetFiles(_options.ToRunSqlsPath, _options.SqlFilesFilter);
                    files = files.OrderBy(c => Path.GetFileName(c)).ToArray();

                    foreach (var file in files)
                    {
                        try
                        {
                            _database.RunScript(_options.DatabaseConnectionString, file);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to run the file {file}", ex);
                        }
                    }

                    string newPath = Path.Combine(Path.GetDirectoryName(_options.BackupPath), Path.GetFileName(_options.BackupPath) + "_new" + Path.GetExtension(_options.BackupPath));
                    if (File.Exists(newPath))
                    {
                        File.Delete(newPath);
                    }

                    _database.BackupDatabase(connection, _options.DatabaseName, newPath);

                    NewBackup = newPath;

                    connection.Close();
                }

                Initialized = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void RestoreLastbackup()
        {
            if (!Initialized)
            {
                throw new Exception("The database is not initialized");
            }

            _database.RestoreDatabase(_options.MasterConnectionString, _options.DatabaseName, NewBackup);

            if (!string.IsNullOrEmpty(_options.AfterRestoreScriptFile))
            {
                _database.RunScript(_options.MasterConnectionString, _options.AfterRestoreScriptFile);
            }
        }

        public static void RunScript(string filename)
        {
            if (!Initialized)
            {
                throw new Exception("The database is not initialized");
            }

            _database.RunScript(_options.DatabaseConnectionString, filename);
        }

        internal static string NewBackup { get; private set; }
    }
}
